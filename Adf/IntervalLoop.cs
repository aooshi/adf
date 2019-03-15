using System;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// interval loop
    /// </summary>
    public class IntervalLoop : IDisposable
    {
        bool disposed = false;

        Thread thread = null;
        EventWaitHandle waitEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        EventWaitHandle endEventHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// time arrived
        /// </summary>
        public event EventHandler Arrived;

        int interval = 0;
        int millisecondsTimeout = 0;

        /// <summary>
        /// get or set interval,  value must than or equal zero, unit seconds, default 60s
        /// </summary>
        public int Interval
        {
            get { return this.interval; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must than zero.");
                }

                //
                this.interval = value;
                if (value == 0)
                {
                    this.millisecondsTimeout = System.Threading.Timeout.Infinite;
                }
                else
                {
                    this.millisecondsTimeout = value * 1000;
                }

                //
                this.waitEventHandle.Set();
            }
        }

        /// <summary>
        /// get is disposed
        /// </summary>
        public bool IsDisposed
        {
            get { return this.disposed; }
        }
        
        /// <summary>
        /// initialize new instance, init internval 60s
        /// </summary>
        public IntervalLoop()
        {
            this.Interval = 60;
            this.InitializeThread();
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="interval">inteval</param>
        public IntervalLoop(int interval)
        {
            this.Interval = interval;
            this.InitializeThread();
        }

        private void InitializeThread()
        {
            this.thread = new Thread(this.Processor);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        private void Processor()
        {
            while (this.disposed == false)
            {
                if (this.waitEventHandle.WaitOne(this.millisecondsTimeout) == false)
                {
                    var action = this.Arrived;
                    if (action != null)
                    {
                        action(this, EventArgs.Empty);
                    }
                }
            }

            this.endEventHandle.Set();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            if (this.disposed == false)
            {
                this.disposed = true;
                //
                this.waitEventHandle.Set();
                this.endEventHandle.WaitOne();
                this.waitEventHandle.Close();
                this.endEventHandle.Close();
                //
                this.Arrived = null;
            }
        }
    }
}