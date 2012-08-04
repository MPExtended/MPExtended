using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    public class MessageAuthenticationResponse : BaseMessage
    {
        /// <summary>
        /// Indicator if authentification was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error messsage in case authentification failed
        /// </summary>
        public String ErrorMessage { get; set; }
        
        /// <summary>
        /// Key used to autologin
        /// </summary>
        public String AutologinKey { get; set; }

    }
}
