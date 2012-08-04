using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class BaseMessage
    {
        /// <summary>
        /// Type is a required attribute for all messages. 
        /// The client decides by this attribute what message was sent.
        /// </summary>
        public String Type
        {
            get;
            set;
        }

        public String AutologinKey
        {
            get;
            set;
        }
    }
}
