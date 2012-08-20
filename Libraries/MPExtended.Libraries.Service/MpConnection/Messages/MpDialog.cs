using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MpDialog
    {
        public MpDialog()
        {
            AvailableActions = new List<String>();
        }

 
        /// <summary>
        /// Dialog Heading
        /// </summary>
        public String Heading { get; set; }

        /// <summary>
        /// Text of dialog
        /// </summary>
        public String DialogText { get; set; }

        /// <summary>
        /// Dialog Module name in MediaPortal (language dependant)
        /// </summary>
        public String DialogType { get; set; }

        /// <summary>
        /// Id of Dialog
        /// </summary>
        public int DialogId { get; set; }

        /// <summary>
        /// Value of Dialog
        /// </summary>
        public int DialogValue { get; set; }

        /// <summary>
        /// Actions available for this dialog
        /// </summary>
        public List<String> AvailableActions { get; set; }
    }
}
