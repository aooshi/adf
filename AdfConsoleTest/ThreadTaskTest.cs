using System;
using System.Collections.Generic;
using System.Text;

namespace AdfConsoleTest
{
    public class ThreadTaskTest
    {
        public void Test()
        {
            int count = 2048;
            var tasks = new Adf.ThreadTasks.TaskCallback[count];

            for (int i = 0; i < count; i++)
            {
                var index = i;
                tasks[index] = (state,index2) =>
                {

                    System.Threading.Thread.Sleep((int)(new Random(i).NextDouble() * 100));
                    lock (tasks)
                        Console.WriteLine("1:"+index);

                };
            }
            Adf.ThreadTasks.ProcessTask(null,tasks);

            Console.WriteLine("completed 1/3");


            //
            count = 2048;
            tasks = new Adf.ThreadTasks.TaskCallback[count];

            for (int i = 0; i < count; i++)
            {
                var index = i;
                tasks[index] = (state,index2) =>
                {

                    System.Threading.Thread.Sleep((int)(new Random(i).NextDouble() * 100));
                    lock (tasks)
                        Console.WriteLine("2:" + index);

                };
            }
            Adf.ThreadTasks.ProcessTask(5, tasks);

            Console.WriteLine("completed 2/3");

            //
            Adf.ThreadTasks.ProcessTask(1024, 5, null, (object o, int i) =>
            {
                lock (tasks)
                    Console.WriteLine("3:" + i);
            });

            Console.WriteLine("completed 3/3");
        }
    }
}