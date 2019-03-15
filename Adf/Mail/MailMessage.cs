using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.IO;

namespace Adf.Mail
{
    /// <summary>
    /// Mail entity class
    /// </summary>
    public class MailMessage
    {
        List<MailAddress> bcc = new List<MailAddress>(5);
        /// <summary>
        /// 获取包含此电子邮件的密件抄送 (BCC) 收件人的地址列表。
        /// </summary>
        public List<MailAddress> Bcc { get { return this.bcc; } }

        /// <summary>
        /// 添加密件抄送
        /// </summary>
        /// <param name="address"></param>
        public void AddBcc(string address)
        {
            this.bcc.Add(new MailAddress(address));
        }

        /// <summary>
        /// 添加密件抄送
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        public void AddBcc(string name, string address)
        {
            this.bcc.Add(new MailAddress(address, name));
        }

        List<MailAddress> cc = new List<MailAddress>(5);
        /// <summary>
        /// 获取包含此电子邮件的抄送 (CC) 收件人的地址列表。
        /// </summary>
        public List<MailAddress> CC { get { return this.cc; } }

        /// <summary>
        /// 添加抄送地址
        /// </summary>
        /// <param name="address"></param>
        public void AddCC(string address)
        {
            this.cc.Add(new MailAddress(address));
        }

        /// <summary>
        /// 添加抄送地址
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        public void AddCC(string name, string address)
        {
            this.cc.Add(new MailAddress(address, name));
        }
        

        List<MailAddress> to = new List<MailAddress>(5);
        /// <summary>
        /// 获取包含此电子邮件的收件人的地址列表.
        /// </summary>
        public List<MailAddress> To { get { return this.to; } }

        /// <summary>
        /// 添加接收者地址
        /// </summary>
        /// <param name="address"></param>
        public void AddTo(string address)
        {
            this.to.Add(new MailAddress(address));
        }

        /// <summary>
        /// 添加接收者地址
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        public void AddTo(string name, string address)
        {
            this.to.Add(new MailAddress(address, name));
        }

        /// <summary>
        /// 获取或设置邮件的回复地址。
        /// </summary>
        public MailAddress ReplyTo { get; set; }

        /// <summary>
        /// 获取或设置此电子邮件实际的发件人地址 （此属性当前未被使用，预留未来版本） 。
        /// </summary>
        public MailAddress Sender { get; set; }

        /// <summary>
        /// 获取或设置此电子邮件显示的发信人地址。
        /// </summary>
        public MailAddress From { get; set; }

        string body;
        /// <summary>
        /// 获取或设置邮件正文。
        /// </summary>
        public string Body
        {
            get { return this.body; }
            set
            {
                this._outputBody = null;
                this.body = value;
            }
        }


