using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

namespace Adf.Mail
{
    /// <summary>
    /// 无代理发送器
    /// </summary>
    public static class MailDeliver
    {
        /// <summary>
        /// mx record. domain,exchange
        /// </summary>
        static Dictionary<string, MXRecordItem> mxrecords = new Dictionary<string, MXRecordItem>(5);
        static Dictionary<string, SmtpClient> clientDictionary = new Dictionary<string, SmtpClient>(5);

        /// <summary>
        /// 无代理发送
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Adf.SmtpException"></exception>
        public static void Send(MailMessage message)
        {
            if (message.To.Count == 1 && message.CC.Count == 0 && message.Bcc.Count == 0)
            {
                MailDeliver.Send(message.To[0].Host, message);
                return;
            }

            //按域分组
            var domainGroup = new Dictionary<string, bool>();
            foreach (MailAddress address in message.To)
            {
                if (!domainGroup.ContainsKey(address.Host))
                    domainGroup.Add(address.Host, false);
            }
            foreach (MailAddress address in message.CC)
            {
                if (!domainGroup.ContainsKey(address.Host))
                    domainGroup.Add(address.Host, false);
            }
            foreach (MailAddress address in message.Bcc)
            {
                if (!domainGroup.ContainsKey(address.Host))
                    domainGroup.Add(address.Host, false);
            }

            //不支持多域
            if (domainGroup.Count > 1)
                throw new SmtpException("no proxy smtp not support multi domain");

            //按域发送
            foreach (KeyValuePair<string, bool> item in domainGroup)
            {
                MailDeliver.Send(item.Key, message);
            }
        }

        /// <summary>
        /// 按域进行无代理发送
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="message"></param>
        /// <exception cref="Adf.SmtpException"></exception>
        public static void Send(string domain, MailMessage message)
        {
            const int port = 25;

            //mx
            MXRecordItem mxe = null;
            DnsRecord record = DnsRecord.EMPTY;
            //
            lock (mxrecords)
            {
                if (mxrecords.TryGetValue(domain, out mxe))
                {
                    try
                    {
                        record = mxe.recordList[mxe.index];
                        if (Environment.TickCount - record.Expired > 0)
                        {
                            //set null, trigger dns query
                            mxe = null;
                        }
                    }
                    catch
                    {
                        mxe = null;
                    }
                }
            }

            //query
            if (mxe == null)
            {
                mxe = new MXRecordItem();
                mxe.recordList = DnsHelper.GetMXRecordList(domain);
                mxe.index = 0;
                if (mxe.recordList == null || mxe.recordList.Count == 0)
                {
                    throw new Adf.SmtpException("dns no found mx record " + domain);
                }

                //
                lock (mxrecords)
                {
                    mxrecords[domain] = mxe;
                }
            }

            Exception firstException;
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    SmtpClient client = null;
                    string clientKey = record.Value + ":" + port;
                    lock (clientDictionary)
                    {
                        if (clientDictionary.TryGetValue(clientKey, out client) == false)
                        {
                            client = new SmtpClient(record.Value, port);
                            clientDictionary.Add(clientKey, client);
                        }
                        else if (client.Connected == false)
                        {
                            client.Dispose();
                            //
                            client = new SmtpClient(record.Value, port);
                            clientDictionary[clientKey] = client;
                        }
                    }
                    //
                    try
                    {
                        client.Send(message);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == 1)
                        {
                            throw;
                        }

                        client.Dispose();
                    }
                }
                //成功发送退出
                return;
            }
            catch (Exception exception)
            {
                //当为 socket 异常时，允许使用其它MX
                if (exception.GetBaseException() is SocketException)
                {
                    if (mxe.recordList.Count == 1)
                        throw;

                    firstException = exception;
                }
                else
                {
                    throw;
                }
            }

            //首选失败后重新选择mx服务器
            for (int i = 0, l = mxe.recordList.Count; i <= l; i++)
            {
                SmtpClient client = null;
                try
                {
                    string clientKey = mxe.recordList[i].Value + ":" + port;
                    lock (clientDictionary)
                    {
                        if (clientDictionary.TryGetValue(clientKey, out client) == false)
                        {
                            client = new SmtpClient(mxe.recordList[i].Value, port);
                            clientDictionary.Add(clientKey, client);
                        }
                        else if (client.Connected == false)
                        {
                            client.Dispose();
                            //
                            client = new SmtpClient(mxe.recordList[i].Value, port);
                            clientDictionary[clientKey] = client;
                        }
                    }
                    //
                    client.Send(message);
                    //发送成功，修改首选
                    mxe.index = i;
                    //发送成功退出
                    return;
                }
                catch (Exception exception)
                {
                    client.Dispose();
                    //
                    if (exception.GetBaseException() is SocketException)
                    {
                        //当为 socket 异常时，允许使用其它MX
                        firstException = exception;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            //当首选服务器为列表中最后一个服务器且重试失败时将首选异常丢出.
            throw firstException;
        }

        class MXRecordItem
        {
            /// <summary>
            /// 所有DNS列表
            /// </summary>
            public List<DnsRecord> recordList;
            /// <summary>
            /// 当前使用的索引序号
            /// </summary>
            public int index;
        }
    }
}