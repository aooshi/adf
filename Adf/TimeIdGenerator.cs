using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Adf
{
    /// <summary>
    /// time id generate
    /// </summary>
    public class TimeIdGenerator
    {
        ushort nodeId = 0;
        int increment = 0;

        byte[] nodeBuffer;
        byte[] keyBuffer;
        byte[] ivBuffer = new byte[] { 1,2,3,4,5,6,7,8,9,0,11,12,13,14,15,16 };
        
        const int BASE_VALUE = 10000000;

        /// <summary>
        /// New Instance
        /// </summary>
        /// <param name="key">key ,length 16 bit</param>
        /// <param name="nodeId"></param>
        /// <exception cref="ArgumentOutOfRangeException">key length invalid</exception>
        public TimeIdGenerator(string key, ushort nodeId)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentOutOfRangeException("key length must 16 length");

            if (key.Length != 16)
                throw new ArgumentOutOfRangeException("key length must 16 length");

            var keyBuffer = Encoding.ASCII.GetBytes(key);
            if (keyBuffer.Length != 16)
                throw new ArgumentOutOfRangeException("key length must 16 length");

            this.nodeId = nodeId;
            this.nodeBuffer = Adf.BaseDataConverter.ToBytes(nodeId);
            this.keyBuffer = keyBuffer;
            this.increment = this.GetHashCode();
        }

        /// <summary>
        /// generate
        /// </summary>
        /// <returns></returns>
        public byte[] Generate()
        {
            var buffer = new byte[14];
            int value = 0;

            value = System.Threading.Interlocked.Increment(ref this.increment);

            var time = DateTime.UtcNow.Ticks / BASE_VALUE;

            buffer[0] = this.nodeBuffer[0];
            buffer[1] = this.nodeBuffer[1];

            Adf.BaseDataConverter.ToBytes(time, buffer, 2);
            Adf.BaseDataConverter.ToBytes(value, buffer, 10);

            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.BlockSize = 128;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.IV = this.ivBuffer;
                rm.Key = this.keyBuffer;

                using (ICryptoTransform ct = rm.CreateEncryptor())
                {
                    buffer = ct.TransformFinalBlock(buffer, 0, buffer.Length);
                }
            }


            return buffer;
        }

        /// <summary>
        /// generate id
        /// </summary>
        /// <returns></returns>
        public string GenerateId()
        {
            var buffer = this.Generate();

            var v = Adf.UUIDBase58.Encode(buffer);

            return v;
        }

        /// <summary>
        /// generate hex id
        /// </summary>
        /// <returns></returns>
        public string GenerateHexId()
        {
            var buffer = this.Generate();

            var build = new StringBuilder();
            foreach (var b in buffer)
            {
                build.Append(b.ToString("x2"));
            }
            return build.ToString();
        }
    }
}
