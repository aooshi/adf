using System;
using System.Threading;

namespace Adf
{
    /// <summary>
    /// Retry Action
    /// </summary>
    public delegate void RetryAction();

    /// <summary>
    /// Retry Helper
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Retry
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="interval"></param>
        /// <param name="throwIfFail"></param>
        /// <param name="action"></param>
        public static void Retry(int retryCount, TimeSpan interval, bool throwIfFail, RetryAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var i = 0;
            while(true)
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    i++;
                    if (i == retryCount)
                    {
                        if (throwIfFail)
                        {
                            throw;
                        }
                        break;
                    }
                    if (interval > TimeSpan.Zero)
                    {
                        Thread.Sleep(interval);
                    }
                }
            }
        }
    }
}
