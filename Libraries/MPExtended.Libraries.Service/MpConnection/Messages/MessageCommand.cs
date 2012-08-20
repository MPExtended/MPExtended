using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageCommand : BaseMessage
    {
        public MessageCommand()
        {
            this.Type = "command";
        }

        public String Command { get; set; }
    }
}
