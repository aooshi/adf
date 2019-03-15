using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Adf
{
    /// <summary>
    /// WebSocket 数据帧
    /// </summary>
    public class WebSocketDataFrame
    {
        WebSocketDataFrameHeader _header;
        private byte[] _extend = new byte[0];
        private byte[] _mask = new byte[0];

        /// <summary>
        /// 获取内容
        /// </summary>
        public byte[] Content
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取当前内容Opcode
        /// </summary>
        public WebSocketOpcode Opcode
        {
            get
            {
                return (WebSocketOpcode)_header.OpCode;
            }
        }
        
        ///// <summary>
        ///// 初始化内容帧
        ///// </summary>
        ///// <param name="buffer"></param>
        //public WebSocketDataFrame(byte[] buffer)
        //{
        //    //帧头
        //    _header = new WebSocketDataFrameHeader(buffer);

        //    //扩展长度
        //    if (_header.Length == 126)
        //    {
        //        _extend = new byte[2];
        //        Buffer.BlockCopy(buffer, 2, _extend, 0, 2);
        //    }
        //    else if (_header.Length == 127)
        //    {
        //        _extend = new byte[8];
        //        Buffer.BlockCopy(buffer, 2, _extend, 0, 8);
        //    }

        //    //是否有掩码
        //    if (_header.HasMask)
        //    {
        //        _mask = new byte[4];
        //        Buffer.BlockCopy(buffer, _extend.Length + 2, _mask, 0, 4);
        //    }

        //    //消息体
        //    if (_extend.Length == 0)
        //    {
        //        _content = new byte[_header.Length];
        //        Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, _content.Length);
        //    }
        //    else if (_extend.Length == 2)
        //    {
        //        int contentLength = (int)_extend[0] * 256 + (int)_extend[1];
        //        _content = new byte[contentLength];
        //        Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, contentLength > 1024 * 100 ? 1024 * 100 : contentLength);
        //    }
        //    else
        //    {
        //        long len = 0;
        //        int n = 1;
        //        for (int i = 7; i >= 0; i--)
        //        {
        //            len += (int)_extend[i] * n;
        //            n *= 256;
        //        }
        //        _content = new byte[len];
        //        Buffer.BlockCopy(buffer, _extend.Length + _mask.Length + 2, _content, 0, _content.Length);
        //    }

        //    if (_header.HasMask) _content = Mask(_content, _mask);

        //}

        /// <summary>
        /// 初始化一个Socket数据帧
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="headerBuffer"></param>
        public WebSocketDataFrame(Socket socket, byte[] headerBuffer)
        {
            //var header = new WebSocketDataFrameHeader(this.headerBuffer);
            //byte[] extend = new byte[0];

            ////扩展长度
            //if (header.Length == 126)
            //{
            //    extend = SocketHelper.Receive(this.Socket, 2);
            //}
            //else if (header.Length == 127)
            //{
            //    extend = SocketHelper.Receive(this.Socket, 8);
            //}

            ////是否有掩码
            //byte[] _mask = null;
            //if (header.HasMask)
            //{
            //    _mask = SocketHelper.Receive(this.Socket, 4);
            //}

            ////content length
            //var content_length = 0L;
            //if (extend.Length == 0)
            //    content_length = header.Length;
            //else if (extend.Length == 2)
            //    content_length = (int)extend[0] * 256 + (int)extend[1];
            //else
            //{
            //    int n = 1;
            //    for (int i = 7; i >= 0; i--)
            //    {
            //        content_length += (int)extend[i] * n;
            //        n *= 256;
            //    }

            //    if (content_length > int.MaxValue)
            //        throw new WebException("Data length transfinite");
            //}

            //var contentBuffer = SocketHelper.Receive(this.Socket, (int)content_length);
            //if (header.HasMask)
            //{
            //    WebSocketDataFrame.Mask(contentBuffer, _mask);
            //}

            _header = new WebSocketDataFrameHeader(headerBuffer);

            //扩展长度
            if (_header.Length == 126)
            {
                _extend = SocketHelper.Receive(socket, 2);
            }
            else if (_header.Length == 127)
            {
                _extend = SocketHelper.Receive(socket, 8);
            }

            //是否有掩码
            if (_header.HasMask)
            {
                _mask = SocketHelper.Receive(socket, 4);
            }

            //content length
            var content_length = 0L;
            if (_extend.Length == 0)
                content_length = _header.Length;
            else if (_extend.Length == 2)
                content_length = (int)_extend[0] * 256 + (int)_extend[1];
            else
            {
                int n = 1;
                for (int i = 7; i >= 0; i--)
                {
                    content_length += (int)_extend[i] * n;
                    n *= 256;
                }

                if (content_length > int.MaxValue)
                    throw new WebException("Data length transfinite");
            }

            this.Content = SocketHelper.Receive(socket, (int)content_length);
            if (_header.HasMask)
            {
                Mask(this.Content, _mask);
            }
        }

        /// <summary>
        /// 初始化一个NetworkStream数据帧
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="headerBuffer"></param>
        public WebSocketDataFrame(NetworkStream stream, byte[] headerBuffer)
        {
            _header = new WebSocketDataFrameHeader(headerBuffer);

            //扩展长度
            if (_header.Length == 126)
            {
                _extend = StreamHelper.Receive(stream, 2);
            }
            else if (_header.Length == 127)
            {
                _extend = StreamHelper.Receive(stream, 8);
            }

            //是否有掩码
            if (_header.HasMask)
            {
                _mask = StreamHelper.Receive(stream, 4);
            }

            //content length
            var content_length = 0L;
            if (_extend.Length == 0)
                content_length = _header.Length;
            else if (_extend.Length == 2)
                content_length = (int)_extend[0] * 256 + (int)_extend[1];
            else
            {
                int n = 1;
                for (int i = 7; i >= 0; i--)
                {
                    content_length += (int)_extend[i] * n;
                    n *= 256;
                }

                if (content_length > int.MaxValue)
                    throw new WebException("Data length transfinite");
            }

            this.Content = StreamHelper.Receive(stream, (int)content_length);
            if (_header.HasMask)
            {
                Mask(this.Content, _mask);
            }
        }

        /// <summary>
        /// 初始一个输出内容帧
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isMask"></param>
        /// <param name="opcode"></param>
        public WebSocketDataFrame(byte[] content, bool isMask, WebSocketOpcode opcode = WebSocketOpcode.Text)
        {
            this.Content = content;
            int length = this.Content.Length;

            if (length < 126)
            {
                _extend = new byte[0];
                _header = new WebSocketDataFrameHeader(true, false, false, false, (sbyte)opcode, isMask, length);
            }
            else if (length < 65536)
            {
                _extend = new byte[2];
                _header = new WebSocketDataFrameHeader(true, false, false, false, (sbyte)opcode, isMask, 126);
                _extend[0] = (byte)(length / 256);
                _extend[1] = (byte)(length % 256);
            }
            else
            {
                _extend = new byte[8];
                _header = new WebSocketDataFrameHeader(true, false, false, false, (sbyte)opcode, isMask, 127);

                int left = length;
                int unit = 256;

                for (int i = 7; i > 1; i--)
                {
                    _extend[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }
            //
            if (_header.HasMask)
            {
                _mask = Adf.BaseDataConverter.ToBytes(Environment.TickCount + content.GetHashCode());
                Mask(this.Content, _mask);
            }
        }

        /// <summary>
        /// 返回完整帧字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] GetFrameBytes()
        {
            byte[] buffer = new byte[2 + _extend.Length + _mask.Length + Content.Length];
            Buffer.BlockCopy(_header.GetBytes(), 0, buffer, 0, 2);
            Buffer.BlockCopy(_extend, 0, buffer, 2, _extend.Length);
            Buffer.BlockCopy(_mask, 0, buffer, 2 + _extend.Length, _mask.Length);
            Buffer.BlockCopy(Content, 0, buffer, 2 + _extend.Length + _mask.Length, Content.Length);
            return buffer;
        }

        /// <summary>
        /// 掩码
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        static byte[] Mask(byte[] data, byte[] mask)
        {
            for (long i = 0, l = data.LongLength; i < l; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }

            return data;
        }



//        void

//unmask (int pos, int len, unsigned char *buf)
//{

// int i = pos;                //The position of payload data

// int n = pos - 4;            //The position of masking-key

// for (; i<len; i++,n++) {    

//     if (n == pos) n = pos - 4;  //back to the first masking-key

//     buf[i] ^= buf[n];       //unmask: payload data XOR masking-key

// }

// //delete 4 masking-keys

// for (i=pos; i<len; i++) {

//     buf[i - 4] = buf[i];

// }

// //buf[len - 4] = '\0'; 

//}


    }
}


//http://www.cnblogs.com/bluedoctor/p/3534087.html


//5.        数据成帧
//5.1.       概述
//在WebSocket协议，数据是用一序列帧来传输。为了避免使网络中间设施（如拦截代理）混乱和安全原因（更多讨论在10.3节），客户端必须标记所有发往服务器的帧（更多细节在5.3节）。（注意，进行标记是不管WebSocket协议是否使用了TLS。）当服务器接收到一个没有标记的帧时必须关闭连接。在这种情况下，服务器可能发送一个有状态码1002（协议错误）的关闭帧，如7.4.1节定义。服务器必须不标记它发给客户端的任何帧。客户端必须关闭连接，如果它检测到标记了的帧。在这种情况下，它可能使用1002状态码（协议错误）。（这些规则在未来的规范中可能放松。）
 
//基本的成帧协议定义了帧类型有操作码、有效载荷的长度，指定位置的"Extension data"和"Application data"，统称为"Payload data"。保留了一些特殊位和操作码供后期扩展。
 
//在打开握手完成后，终端发送一个关闭帧之前的任何时间里，数据帧可能由客户端或服务器的任何一方发送（见5.5.1节）。
 
//5.2.       基本的帧协议
//用于数据传输部分的有线格式用ABNF描述，细节在本节。（注意，不像本文档中的其他章节，本节的ABNF是作用于一组比特位上。每组比特位的长度在注释里指示。当在有线上编码后，最重要的比特的最左边的ABNF）。帧的一个高层概览见下图。当下图与ABNF有冲突时，图是权威的。
//FIN： 1bit
//表示此帧是否是消息的最后帧。第一帧也可能是最后帧。
//RSV1，RSV2，RSV3： 各1bit
//必须是0，除非协商了扩展定义了非0的意义。如果接收到非0，且没有协商扩展定义  此值的意义，接收端必须使WebSocket连接失败。
//Opcode： 4bit
//定义了"Payloaddata"的解释。如果接收到未知的操作码，接收端必须使WebSocket       连接失败。下面的值是定义了的。
//%x0 表示一个后续帧
//%x1 表示一个文本帧
//%x2 表示一个二进制帧
//%x3-7 为以后的非控制帧保留
//%x8 表示一个连接关闭
//%x9 表示一个ping
//%xA 表示一个pong
//%xB-F 为以后的控制帧保留
 
//Mask： 1bit
//定义了"Payload data"是否标记了。如果设为1，必须有标记键出现在masking-key，用   来unmask "payload data"，见5.3节。所有从客户端发往服务器的帧必须把此位设为1。
 
//Payload length： 7bit, 7 + 16bit, 7 + 64bit
//"Payloaddata"的长度，字节单位。如果值是0-125，则是有效载荷长度。如果是126，   接下来的2字节解释为16位无符号整数，作为有效载荷长度。如果127，接下来的8  字节解释为64位无符号整数（最高位必须是0），作为有效载荷长度。多字节长度数值    以网络字节序表示。注意，在任何情况下，必须用最小数量的字节来编码长度，例如，       124字节 长的字符串不能编码为序列126, 0, 124。有效载荷长度是"Extension data"的长     度加上"Application data"的长度。"Extension data"的长度可能是0，在这种情况下，    有效载荷长度是"Applicationdata"的长度。
 
//Masking-key：0或4字节
//所有从客户端发往服务器的帧必须用32位值标记，此值在帧里。如果mask位设为1， 此字段（32位值）出现，否则缺失。更多的信息在5.3节，客户端到服务器标记。
 
//Payload data： (x + y)字节
//"Payloaddata" 定义为"extensiondata" 后接"application data"。
 
//Extension data： x 字节
//"Extensiondata"是0字节，除非协商了扩张。所有扩张必须指定"extensiondata"的长度，      或者如何计算长度，如何使用扩展必须在打开握手时进行协商。如果有，"Extension data"包括在有效载荷长度。
 
//Application data： y字节
//任意"Applicationdata"占据了帧的剩余部分，在"Extensiondata"之后。 "Applicationdata"的长度等于有效载荷长度减去"Extensiondata"的长度。
 
//The base framingprotocol is formally defined by the following ABNF
//[RFC5234]. It isimportant to note that the representation of this
//data is binary, notASCII characters. As such, a field with a length
//of 1 bit that takesvalues %x0 / %x1 is represented as a single bit
//whose value is 0 or1, not a full byte (octet) that stands for the
//characters"0" or "1" in the ASCII encoding. A field with a length
//of 4 bits withvalues between %x0-F again is represented by 4 bits,
//again NOT by anASCII character or full byte (octet) with these
//values. [RFC5234]does not specify a character encoding: "Rules
//resolve into astring of terminal values, sometimes called
//characters. InABNF, a character is merely a non-negative integer.
//In certaincontexts, a specific mapping (encoding) of values into a
//character set (suchas ASCII) will be specified." Here, the
//specified encodingis a binary encoding where each terminal value is
//encoded in the specified number of bits, which varies for each field.
 
//ws-frame =frame-fin ; 1 bit in length
//frame-rsv1; 1 bit in length
//frame-rsv2; 1 bit in length
//frame-rsv3; 1 bit in length
//frame-opcode; 4 bits in length
//frame-masked; 1 bit in length
//frame-payload-length; either 7, 7+16,
//;or 7+64 bits in
//;length
//[frame-masking-key ] ; 32 bits in length
//frame-payload-data; n*8 bits in
//;length, where
//;n >= 0
//frame-fin = %x0 ;more frames of this message follow
//%x1 ; final frame of this message
//;1 bit in length
//frame-rsv1 = %x0 /%x1
//;1 bit in length, MUST be 0 unless
//;negotiated otherwise
//frame-rsv2 = %x0 /%x1
//;1 bit in length, MUST be 0 unless
//;negotiated otherwise
//frame-rsv3 = %x0 /%x1
//;1 bit in length, MUST be 0 unless
//;negotiated otherwise
//frame-opcode =frame-opcode-non-control /
//frame-opcode-control/
//frame-opcode-cont
//frame-opcode-cont =%x0 ; frame continuation
//frame-opcode-non-control=%x1 ; text frame
////%x2 ; binary frame
////%x3-7
//;4 bits in length,
//;reserved for further non-control frames
//frame-opcode-control= %x8 ; connection close
////%x9 ; ping
////%xA ; pong
////%xB-F ; reserved for further control
//; frames
//; 4 bits in length
 
//frame-masked = %x0
//;frame is not masked, no frame-masking-key
////%x1
//;frame is masked, frame-masking-key present
//;1 bit in length
//frame-payload-length= ( %x00-7D )
////( %x7E frame-payload-length-16 )
////( %x7F frame-payload-length-63 )
//;7, 7+16, or 7+64 bits in length,
//;respectively
//frame-payload-length-16= %x0000-FFFF ; 16 bits in length
//frame-payload-length-63= %x0000000000000000-7FFFFFFFFFFFFFFF
//;64 bits in length
//frame-masking-key =4( %x00-FF )
//;present only if frame-masked is 1
//;32 bits in length
//frame-payload-data= (frame-masked-extension-data
//frame-masked-application-data)
//;when frame-masked is 1
////(frame-unmasked-extension-data
//frame-unmasked-application-data)
//;when frame-masked is 0
//frame-masked-extension-data= *( %x00-FF )
//;reserved for future extensibility
//;n*8 bits in length, where n >= 0
//frame-masked-application-data= *( %x00-FF )
//;n*8 bits in length, where n >= 0
//frame-unmasked-extension-data= *( %x00-FF )
//;reserved for future extensibility
//;n*8 bits in length, where n >= 0
//frame-unmasked-application-data= *( %x00-FF )
//; n*8bits in length, where n >= 0
 
 
//5.3.       客户端到服务器标记
//被标记的帧必须设置frame-masked域为1，如5.2节定义。标记键必须作为frame-masking-key完整包含在帧里，作为frame-masking-key，如5.2节定义。它用于标记"Payload data"。
 
//标记键是由客户端随机选择的32位值。准备标记帧时，客户端必须从一组允许的32位值选择一个新的标记键。标记键应该是不可预测的，因此，标记键必须推断自强源的熵（strong source of entropy），给定帧的标记键必须不能简单到服务器或代理可以预测标记键是用于一序列帧的。不可预测的标记键是阻止恶意应用的作者从wire上获取数据的关键。RFC 4086  discusses what entails a suitable source of entropy forsecurity-sensitive applications.
 
//标记不影响"Payloaddata"的长度。为了把标记数据转换为非标记数据，或反转，下面的算法将被应用。同样的算法将应用，而不管转换的方向，例如同样的步骤将用于标记数据和unmask数据。
 
//转换后的数据八位字节i （transformed-octet-i）是 原始八位字节i（original-octet-i）与 标记键在 i 对 4 取模（j = i % 4）后的位序的八位字节（masking-key-octet-j）的 异或(XOR)。
//j  =   i  MOD  4
//transformed-octet-i = original-octec-i  XOR masking-key-octet-j
 
//译者给出的Scala语言实现：
//  def mask(key: Array[Byte],data: Array[Byte]){
//    for (i <- 0 untildata.length) {
//      val j = i & 3 // 对4 取模
//      data(i) = (data(i) ^ key(j)).toByte
//    }
//  }
 
//帧里的有效载荷长度指示frame-payload-length，不包括masking key的长度。它是"Payloaddata"，如，masking key的后续字节 的长度。
 
//5.4.       分帧
//分帧的首要目的是允许发送开始时不知道长度的消息，不需要缓存消息。如果消息不能分帧，那么终端得缓存整个消息，以便在发送前计算长度。有了分帧，服务器或中间设施可以选择一个合理大小的缓存，当缓存满时，把帧写到网络。
 
//分帧的第二个用例是多路复用，在不值得大消息独占逻辑输出通道的地方，多路复用需要自由地把消息划分更小的帧来更好地共享输出通道。(注意，多路技术扩展不在本文档描述。)
 
//除非另有扩展说明，分帧没有语义上的意义。中间设施可能合并 和/或分隔帧，如果客户端和服务器没有协商扩展，或协商了扩展，但中间设施明白所有协商了的扩展，并知道如何合并 和/或拆分帧。分帧的实现在没有扩展时，发送者和接收者必须不能依赖于帧的边界。
 
//分帧的规则：
//A、一个未分帧的消息包含单个帧，FIN设置为1，opcode非0。
 
//B、一个分帧了的消息包含：
//开始于：单个帧，FIN设为0，opcode非0；
//后接  ：0个或多个帧，FIN设为0，opcode设为0；
//终结于：单个帧，FIN设为1，opcode设为0。
//一个分帧了消息在概念上等价于一个未分帧的大消息，它的有效载荷长度等于所有帧的       有效载荷长度的累加；然而，有扩展时，这可能不成立，因为扩展定义了出现的       "Extension data"的解释。例如，"Extensiondata"可能只出现在第一帧，并  用于后续的所有帧，或者"Extensiondata"出现于所有帧，且只应用于特定的那个  帧。在缺少"Extensiondata"时，下面的示例示范了分帧如何工作。
 
//举例：如一个文本消息作为三个帧发送，第一帧的opcode是0x1，FIN是0，第二帧 的opcode是0x0，FIN是0，第三帧的opcode是0x0，FIN是1。
 
//C、控制帧（见5.5节）可能被插入到分帧了消息中。控制帧必须不能被分帧。
 
//D、消息的帧必须以发送者发送的顺序传递给接受者。
 
//E、一个消息的帧必须不能交叉在其他帧的消息中，除非有扩展能够解释交叉。
 
//F、一个终端必须能够处理消息帧中间的控制帧。
 
//G、一个发送者可能对任意大小的非控制消息分帧。
 
//H、客户端和服务器必须支持接收分帧和未分帧的消息。
 
//I、由于控制帧不能分帧，中间设施必须不尝试改变控制帧。
 
//J、中间设施必须不修改消息的帧，如果保留位的值已经被使用，且中间设施不明白这些值的含义。
 
//在扩展已经被协商好，且中间设施不知道已协商扩展的语义的环境，中间设施必须不修改任何消息的帧。类似地，anintermediary that didn’t see theWebSocket handshake (and wasn’t notified about its content) that resulted in a WebSocketconnection MUST NOT change thefragmentation of any message of such connection.
 
//K、作为这些规则的结果，一个消息的所有帧属于同样的类型，由第一个帧的opcdoe指定。由于控制帧不能分帧，消息的所有帧的类型要么是文本、二进制数据或保留的操作码中的一个。
 
//注意：如果控制帧不能插入，例如，ping的延迟将会很长，如果是在一个大消息后面。因此要求处理消息帧中间的控制帧。
 
//实现注意：如果没有任何扩展，接收者不需要为了处理而缓存整个帧。例如，如果使用了流API，帧的一部分可以传递给应用程序。然而，这个假设在未来的WebSocket扩展中可能不成立。
 
 
//5.5.       控制帧
//控制帧由操作码标识，操作码的最高位是1。当前为控制帧定义的操作码有0x8（关闭）、0x9（Ping）和0xA（Pong），操作码0xB-0xF是保留的，未定义。
 
//控制帧用来交流WebSocket的状态，能够插入到消息的多个帧的中间。
 
//所有的控制帧必须有一个小于等于125字节的有效载荷长度，必须不能被分帧。
 
//5.5.1.      关闭
//关闭帧有个操作码0x8。
 
//关闭帧可能包含一个主体（帧的应用数据部分）指明关闭的原因，如终端关闭，终端接收到的帧太大，或终端接收到的帧不符合终端的预期格式。如果有主体，主体的前2个字节必须是2字节的无符号整数（按网络字节序），表示以/code/（7.4节定义）的值为状态码。在2字节整数后，主体可能包含一个UTF-8编码的字符串表示原因。这些数据不一定要人类可读，但对调试友好，或给打开连接的脚本传递信息。由于不保证数据是人类可读的，客户端必须不显示给用户。
 
//从客户端发送到服务器的关闭帧必须标记，按5.3节。
 
//在发送关闭帧后，应用程序必须不再发送任何数据。
 
//如果终端接收到一个关闭帧，且先前没有发送关闭帧，终端必须发送一个关闭帧作为响应。（当发送一个关闭帧作为响应时，终端典型地以接收到的状态码作为回应。）它应该尽快实施。终端可能延迟发送关闭帧，直到它的当前消息发送完成（例如，如果分帧消息的大部分已发送，终端可能在关闭帧之前发送剩余的帧）。然而，不保证已经发送了关闭帧的终端继续处理数据。
 
//在发送和接收到关闭消息后，终端认为WebSocket连接已关闭，必须关闭底层的TCP连接。服务器必须立即关闭底层的TCP连接；客户端应该等待服务器关闭连接，但可能在发送和接收到关闭消息后关闭，例如，如果它在合理的时间间隔内没有收到TCP关闭。
 
//如果客户端和服务器同时发送关闭消息，两端都已发送和接收到关闭消息，应该认为WebSocket连接已关闭，并关闭底层TCP连接。
 
//5.5.2.      Ping
//Ping帧包含操作码0x9。
 
//一个Ping帧可能包含应用程序数据。
 
//当接收到Ping帧，终端必须发送一个Pong帧响应，除非它已经接收到一个关闭帧。它应该尽快返回Pong帧作为响应。Pong帧在5.5.3节讨论。
 
//终端可能在连接建立后、关闭前的任意时间内发送Ping帧。
 
//注意：Ping帧可作为keepalive或作为验证远程终端是否可响应的手段。
 
 
//5.5.3.      Pong
//Pong帧包含操作码0xA。
 
//5.5.2节的详细要求适用于Ping和Pong帧。
 
//Pong 帧必须包含与被响应Ping帧的应用程序数据完全相同的数据。
 
//如果终端接收到一个Ping 帧，且还没有对之前的Ping帧发送Pong 响应，终端可能选择发送一个Pong 帧给最近处理的Ping帧。
 
//一个Pong 帧可能被主动发送，这作为单向心跳。对主动发送的Pong 帧的响应是不希望的。
 
 
//5.6.       数据帧
//数据帧（如非控制帧）由操作码标识，操作码的最高位是0。当前为数据帧定义的操作码有0x1（文本），0x2（二进制）。操作码0x3-0x7 为以后的非控制帧保留，未定义。
 
//数据帧携带 应用程序层和/或扩展层 数据。操作码决定了数据的解释：
//文本：
//有效载荷数据是UTF-8编码的文本数据。注意，特定的文本帧可能包含部分的UTF-8   序列，然而，整个消息必须包含有效的UTF-8。重新聚合后的非法的UTF-8的处理在8.1  节描述。
 
//二进制：
//有效载荷数据是任意的二进制数据，它的解释由应用程序层唯一决定。
 
//5.7.       举例
//单个未标记的文本消息帧：
//0x81 0x05 0x48 0x65 0x6c 0x6c 0x6f(contains "Hello")
 
//单个标记了的文本消息帧：
//0x81 0x85 0x37 0xfa 0x21 0x3d 0x7f 0x9f 0x4d 0x51 0x58 (contains "Hello")
 
//分帧了的未标记的文本消息：
//0x01 0x03 0x48 0x65 0x6c (contains "Hel")
//0x80 0x02 0x6c 0x6f(contains "lo")
 
//未标记的Ping请求和标记了的Ping 响应：
//0x89 0x05 0x48 0x65 0x6c 0x6c 0x6f(contains a body of "Hello", but the contents of the body arearbitrary)
 
//0x8a0x85 0x37 0xfa 0x21 0x3d 0x7f0x9f 0x4d 0x51 0x58 (contains abody of "Hello", matching the body of the ping)
 
//含有256字节二进制数据消息的单个未标记帧：
//0x82 0x7E 0x0100 [256 bytes of binary data]
 
//64KB二进制数据消息在单个未标记帧：
//0x82 0x7F 0x0000000000010000 [65536 bytes of binary data]
 
 
//5.8.       扩展
//协议旨在允许扩展，将给基础协议添加功能。连接的终端必须在打开握手中协商使用任意的扩展。这个规范提供了操作码0x3到0x7和0xB到0xF，帧头的“Extensiondata”域，和frame-rsv1,frame-rsv2和frame-rsv3 位供扩展使用。扩展的协商在9.1节讨论。下面是一些期望的扩展使用，这个清单既不是完整的也不是规范。
//A、"extensiondata" 可能在"applicationdata"之前放置。
 
//B、保留位可能按帧的需要分配。
 
//C、保留的操作码可以定义。
 
//D、保留位可以分配给操作码域，如果需要更多的操作码。、
 
//E、保留位或扩展操作码可以定义在"Payload data"外分配额外的位来定义更大的操作码或     更多的特定帧位。
 
 
//6.        发送和接收数据
//6.1.       发送数据
//为在WebSocket连接上发送一个由/data/组成的WebSocket消息，终端必须执行下面的步骤：
//1.        终端必须确保WebSocket连接处于OPEN状态。在任何时刻，如果WebSocket连接的    状态改变，终端必须终止下面的步骤。
 
//2.        终端必须按5.2节在WebSocket帧里封装数据。如果数据太大或者如果数据在终端准备 开始发送数据时不能完整可得，终端可能选择封装数据为一序列的帧，按5.4节。
 
//3.        包含数据的第一帧的操作码（frame-opcode）必须设置为恰当的值，按5.2节，因为这   决定数据将被接收者解释为文本或二进制数据。
 
//4.        包含数据的最后帧的FIN位（frame-fin）必须设置为1，如5.2节定义。
 
//5.        如果数据由客户端发送，帧必须被标记，如5.3节定义。
 
//6.        如果为WebSocket连接协商了扩展（第9章），额外的考虑可能按定义的扩展 应用。
 
//7.        已形成的帧必须在底层的网络连接上传输。
 
//6.2.       接收数据
//为了接收WebSocket数据，终端监听底层的网络连接。到来的数据必须按5.2节定义的WebSocket帧解析。如果接收到控制帧，必须按5.5节的定义处理。当接收到数据帧时，终端必须知道数据的类型，通过操作码（frame-opcode）定义。帧的"Application data"是定义为消息的/data/的。如果帧包含一个未分帧的消息，就是说接收到一个包含类型和数据的WebSocket消息。如果帧是分帧了的消息的一部分，应用数据是后续帧的数据的拼接。当最后帧（有FIN位指示）接收到时，就是说WebSocket消息接收到了，数据由所有帧的应用数据拼接组成，类型由第一帧指示。后续的数据帧必须解释为属于新的WebSocket消息的。
 
//扩展（第9章）可能改变如何读取数据的语义，具体包括什么组成消息的边界。扩展，在"Applicationdata"前添加"Extensiondata"，是计入有效载荷的，也可能修改"Application data"。
 
//服务器必须移除从客户端接收到的数据帧的标记，如5.3节。
 
 
//7.        关闭连接
//7.1.       定义
//7.1.1.      关闭WebSocket连接
//为关闭WebSocket连接，终端关闭底层的TCP连接。终端应该使用能够干净地关闭TCP连接和TLS会话的方法。如果可以，丢弃可能接收到的后续的任意字节。当需要时，终端可能使用任意方法关闭连接，例如受到攻击时。
 
//在大多数情况下，应该由服务器先关闭底层的TCP连接，这样它持有TIME_WAIT状态，而不是客户端（因为这会导致它在2个最大段生命周期（2MSL）内不能重新打开，然而对服务器没有相应的影响，因为当遇到有更高序号的SYN时，TIME_WAIT连接可以立即重新打开）。在不正常的情况（例如在合适的时间内没有从服务器接收到TCP关闭）下，客户端可能开始TCP关闭。这样，当服务器得到关闭WebSocket连接指示，应该马上开始TCP关闭，当客户端得到关闭WebSocket连接的指示，它应该等待来自服务器的TCP关闭。
 
//As an example ofhow to obtain a clean closure in C using Berkeley
//sockets, one wouldcall shutdown() with SHUT_WR on the socket, call
//recv() untilobtaining a return value of 0 indicating that the peer
//has also performedan orderly shutdown, and finally call close() on
//the socket.
 
//7.1.2.      开始WebSocket关闭握手
//为用状态码/code/（7.4节）和可选的关闭理由/reason/（7.1.6节）开始WebSocket关闭握手，终端必须发送一个关闭帧，如5.5.1节描述。一旦终端已经发送和接收到关闭帧，终端应该关闭WebSocket连接，如7.1.1节定义。
 
//7.1.3.      WebSocket关闭握手已开始
//基于正在发送或接收关闭帧，这说明WebSocket关闭握手已经开始，WebSocket连接处于CLOSING状态。
 
//7.1.4.      WebSocket连接已关闭
//当底层TCP连接已关闭，说明WebSocket连接已关闭，处于CLOSED状态。如果TCP连接在WebSocket关闭握手完成后关闭，就是说WebSocket连接已干净地关闭。
 
//如果WebSocket连接没能建立，这也说WebSocket连接已关闭，但是不干净。
 
//7.1.5.      WebSocket连接关闭码
//如5.5.1和7.4节定义，一个关闭控制帧可能包含一个状态码指示关闭的理由。一个WebSocket连接的关闭可能由任意一方的终端发起，也可能同时。WebSocket连接的关闭码是作为状态码（7.4节）定义，包含在实现这个协议的应用程序接收到的第一个关闭控制帧。如果关闭控制帧不包含状态码，WebSocket连接的关闭码被认为是1005。如果WebSocket连接已关闭，且终端没有收到关闭控制帧（这可能出现在底层传输连接丢失的情况），WebSocket连接的关闭码被认为是1006。
 
//注意：两个终端可能不对WebSocket连接的关闭码达成一致。例如，如果远程终端发送了一个关闭帧，但本地应用程序还没有从它自己的套接字接收缓存读取包含控制帧的数据，本地应用程序独自决定连接，并发送了一个关闭帧，两个终端都已发送和接收到一个关闭帧，不再发送更多的关闭帧。每个终端将看到另一端发送的状态码，作为WebSocket连接的关闭码。这样，在两个终端大致同时开始关闭握手的情况下，它们可能不能在WebSocket连接的关闭码上达成一致。
 
//7.1.6.      WebSocket关闭理由
//如5.5.1和7.4节定义，一个关闭帧可能包含状态码指示关闭的理由，后接UTF-8编码的数据，数据的解释留给终端，不在协议定义。WebSocket连接的关闭可能由两个终端的任何一方发起，也可能同时。WebSocket连接关闭理由定义为UTF-8编码的数据，跟在状态码（7.4节）后面，包含在实现本协议的应用程序接收到的一个关闭帧里。如果关闭帧里没有这样的数据，WebSocket连接关闭理由被认为是空的字符串。
 
//注意：遵循7.1.5节提示的，两个终端可能不在WebSocket连接关闭理由上达成一致。
 
//7.1.7.      使WebSocket连接失败
//特定的算法和规范要求终端使WebSocket连接失败。为此，客户端必须关闭WebSocket连接，可能通过恰当的方式把问题报告给用户（这对开发者特别有用）。类似地，服务器必须关闭WebSocket连接，并应该记录问题到日志。
 
//如果WebSocket连接在终端要求使WebSocket连接失败之前已经建立，终端应该发生一个关闭帧，用合适的状态码，在继续关闭WebSocket连接之前。终端可能忽略发送关闭帧，如果它认为另一端不大可能接收和处理关闭帧，是因为导致WebSocket连接关闭的错误。一个终端必须不继续尝试处理来自远程终端的数据（包括响应的控制帧），在收到使WebSocket连接失败的指令后。
 
//除了上面指示的或由应用程序层指定的，客户端不应该关闭连接。
 
//7.2.       非正常关闭
//7.2.1.      客户端开始的关闭
//特定的算法，特别是在打开握手的时候，要求客户端使WebSocket连接失败。为此，客户端必须使WebSocket连接失败，按7.1.7节定义。
 
//如果在底层传输层连接意外消失的任何时刻，客户端必须使WebSocket连接失败。
 
//除了上面指示的或由应用程序层指定的，客户端不应该关闭连接。
 
//7.2.2.      服务器端开始的关闭
//特殊的算法要求或建议服务器中止WebSocket连接，在打开握手的时候。为此，服务器必须简单地关闭WebSocket连接（7.1.1节）。
 
//7.2.3.      从非正常关闭恢复
//非正常关闭可能由任意数量的原因导致。这样的关闭可能是由于暂时的错误，在这种情况下，重连可能导致一个好的连接，恢复到正常操作。这样的关闭也可能是因为非暂时性问题，在in whichcase if each deployed client experiences an abnormal closure and immediately andpersistently tries to reconnect, the server may experience what amounts to a denial-of-service attack by a large numberof clients trying to reconnect. The end result of such a scenario could be that the service isunable to recover in a timelymanner or recovery is made much more difficult.
 
//To prevent this,clients SHOULD use some form of backoff when trying
//to reconnect afterabnormal closures as described in this section.
 
//The first reconnectattempt SHOULD be delayed by a random amount of time. The parameters by which this randomdelay is chosen are left
//to the client todecide; a value chosen randomly between 0 and 5 seconds is a reasonable initial delay thoughclients MAY choose a differentinterval from which to select a delay length based on implementation experience and particularapplication.
 
//Should the firstreconnect attempt fail, subsequent reconnect attempts SHOULD be delayed by increasinglylonger amounts of time, using amethod such as truncated binary exponential backoff.
 
//7.3.       正常关闭连接
//服务器在需要时可能关闭WebSocket连接。客户端不应该贸然关闭WebSocket连接。在任何情况下，终端遵循开始WebSocket连接关闭握手的步骤（7.1.2节）来发起关闭。
 
//7.4.       状态码
//当关闭一个已建立连接时（如，在打开握手完成后，发送一个关闭帧），终端可能指示一个关闭理由。本规范未定义终端如何解释理由和给出理由要采取的动作。本规范定义了一组预定义的状态码，指定了哪些范围可能由扩展、框架、终端应用程序使用。状态码和关联的文本消息对于关闭帧是可选的。
 
//7.4.1.      状态码定义
//在发送关闭帧时，终端可能使用下面预定义的状态码。
 
//1000：表示正常关闭，意味着连接建立的目的已完成。
 
//1001：表示终端离开，例如服务器关闭或浏览器导航到其他页面。
 
//1002：表示终端因为协议错误而关闭连接。
 
//1003：表示终端因为接收到不能接受的数据而关闭（例如，只明白文本数据的终端可能发送         这个，如果它接收到二进制消息）。
 
//1004：保留。这个特定含义可能在以后定义。
 
//1005：保留。且终端必须不在控制帧里设置作为状态码。它是指定给应用程序而非作为状态       码 使用的，用来指示没有状态码出现。
 
//1006：同上。保留。且终端必须不在控制帧里设置作为状态码。它是指定给应用程序而非作       为状态    码 使用的，用来指示连接非正常关闭，例如，没有发生或接收到关闭帧。
 
//1007：表示终端因为接收到的数据没有消息类型而关闭连接。
 
//1008：表示终端因为接收到的消息背离它的政策而关闭连接。这是一个通用的状态码，用在       没有更合适的状态码或需要隐藏具体的政策细节时。
 
//1009：表示终端因为接收到的消息太大以至于不能处理而关闭连接。
 
//1010：表示客户端因为想和服务器协商一个或多个扩展，而服务器不在响应消息返回它（扩       展）而关闭连接。需要的扩展列表应该出现在关闭帧的/reason/部分。注意，这个状态       码不是由服务器使用，因为它会导致WebSocket握手失败。
 
//1011：表示服务器因为遇到非预期的情况导致它不能完成请求而关闭连接。
 
//1015：保留，且终端必须不在控制帧里设置作为状态码。它是指定用于应用程序希望用状态       码来指示连接因为TLS握手失败而关闭。
 
 
//7.4.2.      保留的状态码范围
//0 - 999 ：未使用。
 
//1000 - 2999 ：此范围的保留给本协议使用的，它以后的校订和扩展将以永久的和容易获取  公开规范里指定。
 
//3000 - 3999 ：此范围保留给类库、框架和应用程序使用。这些状态码直接在IANA注册。  本协议未定义如何解释这些状态码。
 
//4000 - 4999 ：此范围保留给私用，且不能注册。这些值可以在预先达成一致的WebSocket   应用程序间使用。本协议未定义如何解释这些值。
 
 
//8.        错误处理
//8.1.       处理UTF-8编码的数据的错误
//当终端以UTF-8解释字节流时发现字节流不是一个合法的UTF-8流，那么终端必须使WebSocket连接失败。这个规则适用于打开握手时及后续的数据交换时。