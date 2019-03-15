using System;
using System.Collections.Generic;
using System.Text;

namespace Adf
{
    /// <summary>
    /// Http Server error event data
    /// </summary>
   public sealed class HttpServerErrorEventArgs : EventArgs
    {
       Exception exception;

       /// <summary>
       /// get error exception
       /// </summary>
       public Exception Exception
       {
           get { return this.exception; }
       }

       /// <summary>
       /// initialize new instance
       /// </summary>
       /// <param name="exception"></param>
       public HttpServerErrorEventArgs(Exception exception)
       {
           if (exception == null)
               throw new ArgumentNullException("exception");
           this.exception = exception;
       }

       /// <summary>
       /// to exception string
       /// </summary>
       /// <returns></returns>
       public override string ToString()
       {
           return this.exception.ToString();
       }
    }
}
