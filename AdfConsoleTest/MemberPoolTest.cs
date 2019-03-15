using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AdfConsoleTest
{
    public class MemberPoolTest
    {
        const int RUN_COUNT = 10;

        public void Test()
        {
            var total = 0;
            var stopwatch = new Stopwatch();

            
            var memberPool = new Adf.MemberPool<Member>(5);
            memberPool.Creater = () =>
            {
                return new Member(total);
            };

            var random = new Random();
            System.Threading.ThreadPool.QueueUserWorkItem(s => {

                while (total < RUN_COUNT)
                {
                    if (memberPool.AvailableCount == 0)
                    {
                        var count = random.Next(1, memberPool.MaxMemberCount);

                        Console.WriteLine("Full,  discard " + count);
                        Console.WriteLine("MaxMemberCount " + memberPool.MaxMemberCount);
                        Console.WriteLine("AvailableCount " + memberPool.AvailableCount);
                        for (var i = 0; i < count; i++)
                        {
                            memberPool.Discard();
                        }
                    }
                    System.Threading.Thread.Sleep(3000);
                }
            
            });

            stopwatch.Reset();
            stopwatch.Start();
            while (++total != RUN_COUNT)
            {
                var member = memberPool.Get();
                member.Write();
                if (random.Next() % 2 == 1)
                {
                    memberPool.Put(member);
                }
            }
            stopwatch.Stop();
            Console.WriteLine("total:{0}, seconds:{1}, {2} loop/s"
                , total
                , (double)(stopwatch.ElapsedMilliseconds / 1000)
                , total / (double)(stopwatch.ElapsedMilliseconds / 1000)
                );

            Console.WriteLine("AvailableCount: " + memberPool.AvailableCount);
            while (true)
            {
                Console.WriteLine("entry 1 get a member, 2 give a member");
                var k = Console.ReadKey();
                if (k.KeyChar == '1')
                {
                    Console.WriteLine();
                    memberPool.Get();
                    Console.WriteLine("AvailableCount: " + memberPool.AvailableCount);
                }
                else if (k.KeyChar == '2')
                {
                    Console.WriteLine();
                    memberPool.Discard();
                    Console.WriteLine("AvailableCount: " + memberPool.AvailableCount);
                }
            }
        }

        class Member : IDisposable
        {
            public long id;

            public Member(long id)
            {
                this.id = id;
            }

            public void Write()
            {
                Console.WriteLine("M: " + this.id);
            }

            public void Dispose()
            {

            }
        }
    }
}
