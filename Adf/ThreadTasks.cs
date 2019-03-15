using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

namespace Adf
{
    /// <summary>
    /// 线程任务
    /// </summary>
    public class ThreadTasks
    {
        /// <summary>
        /// 任务执行回调
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        public delegate void TaskCallback(object state, int index);

        /// <summary>
        /// 多线程同步执行一个任务
        /// </summary>
        /// <param name="taskCount"></param>
        /// <param name="threadCount"></param>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        public static void ProcessTask(int taskCount, int threadCount, object state, TaskCallback callback)
        {
            if (threadCount < 2)
            {
                for (int i = 0; i < taskCount; i++)
                {
                    callback(state, i);
                }
            }
            else
            {
                Exception exception = null;
                Object lockObject = new object();
                int running = 0;
                int total = 0;
                //
                for (int i = 0; i < taskCount; i++)
                {
                    lock (lockObject)
                    {
                        running++;
                        if (running == threadCount)
                        {
                            Monitor.Wait(lockObject);
                        }
                    }
                    //
                    var index = i;
                    System.Threading.ThreadPool.QueueUserWorkItem(state2 =>
                    {
                        try { callback(state2, index); }
                        catch (Exception ex) { exception = ex; }
                        //
                        lock (lockObject)
                        {
                            running--;
                            total++;
                            Monitor.Pulse(lockObject);
                        }

                    }, state);
                }

                //
                while (true)
                {
                    lock (lockObject)
                    {
                        if (total == taskCount)
                        {
                            break;
                        }
                        Monitor.Wait(lockObject);
                    }
                }

                //
                if (exception != null)
                    throw exception;
            }
        }
        
        /// <summary>
        /// 多线程同步执行一组任务并待所有任务完成
        /// </summary>
        /// <param name="maxThreadCount"></param>
        /// <param name="callbacks"></param>
        /// <param name="state"></param>
        public static void ProcessTask(int maxThreadCount, object state, TaskCallback[] callbacks)
        {
            if (maxThreadCount < 1)
                throw new ArgumentOutOfRangeException("maxThreadCount", "maxThreadCount must than 0");

            var taskCount = callbacks.Length;
            if (taskCount > 0)
            {
                Exception exception = null;
                Object lockObject = new object();
                int running = 0;
                int total = 0;
                //
                for (int i = 0; i < taskCount; i++)
                {
                    lock (lockObject)
                    {
                        running++;
                        if (running == maxThreadCount)
                        {
                            Monitor.Wait(lockObject);
                        }
                    }
                    //
                    var index = i;
                    System.Threading.ThreadPool.QueueUserWorkItem(state2 =>
                    {
                        try { callbacks[index](state2, index); }
                        catch (Exception ex) { exception = ex; }
                        //
                        lock (lockObject)
                        {
                            running--;
                            total++;
                            Monitor.Pulse(lockObject);
                        }
                    });
                }

                //
                while (true)
                {
                    lock (lockObject)
                    {
                        if (total == taskCount)
                        {
                            break;
                        }
                        Monitor.Wait(lockObject);
                    }
                }

                //
                if (exception != null)
                    throw exception;
            }
        }
        
        /// <summary>
        /// 多线程同步执行一组任务并待所有任务完成，使用此方法执行任务数量不应过大，建议小于64
        /// </summary>
        /// <param name="state"></param>
        /// <param name="callbacks"></param>
        public static void ProcessTask(object state, TaskCallback[] callbacks)
        {
            var taskCount = callbacks.Length;
            if (taskCount > 0)
            {
                Exception exception = null;
                Object lockObject = new object();
                int total = 0;
                //
                for (int i = 0; i < taskCount; i++)
                {
                    var index = i;
                    System.Threading.ThreadPool.QueueUserWorkItem(state2 =>
                    {
                        try { callbacks[index](state2, index); }
                        catch (Exception ex) { exception = ex; }
                        //
                        lock (lockObject)
                        {
                            total++;
                            Monitor.Pulse(lockObject);
                        }
                    });
                }

                //
                while (true)
                {
                    lock (lockObject)
                    {
                        if (total == taskCount)
                            break;

                        Monitor.Wait(lockObject);
                    }
                }

                //
                if (exception != null)
                    throw exception;
            }
        }
    }
}