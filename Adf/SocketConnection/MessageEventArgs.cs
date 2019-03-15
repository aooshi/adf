using System;
using System.Collections.Generic;
using System.Text;

namespace Adf.SocketConnection
{
    /// <summary>
    /// on  new message event
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        object message = null;
        /// <summary>
        /// get event content
        /// </summary>
        public object Message
        {
            get { return this.message; }
        }

        bool isContinue = true;
        /// <summary>
        /// is continue read next message, set flase for need manual call read method. default true.
        /// </summary>
        public bool IsContinue
        {
            get { return this.isContinue; }
            set { this.isContinue = value; }
        }

        /// <summary>
        /// message event args
        /// </summary>
        /// <param name="message"></param>
        public MessageEventArgs(object message)
        {
            this.message = message;
        }

        ///// <summary>
        ///// message event args
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="isContinue"></param>
        //public MessageEventArgs(object message, bool isContinue)
        //{
        //    this.message = message;
        //    this.isContinue = isContinue;
        //}

    }
}
