using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AdfConsoleTest
{
    public class MqInfo
    {
        public string Name;
        public long Value;
    }


    public class Mq : Adf.MqReceive<MqInfo>
    {
        /// <summary>
        /// 需要具有相应配置节
        /// </summary>
        const string TestName = "MqTest";

        /// <summary>
        /// 最大执行线程数
        /// </summary>
        const int TestMaxThreadSize = 10;

        /// <summary>
        /// 初始化
        /// </summary>
        public Mq()
            : base(TestName, 10, new Adf.LogManager("TestName"))
        {
            base.Logger.ToConsole = true;

            //产生测试数据
            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                this.BuildSend();
            });
        }
        
        protected override void New(MqInfo item)
        {
            Thread.Sleep(100);
            Console.WriteLine("New Receive(Thread:{0},Available:{1}): {2}:{3}", Thread.CurrentThread.ManagedThreadId, base.Mq.GetAvailableThread(), item.Name, item.Value);
        }

        /// <summary>
        /// 生成消息以使接收测试
        /// </summary>
        private void BuildSend()
        {
            while (true)
            {
                if (base.Mq.GetAvailableThread() < TestMaxThreadSize/2)
                {
                    Thread.Sleep(500);
                }
                var v = DateTime.Now.Ticks;
                var mqInfo = new MqInfo()
                {
                    Name = string.Concat("N", v),
                    Value = v
                };
                base.Mq.Send<MqInfo>(mqInfo);
            }
        }
    }
}
