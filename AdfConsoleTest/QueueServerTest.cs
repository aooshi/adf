using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Adf;

namespace AdfConsoleTest
{
    class QueueServerTest
    {
        //const int SIZE = 25 * 10000;

        const int SIZE = 2 * 10000;

        public void Test()
        {
            var host = "";
            host = "127.0.0.1";
            host = "192.168.199.13";
            var port = 231;
            var manual = false;
            var connectionCount = 64;
            ushort commitTimeout = 120;
            var topic = "/test/1";
            //var batchtest = false;
            var enableParallel = false;

            var dictionary = new Dictionary<UInt64, int>(SIZE);
            var count = 0u;
            var client = new Adf.QueueServerClient(host, port, topic, commitTimeout);
            client.Timeout = 30;
            Console.WriteLine("connect {0}:{1}", host, port);
            client.Connect(5);


            Console.WriteLine("topic " + topic);
            Console.WriteLine("subscribe 2 topic ");
            try
            {
                client.Subscribe(topic + "sub1");
                client.Subscribe(topic + "sub2");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }

            var subTopics = client.SubscribeList();
            Console.WriteLine("subscribe list " + string.Join(",", subTopics));

            Console.WriteLine("please key enter");
            Console.ReadLine();

            //--------------------------------------

            Console.WriteLine("lpush 3");
            client.LPush(new byte[0]);
            client.LPush(new byte[0]);
            client.LPush(new byte[0]);
            Console.WriteLine("rpush 3");
            client.RPush(new byte[0]);
            client.RPush(new byte[0]);
            client.RPush(new byte[0]);


            Console.WriteLine("unsubscribe " + topic + "sub1");
            Console.WriteLine("unsubscribe " + topic + "sub2");
            client.Unsubscribe(topic + "sub1");
            client.Unsubscribe(topic + "sub2");

            subTopics = client.SubscribeList();
            Console.WriteLine("subscribe list " + string.Join(",", subTopics));

            Console.WriteLine("please key enter");
            Console.ReadLine();

            Console.WriteLine();
            client.Select(topic + "sub1");
            client.Delete();
            Console.WriteLine(topic + "sub1 delete");
            //
            client.Select(topic + "sub2");
            client.Delete();
            Console.WriteLine(topic + "sub2 delete");


            Console.WriteLine();
            client.Select(topic);
            Console.WriteLine("topic " + client.Topic);


            count = client.Count();
            Console.WriteLine();
            Console.WriteLine("count " + count);


            count = client.Clear();
            Console.WriteLine("clear " + count);


            //--------------------------------------
            this.Push(SIZE, client, dictionary);


            //--------------------------------------
            this.Pull(SIZE, client, dictionary);

            //--------------------------------------

            count = client.Count();
            Console.WriteLine();
            Console.WriteLine("count " + count);

            //--------------------------------------
            this.PushAsync(SIZE * 10, client, dictionary);
            //Console.WriteLine();
            //Console.WriteLine("async pull - commit " + SIZE);
            //this.PullAsync(SIZE, client);
            count = client.Count();
            Console.WriteLine();
            Console.WriteLine("count " + count);
            if (manual)
            {
                Console.WriteLine("key enter continue");
                Console.ReadLine();
            }


            //--------------------------------------
            if (enableParallel)
            {
                this.ParallelPushAndPullCommit(SIZE, client, dictionary);
                count = client.Count();
                Console.WriteLine();
                Console.WriteLine("count " + count);
                if (manual)
                {
                    Console.WriteLine("key enter continue");
                    Console.ReadLine();
                }
            }

            //count = client.Count();
            //Console.WriteLine();
            //Console.WriteLine("count " + count);
            //if (SPLIT)
            //{
            //    Console.WriteLine("key enter continue");
            //    Console.ReadLine();
            //}
            ////--------------------------------------


            //this.ParallelPushAndPullAsync(SIZE, client, dictionary);


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("push " + 20);
            for (int i = 0; i < 20; i++)
            {
                var data = Adf.BaseDataConverter.ToBytes(i);
                var msgid = client.RPush(data);
                dictionary.Add(msgid, i);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("commit 10");

            for (int i = 0; i < 10; i++)
            {
                var msg = client.Pull();
                client.Commit(msg.MessageId);
                Console.WriteLine("pull message id:{0}, duplications:{1}, length: {2}", msg.MessageId, msg.Duplications, msg.Data.Length);
                Console.WriteLine("commit message id:" + msg.MessageId);
            }

            count = client.Count();
            Console.WriteLine();
            Console.WriteLine("count " + count);


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("rollback 10");
            for (int i = 0; i < 10; i++)
            {
                var msg = client.Pull();
                client.Rollback(msg.MessageId);
                Console.WriteLine("pull message id:{0}, duplications:{1}, length: {2}", msg.MessageId, msg.Duplications, msg.Data.Length);
                Console.WriteLine("rollback message id:" + msg.MessageId);
            }


            Console.WriteLine();
            count = client.Count();
            Console.WriteLine("count " + count);


            //check auto rollback
            Console.WriteLine();
            CheckAutoRollback(client);


            Console.WriteLine();
            //if (batchtest)
            //    Console.WriteLine("keep " + threadCount + " connection received-commit " + count);
            //else
            Console.WriteLine("keep " + connectionCount + " thread received-commit " + count);
            

            var stopwatch = Stopwatch.StartNew();
            var receiveCount = 0;
            System.Threading.ThreadPool.QueueUserWorkItem(q =>
            {
                var last = 0;
                while (true)
                {
                    if (receiveCount > 0)
                    {
                        if (count == receiveCount)
                        {

                            Console.WriteLine("completed");
                            
                            client.Dispose();

                            break;
                        }

                        last = receiveCount;
                        Console.WriteLine(last + " - " + (last / (double)(stopwatch.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
                    }

                    System.Threading.Thread.Sleep(2000);
                }
            });

            //if (batchtest)
            //{
            //    batchClient = new QueueServerBatchClient(host, port, topic, commitTimeout);
            //    batchClient.Received += (QueueServerClient client2, QueueServerMessage message) =>
            //    {
            //        var remoteValue = Adf.BaseDataConverter.ToInt32(message.Data);
            //        var localValue = 0;
            //        lock (dictionary)
            //        {
            //            if (dictionary.TryGetValue(message.MessageId, out localValue) == false)
            //            {
            //                localValue = -1;
            //            }
            //        }
            //        //diff value
            //        if (localValue != remoteValue)
            //        {
            //            Console.WriteLine("receive result error, local: {0} -  remote : {1}, dup: {2}, mid: {3}", localValue, remoteValue, message.Duplications, message.MessageId);
            //        }
            //        client2.Commit(message.MessageId);

            //        System.Threading.Interlocked.Increment(ref receiveCount);
            //    };
            //    batchClient.Receive(threadCount);

            //}
            //else

            client.ReceiveError += this.ReceiveErrorCallback;
            client.Receive((QueueServerClient client6, QueueServerMessageAckArgs message) =>
            {
                var remoteValue = Adf.BaseDataConverter.ToInt32(message.Data);
                var localValue = 0;
                lock (dictionary)
                    dictionary.TryGetValue(message.MessageId, out localValue);
                //diff value
                if (localValue != remoteValue)
                {
                    Console.WriteLine("receive result error, local: {0} -  remote : {1}, dup: {2}", localValue, remoteValue, message.Duplications);
                }

                System.Threading.Interlocked.Increment(ref receiveCount);

                return QueueServerReceiveOption.Commit;

            }, connectionCount);
            

            Console.ReadLine();
            //Console.WriteLine();
            //Console.WriteLine("completed");
        }

        private void ReceiveErrorCallback(QueueServerClient client4, QueueServerErrorEventArgs args)
        {
            Console.WriteLine("receive error:" + args.Exception.ToString());
        }

        private void CheckAutoRollback(QueueServerClient client)
        {
            client.Select(client.Topic, 5);

            Console.WriteLine("push one message and pull no commit,wait timeout 10s, check auto rollback");
            //client.RPush("");
            var msg = client.Pull();

            System.Threading.Thread.Sleep(10000);
            try
            {
                client.Rollback(msg.MessageId);

                //成功则表示未自动 rollback， 
                Console.WriteLine("commit timeout error");
            }
            catch
            {
            }
            Console.WriteLine("completed");
        }

        //private void ParallelPushAndPullAsync(int SIZE, QueueServerClient client, Dictionary<ulong, int> dictionary)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("parallel async push - async pull - commit " + SIZE);

        //    System.Threading.ManualResetEvent waitHandle1 = new System.Threading.ManualResetEvent(false);
        //    var stopwatch = Stopwatch.StartNew();
        //    QueueServerPushAck pushAck = (ulong transferId, ulong messageId, QueueServerErrorCode errorCode, string errorMessage) =>
        //    {
        //        lock (dictionary)
        //            dictionary.Add(messageId, (int)transferId);

        //        if ((int)transferId == SIZE - 1)
        //        {
        //            waitHandle1.Set();
        //            stopwatch.Stop();
        //        }
        //    };
        //    client.PushAck += pushAck;
        //    System.Threading.ThreadPool.QueueUserWorkItem(q =>
        //    {
        //        stopwatch.Reset();
        //        stopwatch.Start();
        //        for (uint i = 0; i < SIZE; i++)
        //        {
        //            var data = Adf.BaseDataConverter.ToBytes(i);
        //            client.PushAsync(data, i);
        //        }
        //    });

        //    this.PullAsync(SIZE, client, dictionary);


        //    waitHandle1.WaitOne();
        //    client.PushAck -= pushAck;
        //    Console.WriteLine("push async:" + (SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)) + "/s");
        //}

        private void ParallelPushAndPullCommit(int SIZE, QueueServerClient client, Dictionary<ulong, int> dictionary)
        {
            Console.WriteLine();
            Console.WriteLine("parallel push / pull-commit " + SIZE);
            System.Threading.ManualResetEvent waitHandle1 = new System.Threading.ManualResetEvent(false);
            System.Threading.ThreadPool.QueueUserWorkItem(s =>
            {
                var stopwatch1 = Stopwatch.StartNew();
                stopwatch1.Start();
                for (int i = 0; i < SIZE; i++)
                {
                    var data = Adf.BaseDataConverter.ToBytes(i);
                    var msgid = client.RPush(data);
                    lock (dictionary)
                        dictionary.Add(msgid, i);
                }
                stopwatch1.Stop();
                Console.WriteLine("push:" + (SIZE / (double)(stopwatch1.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
                waitHandle1.Set();
            });

            System.Threading.ManualResetEvent waitHandle2 = new System.Threading.ManualResetEvent(false);
            System.Threading.ThreadPool.QueueUserWorkItem(s =>
            {
                var stopwatch1 = Stopwatch.StartNew();
                stopwatch1.Start();
                for (int i = 0; i < SIZE; i++)
                {
                    var data = client.Pull();
                    var remoteValue = Adf.BaseDataConverter.ToInt32(data.Data);
                    var localValue = 0;
                    lock (dictionary)
                        dictionary.TryGetValue(data.MessageId, out localValue);
                    //diff value
                    if (localValue != remoteValue)
                    {
                        Console.WriteLine("receive result error, local: {0} -  remote : {1}, dup: {2}", localValue, remoteValue, data.Duplications);
                    }
                    client.Commit(data.MessageId);
                }
                stopwatch1.Stop();
                Console.WriteLine("pull:" + (SIZE / (double)(stopwatch1.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
                waitHandle2.Set();
            });

            waitHandle1.WaitOne();
            waitHandle2.WaitOne();
        }

        private void PushAsync(int SIZE, QueueServerClient client, Dictionary<ulong, int> dictionary)
        {
            Console.WriteLine();
            Console.WriteLine("async push " + SIZE);
            System.Threading.ManualResetEvent waitHandle3 = new System.Threading.ManualResetEvent(false);
            //var pushAck = new QueueServerPushAck((ulong transferId, ulong messageId, QueueServerErrorCode errorCode, string errorMessage) =>
            //var pushAck = new EventHandler<QueueServerPushAckArgs>((sender, args) =>
            var pushAck = new QueueServerClientEvent<QueueServerAckArgs>((sender, args) =>
            {
                lock (dictionary)
                    dictionary.Add(args.MessageId, (int)args.TransferId);

                if ((int)args.TransferId == SIZE - 1)
                {
                    waitHandle3.Set();
                }
            });
            client.RPushAck += pushAck;
            var stopwatch = Stopwatch.StartNew();
            for (uint i = 0; i < SIZE; i++)
            {
                var data = Adf.BaseDataConverter.ToBytes(i);
                client.RPushAsync(data, i);
            }
            waitHandle3.WaitOne();
            stopwatch.Stop();
            client.RPushAck -= pushAck;
            Console.WriteLine((SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
        }

        private void Pull(int SIZE, QueueServerClient client, Dictionary<ulong, int> dictionary)
        {
            Console.WriteLine();
            Console.WriteLine("pull and commit " + SIZE);
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < SIZE; i++)
            {
                var data = client.Pull();
                var remoteValue = Adf.BaseDataConverter.ToInt32(data.Data);
                var localValue = 0;
                dictionary.TryGetValue(data.MessageId, out localValue);
                //diff value
                if (localValue != remoteValue)
                {
                    Console.WriteLine("receive result error, local: {0} -  remote : {1}, dup:{2}", localValue, remoteValue, data.Duplications);
                }
                //diff index
                if (i != remoteValue)
                {
                    Console.WriteLine("receive result error, index: {0} -  remote : {1}, dup:{2}", i, remoteValue, data.Duplications);
                }

                client.Commit(data.MessageId);
            }
            stopwatch.Stop();
            Console.WriteLine((SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
        }

        private void Push(int SIZE, QueueServerClient client, Dictionary<ulong, int> dictionary)
        {
            Console.WriteLine();
            Console.WriteLine("push " + SIZE);
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < SIZE; i++)
            {
                var data = Adf.BaseDataConverter.ToBytes(i);
                var msgid = client.RPush(data);
                dictionary.Add(msgid, i);
            }
            stopwatch.Stop();
            Console.WriteLine((SIZE / (double)(stopwatch.ElapsedMilliseconds / 1000)).ToString("0.000") + "/s");
        }

        //private void PullAsync(int size, QueueServerClient client, Dictionary<ulong, int> dictionary)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("pull async - commit " + size + " for 10 request ");
        //    int request = 0;
        //    int response = 0;
        //    bool running = true;
        //    System.Threading.ManualResetEvent waitHandle4 = new System.Threading.ManualResetEvent(false);

        //    System.Threading.ThreadPool.QueueUserWorkItem(s =>
        //    {
        //        while (running)
        //        {
        //            Console.WriteLine("rec: " + response);
        //            System.Threading.Thread.Sleep(2000);
        //        }
        //    });

        //    var ack = new QueueServerPullAck((ulong transferId, QueueServerMessage message, QueueServerErrorCode errorCode, string errorMessage) =>
        //    {
        //        var remoteValue = Adf.BaseDataConverter.ToInt32(message.Data);
        //        var localValue = 0;
        //        dictionary.TryGetValue(message.MessageId, out localValue);
        //        //diff value
        //        if (localValue != remoteValue)
        //        {
        //            Console.WriteLine("receive result error, local: {0} -  remote : {1}, dup: {2}", localValue, remoteValue, message.Duplications);
        //        }

        //        bool isPull = false;
        //        lock (client)
        //        {
        //            response++;
        //            if (response == size)
        //            {
        //                running = false;
        //                waitHandle4.Set();
        //            }


        //            request++;
        //            if (request <= size)
        //            {
        //                isPull = true;
        //            }
        //        }

        //        if (isPull)
        //        {
        //            client.PullAsync(transferId);
        //        }

        //        client.Commit(message.MessageId);
        //    });

        //    client.PullAck += ack;
        //    var stopwatch = Stopwatch.StartNew();
        //    for (uint i = 0; i < 10; i++)
        //    {
        //        request++;
        //        client.PullAsync(i);
        //    }
        //    waitHandle4.WaitOne();
        //    stopwatch.Stop();
        //    client.PullAck -= ack;
        //    Console.WriteLine("rec: " + response);
        //    Console.WriteLine("pull async " + (size / (double)(stopwatch.ElapsedMilliseconds / 1000)) + "/s");
        //}


        //private void PullAsync(int size, QueueServerClient client)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("pull async and commit - " + size + " for 10 request ");
        //    int request = 0;
        //    int response = 0;
        //    bool running = true;
        //    System.Threading.ManualResetEvent waitHandle4 = new System.Threading.ManualResetEvent(false);

        //    System.Threading.ThreadPool.QueueUserWorkItem(s =>
        //    {
        //        while (running)
        //        {
        //            Console.WriteLine("rec: " + response);
        //            System.Threading.Thread.Sleep(2000);
        //        }
        //    });

        //    var ack = new QueueServerPullAck((ulong transferId, QueueServerMessage message, QueueServerErrorCode errorCode, string errorMessage) =>
        //    {
        //        bool isPull = false;
        //        lock (client)
        //        {
        //            response++;
        //            if (response == size)
        //            {
        //                running = false;
        //                waitHandle4.Set();
        //            }


        //            request++;
        //            if (request <= size)
        //            {
        //                isPull = true;
        //            }
        //        }

        //        if (isPull)
        //        {
        //            client.PullAsync(transferId);
        //        }

        //        client.Commit(message.MessageId);

        //    });

        //    client.PullAck += ack;
        //    var stopwatch = Stopwatch.StartNew();
        //    for (uint i = 0; i < 10; i++)
        //    {
        //        request++;
        //        client.PullAsync(i);
        //    }
        //    waitHandle4.WaitOne();
        //    client.PullAck -= ack;
        //    stopwatch.Stop();
        //    Console.WriteLine("rec: " + response);
        //    Console.WriteLine((size / (double)(stopwatch.ElapsedMilliseconds / 1000)) + "/s");
        //}
    }
}