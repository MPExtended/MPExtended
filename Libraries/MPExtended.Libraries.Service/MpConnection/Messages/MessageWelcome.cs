using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace MPExtended.Libraries.Service.MpConnection.Messages
{
    class MessageWelcome 
    {
        /// <summary>
        /// API version of this WifiRemote instance. 
        /// Should be increased on breaking changes.
        /// </summary>
        public int Server_Version 
        {
            get;
            set;
        }

        /// <summary>
        /// Authentication method required of the client.
        /// </summary>
        public int AuthMethod
        {
            get;
            set;
        }

        public Dictionary<string, bool> MPExtendedServicesInstalled
        {
            get;
            set;
        }
    }
}
