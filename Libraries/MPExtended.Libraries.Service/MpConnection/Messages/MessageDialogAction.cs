using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageDialogAction : BaseMessage
    {
        public MessageDialogAction()
        {
            this.Type = "dialog";
        }
        public String ActionType { get; set; }
        public int Index { get; set; }
        public String DialogType { get; set; }
        public int DialogId { get; set; }
    }
}
