using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace Adf
{
    /// <summary>
    /// HTTP 内容分析器
    /// </summary>
    internal class HttpServerMultipartReceiver
    {
        Encoding encoding;
        Socket socket;
        SocketBufferReader reader;
        string boundary;
        HttpServerContext context;
        //
        string boundary_split;
        string boundary_end;
        string boundary_crlf;
        byte[] boundary_split_data;
        byte[] boundary_crlf_data;
        byte[] boundary_end_data;
        int boundary_split_length;
        int boundary_end_length;
        int boundary_crlf_length;
        //
        int contentLength;
        //
        const int CONTENT_BUFFER_SIZE = 8192;
        byte[] contentBuffer = new byte[CONTENT_BUFFER_SIZE];
        int contentBufferPosition = 0;
        //
        bool isEnd = false;


        public HttpServerMultipartReceiver(Socket socket, string boundary, HttpServerContext context, int contentLength)
        {
            //------WebKitFormBoundary
            //Content-Disposition: form-data; name="picture"; filename="DSCF0001.JPG"
            //Content-Type: image/jpeg

            //<file bytes>
            //------WebKitFormBoundary--


            //-----------------------------je456723
            //Content-Disposition: form-data; name="file"; filename="data.txt"
            //Content-Type: text/plain
            //...
            //...
            //-----------------------------je456723
            //Content-Disposition: form-data; name="submit"
            //
            //Submit
            //-----------------------------je456723--

            this.socket = socket;
            this.encoding = context.Encoding;
            this.boundary = boundary;
            this.context = context;
            this.contentLength = contentLength;
            //
            this.boundary_split = "--" + boundary;
            this.boundary_end = "--" + boundary + "--";
            this.boundary_crlf = "\r\n--" + boundary;
            this.boundary_split_data = Encoding.ASCII.GetBytes(this.boundary_split);
            this.boundary_end_data = Encoding.ASCII.GetBytes(this.boundary_end);
            this.boundary_crlf_data = Encoding.ASCII.GetBytes(this.boundary_crlf);
            this.boundary_split_length = this.boundary_split_data.Length;
            this.boundary_end_length = this.boundary_end_data.Length;
            this.boundary_crlf_length = this.boundary_crlf_data.Length;
            //
            this.reader = new SocketBufferReader(socket, encoding, contentLength);
        }

        /// <summary>
        /// receive
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="IOException"></exception>
        /// <returns>is success</returns>
        public bool Receive()
        {
            //first boundary
            var line_data = this.reader.ReadByteLine();
            if (this.CompareBoundary(line_data))
            {
                while (this.isEnd == false)
                {
                    if (this.ParsePart() == false)
                    {
                        return false;
                    }
                    this.context.UploadedLength = this.reader.ReadAllLength;
                }
            }
            else if (this.CompareBoundaryEnd(line_data))
            {
            }
            else
            {
                throw new InvalidDataException("unknown format of content");
            }

            //
            this.context.UploadedLength = this.reader.ReadAllLength;

            //check content length
            if (this.reader.ReadAllLength == this.contentLength)
            {
                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                {
                    context.Content = "end of unfinished";
                }
                context.Response(HttpStatusCode.BadRequest);
                return false;
            }
        }

        private bool ParsePart()
        {
            var disposition = this.reader.ReadStringLine();
            if (disposition == this.boundary_end)
            {
                return true;
            }
            //
            var start = disposition.IndexOf("name=\"") + 6;
            var end = disposition.IndexOf('"', start);
            var name = disposition.Substring(start, end - start);
            //
            start = disposition.IndexOf("filename=\"") + 10;
            if (start > 9)
            {
                end = disposition.IndexOf('"', start);
                var filename = disposition.Substring(start, end - start);

                //read content_type
                var content_type_line = this.reader.ReadStringLine();
                var content_type = (content_type_line.Split(':')[1]).Trim();
                //read empty line
                while (true)
                {
                    if (this.reader.ReadByteLine().Length == 0)
                    {
                        break;
                    }
                }

                //create file
                HttpServerFileParameter fileParameter = null;
                try
                {
                    fileParameter = this.context.FileHandler.Create(name, filename, content_type, this.context);
                }
                catch (Exception exception)
                {
                    if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                    {
                        context.Content = "Create FileHandler Failure " + exception.Message;
                    }
                    context.Response(HttpStatusCode.InternalServerError);
                    return false;
                }

                if (this.ParseContent(fileParameter.Stream) == false)
                {
                    return false;
                }

                //
                fileParameter.Stream.Position = 0;
                this.context.Files.Add(fileParameter);
            }
            else
            {
                //read empty line
                while (true)
                {
                    if (this.reader.ReadByteLine().Length == 0)
                    {
                        break;
                    }
                }
                //read content
                using (var m = new MemoryStream(64))
                {
                    if (this.ParseContent(m) == false)
                    {
                        return false;
                    }

                    var value = this.encoding.GetString(m.GetBuffer(), 0, (int)m.Length);
                    context.Form.Add(name, value);
                }
            }

            return true;
        }

        private bool ParseContent(Stream contentStream)
        {
            var matchPos = 0;
            byte b = 0;
            //
            this.contentBufferPosition = 0;
            int flag = 0;
            //
            while (true)
            {
                b = this.reader.Read();
                if (matchPos != 0)
                {
                    if (b == this.boundary_crlf_data[matchPos])
                    {
                        matchPos++;
                        //match
                        if (matchPos == this.boundary_crlf_length)
                        {
                            //completed
                            if (this.contentBufferPosition > 0)
                            {
                                //writer to file stream
                                try
                                {
                                    contentStream.Write(this.contentBuffer, 0, this.contentBufferPosition);
                                }
                                catch (Exception exception)
                                {
                                    if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                                    {
                                        context.Content = "Write File Data Failure " + exception.Message;
                                    }
                                    context.Response(HttpStatusCode.InternalServerError);
                                    return false;
                                }
                            }
                            //end -- & \r\n
                            while (true)
                            {
                                var b1 = this.reader.Read();
                                var b2 = this.reader.Read();
                                if (b1 == 13 && b2 == 10)
                                {
                                    break;
                                }
                                else if (b1 == this.boundary_split_data[0] && b2 == this.boundary_split_data[0])
                                {
                                    //end
                                    this.isEnd = true;
                                    continue;
                                }
                            }
                            //
                            return true;
                        }
                    }
                    else
                    {
                        //reset
                        for (int i = 0; i < matchPos; i++)
                        {
                            if (this.WriteToContent(contentStream, this.boundary_crlf_data[i]) == false)
                                return false;
                        }
                        if (this.WriteToContent(contentStream, b) == false)
                            return false;
                        //
                        matchPos = 0;
                    }
                }
                else if (b == boundary_crlf_data[0])
                {
                    matchPos = 1;
                }
                else
                {
                    if (this.WriteToContent(contentStream, b) == false)
                        return false;
                }

                //set uploaded length
                flag++;
                if (flag > 1024 && flag % 1024 == 0)
                {
                    this.context.UploadedLength = this.reader.ReadAllLength;
                }
            }
        }

        private bool WriteToContent(Stream contentStream, byte b)
        {
            this.contentBuffer[this.contentBufferPosition++] = b;
            if (this.contentBufferPosition == CONTENT_BUFFER_SIZE)
            {
                //writer to file stream 
                try
                {
                    contentStream.Write(this.contentBuffer, 0, CONTENT_BUFFER_SIZE);
                }
                catch (Exception exception)
                {
                    if (string.IsNullOrEmpty(context.Content) && context.ContentBuffer == null)
                    {
                        context.Content = "Write File Data Failure " + exception.Message;
                    }
                    context.Response(HttpStatusCode.InternalServerError);
                    return false;
                }

                this.contentBufferPosition = 0;
            }
            return true;
        }

        private bool CompareBoundary(byte[] buffer)
        {
            if (buffer == null)
                return false;

            if (buffer.Length == this.boundary_split_length && buffer[0] == this.boundary_split_data[0])
            {
                for (int i = 1; i < this.boundary_split_length; i++)
                {
                    if (buffer[i] != this.boundary_split_data[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private bool CompareBoundaryEnd(byte[] buffer)
        {
            if (buffer == null)
                return false;

            if (buffer.Length == this.boundary_end_length && buffer[0] == this.boundary_end_data[0])
            {
                for (int i = 1; i < this.boundary_end_length; i++)
                {
                    if (buffer[i] != this.boundary_end_data[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}