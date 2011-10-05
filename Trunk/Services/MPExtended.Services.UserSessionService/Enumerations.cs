using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.UserSessionService
{
    public class Utils
    {

    }

    [DataContract]
    public enum WebPowerModes
    {

        /// <summary>
        /// Shuts down all processes running in the security context of the process that called the ExitWindowsEx function. Then it logs the user off.
        /// </summary>
        [EnumMember]
        LogOff = 0,
        /// <summary>
        /// Shuts down the system and turns off the power. The system must support the power-off feature.
        /// </summary>
        [EnumMember]
        PowerOff = 1,
        /// <summary>
        /// Shuts down the system and then restarts the system.
        /// </summary>
        [EnumMember]
        Reboot = 2,
        /// <summary>
        /// Shuts down the system to a point at which it is safe to turn off the power. All file buffers have been flushed to disk, and all running processes have stopped. If the system supports the power-off feature, the power is also turned off.
        /// </summary>
        [EnumMember]
        ShutDown = 3,
        /// <summary>
        /// Suspends the system.
        /// </summary>
        [EnumMember]
        Suspend = 4,
        /// <summary>
        /// Hibernates the system.
        /// </summary>
        [EnumMember]
        Hibernate = 5,
    }
}
