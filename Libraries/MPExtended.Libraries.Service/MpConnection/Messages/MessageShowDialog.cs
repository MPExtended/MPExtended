using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageShowDialog: BaseMessage
    {
        public MessageShowDialog()
        {
            this.Type = "showdialog";
        }

        public String DialogType { get; set; }
        public String DialogId { get; set; }
        public String Title { get; set; }
        public String Text { get; set; }

    }
}
