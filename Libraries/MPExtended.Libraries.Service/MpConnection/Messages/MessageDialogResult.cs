using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageDialogResult: BaseMessage
    {
        /// <summary>
        /// Result of Yes/No dialog
        /// </summary>
        public bool YesNoResult { get; set; }

        /// <summary>
        /// Id of dialog (random id sent by client)
        /// </summary>
        public String DialogId { get; set; }
    }
}
