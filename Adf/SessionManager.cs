using System;

namespace Adf
{
    /// <summary>
    /// session manager, default use <see cref="Adf.SessionClient.Instance"/>. configuration SessionServer
    /// </summary>
    public class SessionManager
    {
        static readonly Random random = new Random();
        static readonly Object randomLock = new object();

        /// <summary>
        /// instance create time
        /// </summary>
        public readonly DateTime Now = DateTime.Now;


        SessionClient sessionClient;
        /// <summary>
        /// get session channel, default use <see cref="Adf.SessionClient.Instance"/>. configuration SessionServer
        /// </summary>
        public SessionClient SessionClient
        {
            get { return this.sessionClient; }
        }


        int ttl = 1800;
        /// <summary>
        /// get or set time of live, unit seconds, default 30m, max 30day, set zero for no expired, 
        /// configuration: SessionManager:TTL=1800
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">value must 1s - 30 day</exception>
        public int TTL
        {
            get { return this.ttl; }
            set
            {
                if (value > Adf.SessionClient.MAX_TTL || value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must 1s - 30 day (" + Adf.SessionClient.MAX_TTL + "s).");
                }
                this.ttl = value;
            }
        }

        string sid = null;
        /// <summary>
        /// get or set session id
        /// </summary>
        public string Sid
        {
            get { return this.sid; }
            set { this.sid = value; }
        }

        long uid = 0;
        /// <summary>
        /// get or set login user id
        /// </summary>
        public long Uid
        {
            get { return this.uid; }
            set { this.uid = value; }
        }
                
        string token = null;
        /// <summary>
        /// get or set session token
        /// </summary>
        public string Token
        {
            get { return this.token; }
            set { this.token = value; }
        }

        string userToken = null;
        /// <summary>
        /// get or set user defined token
        /// </summary>
        public string UserToken
        {
            get { return this.userToken; }
            set { this.userToken = value; }
        }

        bool isLogin = false;
        /// <summary>
        /// get or set login status
        /// </summary>
        public bool IsLogin
        {
            get { return this.isLogin; }
            set { this.isLogin = value; }
        }

        string data;
        /// <summary>
        /// get session data
        /// </summary>
        public string Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        /// <summary>
        /// create new instance
        /// </summary>
        public SessionManager()
        {
            this.sessionClient = this.CreateChannel();
            if (this.sessionClient == null)
                throw new NullReferenceException("CreateChannel method return null");
            //
            this.ttl = Adf.ConfigHelper.GetSettingAsInt("SessionManager:TTL", 1800);
        }

        /// <summary>
        /// 判断是否符合会话标识(数字与小写字母)
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual bool IsSessionId(string sessionId)
        {
            if (sessionId == null)
                return false;
            if (sessionId == "")
                return false;

            var match = Adf.ValidateHelper.IsLetterAndNumber(sessionId, LetterCase.Lower);
            return match;
        }


        static char[] DATA_SYMBOL = new char[1] { '|' };

        /// <summary>
        /// verify login
        /// </summary>
        public void VerifyLogin()
        {
            var sid = this.sid;
            var token = this.token;

            if (sid == null || sid == "")
            {
                return;
            }

            if (token == null || token == "")
            {
                return;
            }

            if (this.IsSessionId(sid) == true)
            {
                string data = this.sessionClient.GetData(sid);
                if (data != null)
                {
                    //token|uid|usertoken
                    var items = data.Split(DATA_SYMBOL, 3);
                    if (items.Length == 3 && items[0] == token)
                    {
                        this.data = data;
                        //
                        this.token = token;
                        long.TryParse(items[1], out this.uid);
                        this.userToken = items[2];
                        //
                        this.sessionClient.Refresh(sid, this.ttl);
                        //
                        this.isLogin = true;
                    }
                }
            }
        }

        /// <summary>
        /// create channel instance, default use <see cref="Adf.SessionClient.Instance"/>. use configuration SessionServer
        /// </summary>
        /// <returns></returns>
        protected virtual Adf.SessionClient CreateChannel()
        {
            return Adf.SessionClient.Instance;
        }

