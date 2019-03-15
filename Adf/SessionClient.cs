using System;

namespace Adf
{
    /// <summary>
    /// session client
    /// </summary>
    public class SessionClient : MemcachePool
    {
        const int HASH_BUCKET = 255;

        /// <summary>
        /// max ttl (2592000)
        /// </summary>
        public const int MAX_TTL = 60 * 60 * 24 * 30;

        /// <summary>
        /// 获取SessionServer实例,对应配置节SessionServer
        /// </summary>
        public static readonly SessionClient Instance = new SessionClient("SessionServer");
                
        /// <summary>
        /// 根据指定配置初始实例
        /// </summary>
        /// <param name="configName"></param>
        /// <exception cref="Adf.ConfigException"></exception>
        public SessionClient(string configName)
            : base(configName)
        {
        }

        /// <summary>
        /// 获取一个可用的会话标识
        /// </summary>
        /// <exception cref="Adf.SessionException">Create session id failed.</exception>
        /// <returns></returns>
        public virtual string GetSessionId()
        {
            var guid = Guid.NewGuid();
            var id = guid.ToString("N");
            return id;
        }
                
        /// <summary>
        /// add new session, 添加新会话,并指定过期时间
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="data"></param>
        /// <param name="ttl">time of live, unit seconds, default 30m, max 30day, set zero for no expired</param>
        /// <returns>success: true, failure: false</returns>
        /// <exception cref="System.ArgumentNullException">token is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">ttl value invalid</exception>
        public bool AddSession(string sessionId, string data, int ttl)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (ttl > MAX_TTL)
                throw new ArgumentOutOfRangeException("ttl", "ttl value must is 1s - 30 day (" + MAX_TTL + "s).");

            String key = this.GetKey(sessionId);
            Boolean success = true;
            //
            base.Pool.Call(m =>
            {
                if (m.Set(key, data, (long)ttl) == false)
                {
                    success = false;
                }
            }, sessionId, null);
            //
            return success;
        }

        /// <summary>
        /// remove a session, 移除会话
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>find and removed : true, off-line : false</returns>
        public bool RemoveSession(string sessionId)
        {
            String key = this.GetKey(sessionId);
            Boolean success = true;
            //
            base.Pool.Call(m =>
            {
                if (m.Delete(key) == false)
                {
                    success = false;
                }
            }, sessionId, null);
            //
            return success;
        }

        /// <summary>
        /// get session data, 获取会话数据
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>find return session data, else return null</returns>
        public string GetData(string sessionId)
        {
            String key = this.GetKey(sessionId);
            string token = null;
            //
            base.Pool.Call(m =>
            {
                token = m.Get<string>(key);

            }, sessionId, null);

            //
            return token;
        }
        
        /// <summary>
        /// refresh expire time, 刷新过期时间
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="expires">seconds or unix-timestamp</param>
        /// <returns>success: true, off-line : false</returns>
        public bool Refresh(string sessionId, int expires)
        {
            String key = this.GetKey(sessionId);
            Boolean success = true;
            //
            base.Pool.Call(m =>
            {
                if (m.Touch(key, expires) == false)
                {
                    success = false;
                }
            }, sessionId, null);
            //
            return success;
        }

        /// <summary>
        /// get session store key, 获取会话存储键
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual string GetKey(string sessionId)
        {
            String key = "session" + sessionId;
            return key;
        }

        /// <summary>
        /// callback for session, 为一个会话调用一个请求
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="action"></param>
        public void Call(string sessionId, Action<Memcache> action)
        {
            base.Pool.Call(action, sessionId, null);
        }
    }

    /// <summary>
    /// Session exception
    /// </summary>
    public class SessionException : Exception
    {
        /// <summary>
        /// session
        /// </summary>
        /// <param name="message"></param>
        public SessionException(string message)
            : base(message)
        {

        }
    }
}