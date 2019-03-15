using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Adf
{
    /// <summary>
    /// DNS HELPER
    /// </summary>
    public class DnsHelper
    {
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public static bool CACHE_ENABL = true;
        /// <summary>
        /// 查询超时时间（毫秒）
        /// </summary>
        public static int QUERY_TIMEOUT = 2000;

        static Dictionary<string, List<DnsRecord>> cacheDictionary = new Dictionary<string, List<DnsRecord>>(8);

        private static List<string> queryServers = new List<string>(5);
                
        /// <summary>
        /// 获取MX记录列表
        /// </summary>
        /// <param name="domain"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>dns failure, return null , else return values,结果已按优先级排序</returns>
        public static List<DnsRecord> GetMXRecordList(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domain");

            List<DnsRecord> records = null;
            if (CACHE_ENABL == true)
            {
                records = GetCacheRecords(domain, "mx");
                if (records != null)
                {
                    var now = Environment.TickCount;
                    for (int i = records.Count - 1; i >= 0; i--)
                    {
                        var record = records[i];
                        if (now - record.Expired > 0)
                        {
                            //set expired
                            records = null;
                            break;
                        }
                    }
                }
            }

            //
            if (records == null)
            {
                int l = queryServers.Count;
                do
                {
                    var server = GetServer();
                    if (server == null)
                    {
                        break;
                    }
                    //
                    var query = new DnsQuery(server);
                    try
                    {
                        query.QueryTimeout = QUERY_TIMEOUT;
                        records = query.QueryMX(domain);
                    }
                    catch
                    {
                        ServerExchange();
                        continue;
                    }

                    if (records == null || records.Count == 0)
                    {
                        records = null;
                        break;
                    }

                    //sort
                    records.Sort((a, b) =>
                    {
                        return a.Preference - b.Preference;
                    });

                    break;

                } while (--l > 0);

                //
                if (CACHE_ENABL == true && records != null)
                {
                    SetCacheRecords(domain, "mx", records);
                }
            }

            return records;
        }

        private static List<DnsRecord> GetCacheRecords(string domain, string type)
        {
            string key = domain + "_" + type;
            List<DnsRecord> records = null;
            lock (cacheDictionary)
            {
                cacheDictionary.TryGetValue(key, out records);
            }
            return records;
        }

        private static void SetCacheRecords(string domain, string type, List<DnsRecord> records)
        {
            string key = domain + "_" + type;
            lock (cacheDictionary)
            {
                cacheDictionary[key] = records;
            }
        }

        //get first server
        private static string GetServer()
        {
            if (queryServers.Count == 0)
            {
                lock (queryServers)
                {
                    var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var ni in interfaces)
                    {
                        if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
                        {
                            var ip = ni.GetIPProperties();
                            foreach (var item in ip.DnsAddresses)
                            {
                                //only ip 4
                                if (item.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    queryServers.Add(item.ToString());
                                }
                            }
                        }
                    }
                }
            }

            //
            try
            {
                return queryServers[0];
            }
            catch
            {
                return null;
            }
        }

        //server list position change
        private static void ServerExchange()
        {
            lock (queryServers)
            {
                if (queryServers.Count > 1)
                {
                    var server = queryServers[0];
                    queryServers.RemoveAt(0);
                    queryServers.Add(server);
                }
            }
        }

        /// <summary>
        /// 清理缓存数据
        /// </summary>
        public static void ClearCache()
        {
            lock (cacheDictionary)
            {
                cacheDictionary.Clear();
            }
            //
            lock (queryServers)
            {
                queryServers.Clear();
            }
        }

    }

    /// <summary>
    /// DNS Query Handler
    /// </summary>
    public class DnsQuery
    {
        int identity = 0;


        /// <summary>
        /// RFC 1035 (https://tools.ietf.org/html/rfc1035#section-3.2.2)
        /// </summary>
        enum DnsQueryType : ushort
        {
            A = 1,
            MX = 15
        }

        string server;
        /// <summary>
        /// name server address
        /// </summary>
        public string Server
        {
            get { return this.server; }
        }

        int timeout = 2000;
        /// <summary>
        /// set or get query timeout (milliseconds), default 2000
        /// </summary>
        public int QueryTimeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        /// <summary>
        /// initialize new instance
        /// </summary>
        /// <param name="server"></param>
        public DnsQuery(string server)
        {
            this.server = server;
        }

        /// <summary>
        /// query mx
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>failure reutrn null</returns>
        public List<DnsRecord> QueryMX(string domain)
        {
            string error = null;
            return this.Query(domain, DnsQueryType.MX, out error);
        }

        //public void QueryA(string domain)
        //{
        //    string error = null;
        //    this.Query(domain, DnsQueryType.A, out error);
        //}

        private List<DnsRecord> Query(string domain, DnsQueryType queryType, out string error)
        {
            error = null;

            var id = System.Threading.Interlocked.Increment(ref this.identity);

            byte[] recvBuf = null;
            using (UdpClient udpc = new UdpClient(this.server, 53))
            {
                //
                udpc.Client.ReceiveTimeout = this.timeout;

                // SEND REQUEST--------------------
                var reqBuf = this.BuildQuery(id, domain, (ushort)queryType);
                udpc.Send(reqBuf, reqBuf.Length);

                // RECEIVE RESPONSE--------------
                IPEndPoint ep = null;
                recvBuf = udpc.Receive(ref ep);
            }

            //Check the DNS reply
            //check if bit QR (Query response) is set
            if (recvBuf[2] < 128)
            {
                //response byte not set (probably a malformed packet)
                error = "Query response bit not set";
                return null;
            }

            //check if RCODE field is 0
            if ((recvBuf[3] & 15) > 0)
            {
                //DNS server error, invalid reply
                var status = recvBuf[3];
                error = "DNS server error, invalid reply (" + status + ")";
                return null;
            }

            //
            var answers = recvBuf[7];
            if (answers == 0)
            {
                //throw new Exception("No results");
                return new List<DnsRecord>(0);
            }

            var recordList = new List<DnsRecord>(answers);
            //
            int pos = domain.Length + 18;
            if (queryType == DnsQueryType.MX) // MX record
            {
                while (answers > 0)
                {
                    /*
                                                         1  1  1  1  1  1
                          0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                        |                                               |
                        /                                               /
                        /                      NAME                     /
                        |                                               |
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                        |                      TYPE                     |
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                        |                     CLASS                     |
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                        |                      TTL                      |
                        |                                               |
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                        |                   RDLENGTH                    |
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
                        /                     RDATA                     /
                        /                                               /
                        +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
                     */
                    //pos += 2;
                    //ushort recordType = Adf.BaseDataConverter.ToUInt16(recvBuf, pos);
                    //pos += 2;
                    //ushort recordClass = Adf.BaseDataConverter.ToUInt16(recvBuf, pos);
                    //pos += 2;
                    //int recordTTL = Adf.BaseDataConverter.ToInt32(recvBuf, pos);
                    //pos += 4;
                    //ushort blocklen = Adf.BaseDataConverter.ToUInt16(recvBuf, pos);
                    //pos += 2;
                    //ushort preference = 0;
                    //if (recordType == (ushort)DnsQueryType.MX)
                    //{
                    //    preference = Adf.BaseDataConverter.ToUInt16(recvBuf, pos);
                    //    pos += 2;
                    //}

                    int ttl = Adf.BaseDataConverter.ToInt32(recvBuf, pos + 6);
                    var preference = Adf.BaseDataConverter.ToUInt16(recvBuf, pos + 12);
                    pos += 14; //offset
                    string value = this.GetMXRecord(recvBuf, pos, out pos);

                    //Console.WriteLine("MX:\t{0}\t{1}\t{2}\t{3}\n", domain, ttl, preference, value);

                    var record = new DnsRecord();
                    record.Preference = preference;
                    record.TTL = ttl;
                    record.Value = value;
                    record.Expired = Environment.TickCount + (ttl * 1000);

                    answers--;

                    //
                    recordList.Add(record);
                }

                //else if (queryType == DnsQueryType.A) // A record
                //{
                //    while (answers > 0)
                //    {
                //        pos += 11; //offset
                //        string record = this.GetARecord(recvBuf, ref pos);

                //        Console.WriteLine("A :\t{0}\t{1}\n", domain, record);

                //        answers--;
                //    }
                //}
            }
            //
            return recordList;
        }

        private string GetMXRecord(byte[] recv, int start, out int pos)
        {
            StringBuilder sb = new StringBuilder();
            int len = recv[start];
            while (len > 0)
            {
                if (len != 192)
                {
                    if (sb.Length > 0) sb.Append(".");
                    for (int i = start; i < start + len; i++)
                        sb.Append(Convert.ToChar(recv[i + 1]));
                    start += len + 1;
                    len = recv[start];
                }
                else if (len == 192)
                {
                    int newpos = recv[start + 1];
                    if (sb.Length > 0) sb.Append(".");
                    sb.Append(this.GetMXRecord(recv, newpos, out newpos));
                    start++;
                    break;
                }
            }
            pos = start + 1;
            return sb.ToString();
        }

        //
        private byte[] BuildQuery(int id, string query, ushort qtype)
        {
            //Build a DNS query buffer according to RFC 1035 4.1.1 e 4.1.2

            //init vectors with given + default values
            //var id = id;
            var _flags = 256;
            var _QDcount = 1;
            var _ANcount = 0;
            var _NScount = 0;
            var _ARcount = 0;
            var _Qname = query;
            var _Qtype = qtype;
            var _Qclass = 1; //Internet = IN = 1

            //build a buffer with formatted query data

            //header information (16 bit padding
            var buf = new byte[12 + _Qname.Length + 2 + 4];
            buf[0] = (byte)(id / 256);
            buf[1] = (byte)(id - (buf[0] * 256));
            buf[2] = (byte)(_flags / 256);
            buf[3] = (byte)(_flags - (buf[2] * 256));
            buf[4] = (byte)(_QDcount / 256);
            buf[5] = (byte)(_QDcount - (buf[4] * 256));
            buf[6] = (byte)(_ANcount / 256);
            buf[7] = (byte)(_ANcount - (buf[6] * 256));
            buf[8] = (byte)(_NScount / 256);
            buf[9] = (byte)(_NScount - (buf[8] * 256));
            buf[10] = (byte)(_ARcount / 256);
            buf[11] = (byte)(_ARcount - (buf[10] * 256));

            //QNAME (RFC 1035 4.1.2)
            //no padding
            string[] s = _Qname.Split('.');
            int index = 12;
            foreach (string name_item in s)
            {
                buf[index] = (byte)name_item.Length;
                index++;
                byte[] buf1 = Encoding.ASCII.GetBytes(name_item);
                buf1.CopyTo(buf, index);
                index += buf1.Length;
            }
            //add root domain label (chr(0))
            buf[index] = 0;

            //add Qtype and Qclass (16 bit values)
            index = buf.Length - 4;
            buf[index] = (byte)(_Qtype / 256);
            buf[index + 1] = (byte)(_Qtype - (buf[index] * 256));
            buf[index + 2] = (byte)(_Qclass / 256);
            buf[index + 3] = (byte)(_Qclass - (buf[index + 2] * 256));
            //

            return buf;
        }
    }

    /// <summary>
    /// DNS Record
    /// </summary>
    public struct DnsRecord
    {
        /// <summary>
        /// 空值
        /// </summary>
        public static readonly DnsRecord EMPTY = new DnsRecord();

        /// <summary>
        /// 记录值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// TTL
        /// </summary>
        public int TTL { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public ushort Preference { get; set; }

        /// <summary>
        /// 过期时间，Environment.TickCount 值
        /// </summary>
        public int Expired { get; set; }
    }
}