        /// <summary>
        /// login a user ,use global ttl
        /// </summary>
        /// <param name="uid"></param>
        /// <exception cref="System.ArgumentOutOfRangeException">id no allow zero</exception>
        /// <returns></returns>
        public bool Login(long uid)
        {
            return this.Login(uid, "");
        }

        /// <summary>
        /// login a user ,use global ttl, override please ensure callbacks
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="userToken">define user token, the value keep current session, usertoken可在会话中保持，通过UserToken属性获取</param>
        /// <exception cref="System.ArgumentOutOfRangeException">id no allow zero</exception>
        /// <exception cref="System.ArgumentNullException">user token allow empty, but not equal null.</exception>
        /// <returns></returns>
        public bool Login(long uid, string userToken)
        {
            if (uid == 0)
            {
                //throw new ArgumentNullException("uid");
                throw new ArgumentOutOfRangeException("uid");
            }

            if (userToken == null)
            {
                throw new ArgumentNullException("userToken", "user token allow empty, but not equal null.");
            }
            //session token
            var token = "";
            lock (randomLock)
            {
                token = random.NextDouble().ToString();
            }
            //session id
            var sid = this.sid;
            if (sid == null || sid == "")
            {
                sid = this.sessionClient.GetSessionId();
            }
            //token|uid|usertoken
            var data = token + "|" + uid + "|" + userToken;
            //
            if (this.LoginBefore(sid, uid, token, userToken) == false)
            {
                return false;
            }
            if (this.sessionClient.AddSession(sid, data, this.ttl) == false)
            {
                return false;
            }
            //reset token & id
            this.token = token;
            this.uid = uid;
            this.data = data;
            this.sid = sid;
            this.isLogin = true;
            //
            this.LoginAfter(sid, uid, token, userToken);
            //
            return true;
        }

        /// <summary>
        /// on login before
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <param name="userToken"></param>
        /// <returns>is allow login</returns>
        protected virtual bool LoginBefore(string sid, long uid, string token, string userToken)
        {
            return true;
        }

        /// <summary>
        /// on login after
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <param name="userToken"></param>
        protected virtual void LoginAfter(string sid, long uid, string token, string userToken)
        {
        }

        /// <summary>
        /// Logout, override please ensure callbacks
        /// </summary>
        /// <returns>is logout success</returns>
        public bool Logout()
        {
            var uid = this.uid;
            var sid = this.sid;
            var token = this.token;
            var userToken = this.userToken;

            //
            if (this.LogoutBefore(sid, uid, token, userToken) == false)
            {
                return false;
            }

            if (this.isLogin == true && this.sid != null)
            {
                this.sessionClient.RemoveSession(this.sid);
            }

            //reset token & uid
            this.token = null;
            this.uid = 0;
            this.data = null;
            this.userToken = null;

            //
            this.LogoutAfter(sid, uid, token, userToken);

            //
            return true;
        }

        /// <summary>
        /// on logout before
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <param name="userToken"></param>
        /// <returns>return is allow logout</returns>
        protected virtual bool LogoutBefore(string sid, long uid, string token, string userToken)
        {
            return true;
        }

        /// <summary>
        /// on logout after
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <param name="userToken"></param>
        protected virtual void LogoutAfter(string sid, long uid, string token, string userToken)
        {
        }

        /// <summary>
        /// get a propose user key name, 获取一个建议的用户键名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetUserKey(string name)
        {
            var key = this.sid + "_" + this.uid + "_" + this.token + "_" + name;
            return key;
        }

        /// <summary>
        /// call for session, must set sid, 为会话调用请求,调用此方法必需已设置Sid
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="Adf.SessionException">no set sid</exception>
        public void Call(Action<Memcache> action)
        {
            var sid = this.sid;
            if (sid == null || sid == "")
            {
                throw new SessionException("no set sid");
            }
            this.sessionClient.Call(sid, action);
        }
    }
}