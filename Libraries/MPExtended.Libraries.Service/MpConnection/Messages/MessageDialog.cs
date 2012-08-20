using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageDialog : BaseMessage
    {
        /// <summary>
        /// Dialog data (only sent when dialog state == shown)
        /// </summary>
        public MpDialog Dialog { get; set; }

        /// <summary>
        /// Dialog state (shown, closed)
        /// </summary>
        public bool DialogShown { get; set; }

        /// <summary>
        /// True if the message contains updates to the already shown dialog, false if it's a new dialog
        /// </summary>
        public bool DialogUpdate { get; set; }
    }
}