        Encoding encoding = Encoding.UTF8;
        /// <summary>
        /// 获取或设置用于邮件的编码。
        /// </summary>
        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; this._outputBody = null; }
        }


        NameValueCollection headers = new NameValueCollection(5);
        /// <summary>
        /// 获取与此电子邮件一起传输的自定义电子邮件标头。
        /// </summary>
        public NameValueCollection Headers { get { return this.headers; } }



        NameValueCollection outputHeaders = null;
        /// <summary>
        /// 获取此邮件构建的输出标头集合。
        /// </summary>
        public NameValueCollection OutputHeaders { get { return this.outputHeaders; } }

        /// <summary>
        /// 获取或设置指示邮件正文是否为 Html 格式的值。
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// 获取或设置此电子邮件的优先级
        /// </summary>
        public MailPriority Priority { get; set; }

        /// <summary>
        /// 获取或设置此电子邮件的主题行。
        /// </summary>
        public string Subject { get; set; }


        string boundary = null;
        /// <summary>
        /// 获取该邮件的段落分隔符
        /// </summary>
        public string Boundary { get { return this.boundary; } }


        string messageId;
        /// <summary>
        /// 获取为此邮件分配的ID
        /// </summary>
        public string MessageId { get { return this.messageId; } }

        DKIM dkim = null;
        /// <summary>
        /// 获取或设置当前邮件DKIM对象，若不进行DKIM则设置为null, 参考： http://www.xiaobo.li/?p=828
        /// </summary>
        public DKIM Dkim
        {
            get { return this.dkim; }
            set { this.dkim = value; }
        }

        /// <summary>
        /// 初始化邮件体
        /// </summary>
        public MailMessage()
        {
            var now = DateTime.Now;
            this.messageId = MailCommon.BuildMessageID(now);
            //this.boundary = MailCommon.BuildBoundary(now);
            this.boundary = this.messageId;
        }
        
        /// <summary>
        /// 获取邮件头，并填充 OutputHeaders
        /// </summary>
        /// <exception cref="InvalidOperationException">message.From invalid or message.To is empty or subject is empty</exception>
        /// <returns></returns>
        public string GetHead()
        {
            if (this.From == null || string.IsNullOrEmpty(this.From.Address))
                throw new InvalidOperationException("message.From invalid");

            if (this.To == null || this.To.Count == 0)
                throw new InvalidOperationException("message.To is empty");

            if (string.IsNullOrEmpty(this.Subject))
                throw new InvalidOperationException("subject is empty");

            this.outputHeaders = new NameValueCollection(15 + this.headers.Count);

            StringBuilder sb = new StringBuilder();

            //message id
            var messageId = "<" + this.messageId + ">";
            sb.Append("Message-ID: ");
            sb.Append(messageId);
            sb.Append(MailCommon.NewLine);
            outputHeaders["Message-ID"] = messageId;

            //from
            var from = MailCommon.Base64EncodeAddress(this.From.Name, this.From.Address, this.encoding);
            sb.Append("From: ");
            sb.Append(from);
            sb.Append(MailCommon.NewLine);
            outputHeaders["From"] = from;

            //replay
            if (this.ReplyTo != null)
            {
                var replayTo = MailCommon.Base64EncodeAddress(this.ReplyTo.Name, this.ReplyTo.Address, this.encoding);
                sb.Append("Replay-To: ");
                sb.Append(replayTo);
                sb.Append(MailCommon.NewLine);
                outputHeaders["Replay-To"] = replayTo;
            }
            //to
            var to = MailCommon.JoinAddressList(this.to, this.encoding);
            sb.Append("To: ");
            sb.Append(to);
            sb.Append(MailCommon.NewLine);
            outputHeaders["To"] = to;
            //cc
            if (this.cc != null && this.cc.Count > 0)
            {
                var cc = MailCommon.JoinAddressList(this.cc, this.encoding);
                sb.Append("CC: ");
                sb.Append(cc);
                sb.Append(MailCommon.NewLine);
                outputHeaders["CC"] = cc;
            }
            //Subject
            var subject = "";
            sb.Append("Subject: ");
            if (MailCommon.IsAscii(this.Subject))
            {
                subject = this.Subject;
            }
            else
            {
                subject = MailCommon.Base64EncodHead(this.Subject, this.encoding);
            }
            sb.Append(subject);
            sb.Append(MailCommon.NewLine);
            outputHeaders["Subject"] = subject;

            //Data
            var date = DateTime.UtcNow.ToString("R");
            sb.Append("Date: ");
            sb.Append(date);
            sb.Append(MailCommon.NewLine);
            outputHeaders["Date"] = date;

            //Mime
            sb.Append(MailCommon.MimeVersion);
            sb.Append(MailCommon.NewLine);
            outputHeaders["MIME-Version"] = "1.0";

            //priority 
            if (this.Priority != MailPriority.Normal)
            {
                sb.Append("X-Priority: ");
                sb.Append((int)this.Priority);
                sb.Append(MailCommon.NewLine);
                outputHeaders["X-Priority"] = ((int)this.Priority).ToString();
            }
            //Mailer
            sb.Append(MailCommon.Mailer);
            sb.Append(MailCommon.NewLine);
            outputHeaders["X-Mailer"] = "http://www.aooshi.org/adf/";

            //Content-Type
            //sb.Append(CreateContentType());
            var contentType = "multipart/alternative;\r\n\tboundary=\"" + this.boundary + "\"";
            sb.Append("Content-Type: ");
            sb.Append(contentType);
            sb.Append(MailCommon.NewLine);
            outputHeaders["Content-Type"] = contentType;

            //encoding
            sb.Append("Content-Transfer-Encoding: base64");
            sb.Append(MailCommon.NewLine);
            outputHeaders["Content-Transfer-Encoding"] = "base64";

            //custom headers
            foreach (var key in this.headers.AllKeys)
            {
                sb.Append(key);
                sb.Append(": ");
                sb.Append(this.headers[key]);
                sb.Append(MailCommon.NewLine);

                //
                outputHeaders[key] = this.headers[key];
            }

            //dkim
            var dkim = this.dkim;
            if (dkim != null)
            {
                var dkimValue = dkim.Sign(this);
                //
                sb.Insert(0, dkimValue + MailCommon.NewLine);
            }

            //
            sb.Append(MailCommon.NewLine);

            return sb.ToString();
        }

        string _outputBody = null;

        /// <summary>
        /// 获取邮件体
        /// </summary>
        /// <exception cref="InvalidOperationException">message.Body is empty</exception>
        /// <returns></returns>
        public string GetBody()
        {
            if (this._outputBody == null)
            {
                this._outputBody = this.GenerateBody();
            }
            return this._outputBody;
        }

        private string GenerateBody()
        {
            if (string.IsNullOrEmpty(this.Body))
                throw new InvalidOperationException("message.Body is empty");

            //编码完成
            var body = Convert.ToBase64String(this.encoding.GetBytes(this.Body));


            var sb = new StringBuilder();

            //begin
            sb.Append("--");
            sb.Append(this.boundary);
            sb.Append(MailCommon.NewLine);  //分隔完成

            if (this.IsBodyHtml == true)
            {
                sb.Append("Content-Type: text/html; charset=" + this.encoding.HeaderName);
            }
            else
            {
                sb.Append("Content-Type: text/plain; charset=" + this.encoding.HeaderName);
            }
            sb.Append(MailCommon.NewLine);
            sb.Append("Content-Transfer-Encoding: base64");
            sb.Append(MailCommon.NewLine);
            sb.Append(MailCommon.NewLine);
            sb.Append(MailCommon.Line76Break(body));
            sb.Append(MailCommon.NewLine);
            sb.Append(MailCommon.NewLine);

            //end
            // "--" + this.Boundary + "--" + MailCommon.NewLine;
            sb.Append("--");
            sb.Append(this.Boundary);
            sb.Append("--");
            sb.Append(MailCommon.NewLine);

            return sb.ToString();
        }
        
        /// <summary>
        /// 将邮件内容存储至指定流
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            var head = this.GetHead();
            var body = this.GetBody();

            var headBytes = this.encoding.GetBytes(head);
            var bodyBytes = this.encoding.GetBytes(body);

            //输入邮件
            output.Write(headBytes, 0, headBytes.Length);
            output.Write(bodyBytes, 0, bodyBytes.Length);
        }

        /// <summary>
        /// 将邮件邮件存储至指定路径
        /// </summary>
        /// <param name="filepath"></param>
        public void Save(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
            {
                this.Save(fs);
            }
        }
    }
}