/* https://tools.ietf.org/html/rfc1035#section-3.3.9
3.3.9. MX RDATA format
+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
|                  PREFERENCE                   |
+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
/                   EXCHANGE                    /
/                                               /
+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
where:
PREFERENCE      A 16 bit integer which specifies the preference given to
         this RR among others at the same owner.  Lower values
         are preferred.
EXCHANGE        A <domain-name> which specifies a host willing to act as
         a mail exchange for the owner name.
MX records cause type A additional section processing for the host
specified by EXCHANGE.  The use of MX RRs is explained in detail in
[RFC-974].
*/




/*
 public class Query
        {
            //Build a DNS query buffer according to RFC 1035 4.1.1 e 4.1.2
            private readonly int id;
        private readonly int flags;
        private readonly int QDcount;
        private readonly int ANcount;
        private readonly int NScount;
        private readonly int ARcount;
        private readonly string Qname;
        private readonly int Qtype;
        private readonly  int Qclass;
        public byte[] buf;

        public Query(int ID, string query, int qtype)
        {
            //init vectors with given + default values
            id = ID;
            flags = 256;
            QDcount = 1;
            ANcount = 0;
            NScount = 0;
            ARcount = 0;
            Qname = query;
            Qtype = qtype;
            Qclass = 1; //Internet = IN = 1

            //build a buffer with formatted query data

            //header information (16 bit padding
            buf = new byte[12 + Qname.Length + 2 + 4];
            buf[0] = (byte)(id / 256);
            buf[1] = (byte)(id - (buf[0] * 256));
            buf[2] = (byte)(flags / 256);
            buf[3] = (byte)(flags - (buf[2] * 256));
            buf[4] = (byte)(QDcount / 256);
            buf[5] = (byte)(QDcount - (buf[4] * 256));
            buf[6] = (byte)(ANcount / 256);
            buf[7] = (byte)(ANcount - (buf[6] * 256));
            buf[8] = (byte)(NScount / 256);
            buf[9] = (byte)(NScount - (buf[8] * 256));
            buf[10] = (byte)(ARcount / 256);
            buf[11] = (byte)(ARcount - (buf[10] * 256));
            //QNAME (RFC 1035 4.1.2)
            //no padding
            string[] s = Qname.Split('.');
            int index = 12;
            foreach (string str in s) {
                buf[index] = (byte)str.Length;
                index++;
                byte[] buf1 = Encoding.ASCII.GetBytes(str);
                buf1.CopyTo(buf, index);
                index += buf1.Length;
            }
            //add root domain label (chr(0))
            buf[index] = 0;

            //add Qtype and Qclass (16 bit values)
            index = buf.Length - 4;
            buf[index] = (byte)(Qtype / 256);
            buf[index + 1] = (byte)(Qtype - (buf[index] * 256));
            buf[index + 2] = (byte)(Qclass / 256);
            buf[index + 3] = (byte)(Qclass - (buf[index + 2] * 256));
        }
    }
    public class C_DNSquery
    {
        public StringCollection result = new StringCollection();
        public int Error = 0;
        public string ErrorTxt = "undefined text";
        public bool Done = false;
        public UdpClient udpClient;
        private string DNS;
        private string Query;
        private int Qtype;
        public bool IS_BLACKLIST_QUERY = false;
        public C_DNSquery(string IPorDNSname, string query, int type)
        {
            DNS = IPorDNSname;
            Query = query;
            Qtype = type;
        }
        public void doTheJob()
        {
            //check if provided DNS contains an IP address or a name
            IPAddress ipDNS;
            IPHostEntry he;
            try {
                //try to parse an IPaddress
                ipDNS = IPAddress.Parse(DNS);
            } catch (FormatException ) {
//              Console.WriteLine(e);
                //format error, probably is a FQname, try to resolve it
                try {
                    //try to resolve the hostname
                    he = Dns.GetHostEntry(DNS);
                } catch {
                    //Error, invalid server name or address
                    Error = 98;
                    ErrorTxt = "Invalid server name:" + DNS;
                    Done = true;
                    return;
                }
                //OK, get the first server address
                ipDNS = he.AddressList[0];
            }

            //Query the DNS server
            //our current thread ID is used to match the reply with this process

            Query myQuery = new Query(System.Threading.Thread.CurrentThread.ManagedThreadId, Query, Qtype);
            //data buffer for query return value
            Byte[] recBuf;

            //use UDP protocol to connect
            udpClient = new UdpClient();
            do {
                try {
                    //connect to given nameserver, port 53 (DNS)
                    udpClient.Connect(DNS, 53);
                    //send query
                    udpClient.Send(myQuery.buf, myQuery.buf.Length);
                    //IPEndPoint object allow us to read datagrams..
                    //..selecting only packet coming from our nameserver and port
                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(ipDNS, 53);
                    //Blocks until a message returns on this socket from a remote host.
                    recBuf = udpClient.Receive(ref RemoteIpEndPoint);
                    udpClient.Close();
                } catch (Exception e) {
                    //connection error, probably a wrong server address
                    udpClient.Close();
                    Error = 99;
                    ErrorTxt = e.Message + "(server:" + DNS + ")";
                    Done = true;
                    return;
                }
                //repeat until we get the reply with our threadID
            } while (System.Threading.Thread.CurrentThread.ManagedThreadId != ((recBuf[0] * 256) + recBuf[1]));

            //Check the DNS reply
            //check if bit QR (Query response) is set
            if (recBuf[2] < 128) {
                //response byte not set (probably a malformed packet)
                Error = 2;
                ErrorTxt = "Query response bit not set";
                Done = true;
                return;
            }
            //check if RCODE field is 0
            if ((recBuf[3] & 15) > 0) {
                //DNS server error, invalid reply
                switch (recBuf[3] & 15) {
                    case 1:
                        Error = 31;
                        ErrorTxt = "Format error. The nameserver was unable to interpret the query";
                        break;
                    case 2:
                        Error = 32;
                        ErrorTxt = "Server failure. The nameserver was unable to process the query.";
                        break;
                    case 3:
                        Error = 33;
                        ErrorTxt = "Name error. Check provided domain name!!";
                        break;
                    case 4:
                        Error = 34;
                        ErrorTxt = "Not implemented. The name server does not support the requested query";
                        break;
                    case 5:
                        Error = 35;
                        ErrorTxt = "Refused. The name server refuses to reply for policy reasons";
                        break;
                    default:
                        Error = 36;
                        ErrorTxt = "Unknown. The name server error code was: " + Convert.ToString((recBuf[3] & 15));
                        break;
                }
                Done = true;
                return;
            }
            //OK, now we should have valid header fields
            int QDcnt, ANcnt, NScnt, ARcnt;
            int index;
            QDcnt = (recBuf[4] * 256) + recBuf[5];
            ANcnt = (recBuf[6] * 256) + recBuf[7];
            NScnt = (recBuf[8] * 256) + recBuf[9];
            ARcnt = (recBuf[10] * 256) + recBuf[11];
            index = 12;
            //sometimes there are no erros but blank reply... ANcnt == 0...
            if (ANcnt == 0) { // if blackhole list query, means no spammer !!//if ((ANcnt == 0) & (IS_BLACKLIST_QUERY == false))
                //error blank reply, return an empty array
                Error = 4;
                ErrorTxt = "Empty string array";
                Done = true;
                return;
            }

            //Decode received information
            string s1;
            // START TEST
            s1 = Encoding.ASCII.GetString(recBuf, 0, recBuf.Length);
            // END TEST

            if (QDcnt > 0) {
                //we are not really interested to this string, just parse and skip
                s1 = "";
                index = parseString(recBuf, index, out s1);
                index += 4; //skip root domain, Qtype and QClass values... unuseful in this contest
            }
            if (IS_BLACKLIST_QUERY) {
                // get the answers, normally one !
                // int the four last bytes there is the ip address
                Error = 0;
                int Last_Position = recBuf.Length - 1;
                result.Add(recBuf[Last_Position - 3].ToString() + "." + recBuf[Last_Position - 2].ToString() + "." + recBuf[Last_Position - 1].ToString() + "." + recBuf[Last_Position].ToString());
                Done = true;
                return;
            }
            int count = 0;
            //get all answers
            while (count < ANcnt) {
                s1 = "";
                index = parseString(recBuf, index, out s1);
                //Qtype
                int QType = (recBuf[index] * 256) + recBuf[index + 1];
                index += 2;
                s1 += "," + QType.ToString();
                //QClass
                int QClass = (recBuf[index] * 256) + recBuf[index + 1];
                index += 2;
                s1 += "," + QClass.ToString();
                //TTL (Time to live)
                int TTL = (recBuf[index] * 16777216) + (recBuf[index + 1] * 65536) + (recBuf[index + 2] * 256) + recBuf[index + 3];
                index += 4;
                s1 += "," + TTL.ToString();
                int blocklen = (recBuf[index] * 256) + recBuf[index + 1];
                index += 2;
                if (QType == 15) {
                    int MXprio = (recBuf[index] * 256) + recBuf[index + 1];
                    index += 2;
                    s1 += "," + MXprio.ToString();
                }
                string s2;
                index = parseString(recBuf, index, out s2);
                s1 += "," + s2;
                result.Add(s1);
                count++;
            }
            Error = 0;
            Done = true;
        }
        private int parseString(byte[] buf, int i, out string s)
        {
            int len;
            s = "";
            bool end = false;
            while (!end) {
                if (buf[i] == 192) {
                    //next byte is a pointer to the string, get it..
                    i++;
                    s += getString(buf, buf[i]);
                    i++;
                    end = true;
                } else {
                    //next byte is the string length
                    len = buf[i];
                    i++;
                    //get the string
                    s += Encoding.ASCII.GetString(buf, i, len);
                    i += len;
                    //check for the null terminator
                    if (buf[i] != 0) {
                        //not null, add a point to the name
                        s += ".";
                    } else {
                        //null char..the string is complete, exit
                        end = true;
                        i++;
                    }
                }
            }
            return i;
        }
        private string getString(byte[] buf, int i)
        {
            string s = "";
            int len;
            bool end = false;
            while (!end) {
                len = buf[i];
                i++;
                s += Encoding.ASCII.GetString(buf, i, len);
                i += len;
                if (buf[i] == 192) {
                    i++;
                    s += "." + getString(buf, buf[i]);
                    return s;
                }
                if (buf[i] != 0) {
                    s += ".";
                } else {
                    end = true;
                }
            }
            return s;
        }
    }
 
 */