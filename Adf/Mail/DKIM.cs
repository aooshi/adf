using System;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Adf.Mail
{
    /// <summary>
    /// DKIM Algorithm
    /// </summary>
    public enum DKIMAlgorithm
    {
        /// <summary>
        /// Sha1
        /// </summary>
        RSASha1 = 1,
        /// <summary>
        /// Sha256, is default
        /// </summary>
        RSASha256 = 2
    }

    /// <summary>
    /// DKIM Type
    /// </summary>
    public enum DKIMType
    {
        /// <summary>
        /// Simple
        /// </summary>
        Simple,
        /// <summary>
        /// Relaxed
        /// </summary>
        Relaxed
    }

    /// <summary>
    /// DKIM
    /// </summary>
    public class DKIM
    {
        static string[] SIGN_HEADERS = new string[] { "From", "To", "Subject", "Date" };

        //默认设置为relaxed， 因relaxed进行了头重构，进行头一致化加密。减少因在传输过种中遇到的一睦mta传递非规范头而引起的校验失败。
        DKIMType headerType = DKIMType.Relaxed;
        /// <summary>
        /// DKIM type , default Relaxed
        /// </summary>
        public DKIMType HeaderType
        {
            get { return this.headerType; }
            set { this.headerType = value; }
        }
        //默认设置为simple, 增加性能，且因主体一般不会类似头重写一样， 出发地和目的地基本保持一些，因此使用simple以提高性能。
        DKIMType bodyType = DKIMType.Simple;
        /// <summary>
        /// DKIM type , default Simple
        /// </summary>
        public DKIMType BodyType
        {
            get { return this.bodyType; }
            set { this.bodyType = value; }
        }

        byte[] keys = null;

        DKIMAlgorithm algorithm = DKIMAlgorithm.RSASha256;
        /// <summary>
        /// DKIM Algorithm, default RSASha256
        /// </summary>
        public DKIMAlgorithm Algorithm
        {
            get { return this.algorithm; }
            set { this.algorithm = value; }
        }

        string domain = null;
        /// <summary>
        /// domain
        /// </summary>
        public string Domain
        {
            get { return this.domain; }
        }

        string selector = null;
        /// <summary>
        /// selector
        /// </summary>
        public string Selector
        {
            get { return this.selector; }
        }

        string[] signHeaders = SIGN_HEADERS;
        /// <summary>
        /// sign headers
        /// </summary>
        public string[] SignHeaders
        {
            get { return this.signHeaders; }
            set { this.signHeaders = value; }
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="selector"></param>
        public DKIM(string domain, string selector)
        {
            if (string.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domain");

            if (string.IsNullOrEmpty(selector))
                throw new ArgumentNullException("selector");

            this.domain = domain;
            this.selector = selector;
        }

        /// <summary>
        /// Load Key File pcks7 pem
        /// </summary>
        /// <param name="filepath"></param>
        public void LoadKeyFile(string filepath)
        {
            if (filepath == null)
            {
                throw new ArgumentNullException("filepath");
            }

            var privateKey = File.ReadAllText(filepath);

            this.keys = OpenSslKey.DecodeOpenSSLPrivateKey(privateKey);
        }

        /// <summary>
        /// Load Key pcks7 pem
        /// </summary>
        /// <param name="privateKey"></param>
        public void LoadKey(string privateKey)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException("privateKey");
            }

            this.keys = OpenSslKey.DecodeOpenSSLPrivateKey(privateKey);
        }

        private byte[] Sign(byte[] data)
        {
            using (var rsa = OpenSslKey.DecodeRSAPrivateKey(this.keys))
            {
                byte[] signature = rsa.SignData(data, this.GetAlgorithmName());

                return signature;

            }
        }

        private byte[] Hash(byte[] data)
        {
            using (var hash = GetHash())
            {
                return hash.ComputeHash(data);
            }
        }

        private HashAlgorithm GetHash()
        {
            switch (this.algorithm)
            {
                case DKIMAlgorithm.RSASha1:
                    {
                        return new SHA1Managed();
                    }
                case DKIMAlgorithm.RSASha256:
                    {
                        return new SHA256Managed();
                    }

                default:
                    {
                        throw new ArgumentException("Invalid DKIMAlgorithm value", "DKIMAlgorithm");
                    }

            }
        }

        private string GetAlgorithmName()
        {
            switch (this.algorithm)
            {
                case DKIMAlgorithm.RSASha1:
                    {
                        return "SHA1";
                    }
                case DKIMAlgorithm.RSASha256:
                    {
                        return "SHA256";
                    }
                default:
                    {
                        throw new ArgumentException("Invalid DKIMAlgorithm value", "DKIMAlgorithm");
                    }

            }
        }

        /// <summary>
        /// DKIM message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Sign(MailMessage message)
        {
            //http://dkimcore.org/specification.html#1
            //DKIM-Signature: v=1;a=rsa-sha256;bh=BODYHASH;c=relaxed;d=TOKEN;h=HEADERS;s=SELECTOR;b=SIGNATURE

            //http://www.dkim.org/specs/rfc4871-dkimbase.html#dkim-sig-hdr

            /*
DKIM-Signature: v=1; a=rsa-sha256; c=simple/relaxed; d=domain.com;
 h=From:To:Subject:Date; q=dns/txt; s=newsletter17100; t=1510020630;
 bh=+5tUEqyyuolGsLc9usglUSkelpZtxEkEWKaGC21PSEM=;
 b=
             */

            var dsLength = "DKIM-Signature: ".Length;

            var build = new StringBuilder(128);
            build.Append("DKIM-Signature:");
            build.Append(" v=1;");


            //
            string algorithm = this.algorithm == DKIMAlgorithm.RSASha1 ? "rsa-sha1" : "rsa-sha256";
            build.Append(" a=").Append(algorithm).Append(';');

            //c=header type / bodey type;
            string htype = this.headerType == DKIMType.Simple ? "simple" : "relaxed";
            string bType = this.bodyType == DKIMType.Simple ? "simple" : "relaxed";
            //build.Append(" c=simple/relaxed;");
            build.Append(" c=").Append(htype).Append('/').Append(bType).Append(';').Append(MailCommon.NewLine);
            //create time
            long unixtime = Adf.UnixTimestampHelper.ToInt64Timestamp();
            build.Append("\tt=").Append(unixtime).Append(';');
            build.Append(" q=dns/txt;").Append(MailCommon.NewLine);
            // domain
            build.Append("\td=").Append(domain).Append(';');
            // DNS selector
            build.Append(" s=").Append(selector).Append(';').Append(MailCommon.NewLine);

            //headers
            build.Append("\th=").Append(string.Join(":", this.signHeaders)).Append(';').Append(MailCommon.NewLine);

            //body hash
            var body = "";
            if (this.bodyType == DKIMType.Relaxed)
                body = this.RelaxedBodyCanonicalization(message.GetBody());
            else
                body = this.SimpleBodyCanonicalization(message.GetBody());

            var bodyBytes = message.Encoding.GetBytes(body);
            var bodySigned = this.Hash(bodyBytes);
            var bodyHash = Convert.ToBase64String(bodySigned);
            build.Append("\tbh=").Append(bodyHash).Append(';').Append(MailCommon.NewLine);
            //b
            build.Append("\tb=");

            //build string
            /*
DKIM-Signature: v=1; a=rsa-sha256; c=simple/relaxed; d=caping.co.id;
 s=newsletter17800; q=dns/txt; t=1510023325; h=From:To:Subject:Date;
 h=yklAmb1CNh0SAfmrk97PlZTEHrGKV94Ps6R7KDwLLoo=;
 b=
             */

            //set current DKIM-Signature
            var dkimHead = build.ToString(dsLength, build.Length - dsLength);
            message.OutputHeaders["DKIM-Signature"] = dkimHead;

            var headers = "";
            if (this.headerType == DKIMType.Relaxed)
                headers = this.RelaxedHeaderCanonicalization(message.OutputHeaders);
            else
                headers = this.SimpleHeaderCanonicalization(message.OutputHeaders);
            var headerBytes = message.Encoding.GetBytes(headers);
            var headerSigned = this.Sign(headerBytes);

            // assumes signature ends with "b="
            var b = Convert.ToBase64String(headerSigned);
            b = MailCommon.Line76Break("b=" + b, "\t");


            //remove last b
            build.Length -= "\tb=".Length;
            //append b
            build.Append(b);


            //update DKIM-Signature
            dkimHead = build.ToString(dsLength, build.Length - dsLength);
            message.OutputHeaders["DKIM-Signature"] = dkimHead;

            var signed = build.ToString();
            return signed;
        }

        //http://www.dkim.org/specs/rfc4871-dkimbase.html#canonicalization
        private string SimpleHeaderCanonicalization(NameValueCollection headers)
        {
            var signHeadersLength = signHeaders.Length;
            var items = new string[signHeadersLength + 1];
            for (int i = 0; i < signHeadersLength; i++)
            {
                var k = this.signHeaders[i];
                var v = headers[k];
                //
                items[i] = string.Concat(k, ": ", v);
            }

            //add DKIM-Signature
            var v1 = headers["DKIM-Signature"];
            items[items.Length - 1] = string.Concat("DKIM-Signature: ", v1);
            //
            return string.Join(MailCommon.NewLine, items);
        }

        // 3.4.2 The "relaxed" Header Canonicalization Algorithm
        //http://www.dkim.org/specs/rfc4871-dkimbase.html#canonicalization
        private string RelaxedHeaderCanonicalization(NameValueCollection headers)
        {
            var signHeadersLength = signHeaders.Length;
            var items = new string[signHeadersLength + 1];
            for (int i = 0; i < signHeadersLength; i++)
            {
                var k = this.signHeaders[i].Trim().ToLower();
                var v = headers[k].Trim().Replace(MailCommon.NewLine, string.Empty);
                //
                items[i] = string.Concat(k, ":", ReplaceWitespace(v));
            }

            //add DKIM-Signature
            var v1 = headers["DKIM-Signature"].Trim().Replace(MailCommon.NewLine, string.Empty);
            items[items.Length - 1] = string.Concat("dkim-signature:", ReplaceWitespace(v1));

            //
            return string.Join(MailCommon.NewLine, items);
        }

        //3.4.4 The "relaxed" Body Canonicalization Algorithm
        //The "relaxed" body canonicalization algorithm:
        private string RelaxedBodyCanonicalization(string body)
        {
            if (body.Length < 1)
            {
                return string.Empty;
            }

            string line = null;
            var list = new List<string>(10);

            using (var reader = new StringReader(body))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty)
                    {
                        list.Add(string.Empty);
                    }
                    else
                    {
                        list.Add(ReplaceWitespace(line.TrimEnd()));
                    }
                }
            }

            //add last newlist
            list.Add(string.Empty);

            return Adf.ConvertHelper.ArrayToString<string>(list, MailCommon.NewLine);
        }

        private string SimpleBodyCanonicalization(string body)
        {
            if (body.Length < 1)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(body.Length + MailCommon.NewLine.Length);

            using (var reader = new StringReader(body))
            {
                string line = null;
                int lineCount = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty)
                    {
                        lineCount++;
                        continue;
                    }

                    while (lineCount > 0)
                    {
                        builder.AppendLine();
                        lineCount--;
                    }

                    builder.AppendLine(line);
                }
            }

            if (builder.Length == 0)
            {
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string ReplaceWitespace(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            var build = new StringBuilder(text.Length);
            bool hasWhiteSpace = false;
            foreach (var c in text)
            {
                if (IsWhiteSpace(c))
                {
                    hasWhiteSpace = true;
                }
                else
                {
                    if (hasWhiteSpace)
                    {
                        build.Append(' ');
                    }
                    build.Append(c);
                    hasWhiteSpace = false;
                }
            }
            return build.ToString();
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }
    }
}