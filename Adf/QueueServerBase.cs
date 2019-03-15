using System;

namespace Adf
{
    /// <summary>
    /// 队列服务终端基础类， 此业务更建议应用于队列监听，非高性能要求情况下，建议使用HTTP方式进行PUSH, 参考: http://www.xiaobo.li/?p=805
    /// </summary>
    public abstract class QueueServerBase : IDisposable
    {
        string name = null;
        long identity = 0;

        LogWriter logWriter = null;
        /// <summary>
        /// 获取当前实例日志书写器
        /// </summary>
        public LogWriter LogWriter
        {
            get { return this.logWriter; }
        }

        LogManager logManager = null;
        /// <summary>
        /// 获取日志管理器
        /// </summary>
        public LogManager LogManager
        {
            get { return this.logManager; }
        }

        int keepalive = 60;
        /// <summary>
        /// 获取或设置持续保持或重连间隔，单位：秒, 此设置对新连接有效，已有连接不应用。
        /// </summary>
        public int Keepalive
        {
            get { return this.keepalive; }
            set { this.keepalive = value; }
        }

        string[] servers = new string[0];
        /// <summary>
        /// 获取服务器列表
        /// </summary>
        public string[] Servers
        {
            get { return this.servers; }
        }

        Adf.WebSocketClient[] queueClients;
        /// <summary>
        /// 获取队列连接客户端
        /// </summary>
        public Adf.WebSocketClient[] QueueClients
        {
            get { return this.queueClients; }
        }

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <remarks>使用本类需配置同类名相同的服务器配置节点</remarks>
        /// <param name="logManager"></param>
        public QueueServerBase(LogManager logManager)
        {
            //use class name to instance name
            this.name = this.GetType().Name;
            //
            this.logManager = logManager;
            //
            this.logWriter = logManager.GetWriter(this.name);
            //
            this.InitializeQueue();
        }

        /// <summary>
        /// 创建一个当前实例递增的标识
        /// </summary>
        /// <returns></returns>
        public long GenerateID()
        {
            return System.Threading.Interlocked.Increment(ref this.identity);
        }
        
        /// <summary>
        /// 启动客户端监听
        /// </summary>
        /// <exception cref="Adf.QueueServerException">No active client or no configuration servers.</exception>
        public void Start()
        {
            var clients = this.queueClients;
            if (clients == null)
            {
                throw new QueueServerException("No active client or no configuration servers.");
            }
            for (int i = 0; i < clients.Length; i++)
            {
                clients[i].AutoConnect = true;
            }
        }

        /// <summary>
        /// 终止客户端监听
        /// </summary>
        public void Stop()
        {
            var clients = this.queueClients;
            if (clients != null)
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Close();
                }
            }
        }

        /// <summary>
        /// 初始化队列
        /// </summary>
        private void InitializeQueue()
        {
            var cfg = new Adf.Config.ServerConfig(this.name + ".config");
            var items = cfg.GetItems();
            this.servers = new string[items.Length];
            //
            if (this.servers.Length > 0)
            {
                this.queueClients = new Adf.WebSocketClient[items.Length];
                //
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    this.servers[i] = item.Ip + ":" + item.Port;
                    //
                    this.queueClients[i] = new Adf.WebSocketClient(item.Ip, item.Port, "/queue/bin", this.keepalive);
                    //初始化队列处理事件
                    this.queueClients[i].Message += this.QueueClientMessage;
                    this.queueClients[i].Connectioned += this.QueueClientConnectioned;
                    this.queueClients[i].Closed += this.QueueClientClosed;
                    //
                    this.OnInitializeQueueClient(this.queueClients[i]);
                }
            }
        }

        /// <summary>
        /// 当初始化客户端时
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnInitializeQueueClient(WebSocketClient client)
        {
        }

        /// <summary>
        /// 队列已关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void QueueClientClosed(object sender, Adf.WebSocketCloseEventArgs e)
        {
            var client = (Adf.WebSocketClient)sender;
            if (this.logWriter.Enable)
            {
                this.logWriter.WriteTimeLine("{0}:{1} closed, Reason {2}.", client.Host, client.Port, e.Reason);
            }
        }

        /// <summary>
        /// 队列成功连接事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueueClientConnectioned(object sender, EventArgs e)
        {
            var client = (Adf.WebSocketClient)sender;
            if (this.logWriter.Enable)
            {
                this.logWriter.WriteTimeLine("{0}:{1} connected.", client.Host, client.Port);
            }

            this.Connectioned(client);
        }

        /// <summary>
        /// 队列成功连接事件
        /// </summary>
        /// <param name="client"></param>
        public abstract void Connectioned(Adf.WebSocketClient client);

        /// <summary>
        /// 队列新消息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueueClientMessage(object sender, Adf.WebSocketMessageEventArgs e)
        {
            var client = (Adf.WebSocketClient)sender;
            if (e.Opcode == WebSocketOpcode.Binary)
            {
                var result = Adf.QueueServerEncoder.Decode(e.Buffer);
                this.Message(client,result);
            }
        }

        /// <summary>
        /// 收到队列消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        public abstract void Message(WebSocketClient client, QueueServerActionResult result);

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            var clients = this.queueClients;
            if (clients != null)
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Dispose();
                }
            }
        }
    }
}