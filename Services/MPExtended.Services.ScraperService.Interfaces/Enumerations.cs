using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.ScraperService.Interfaces
{
    [DataContract]
    public enum WebInputTypes
    {
        /// <summary>
        /// Shuts down all processes running in the security context of the process that called the ExitWindowsEx function. Then it logs the user off.
        /// </summary>
        [EnumMember]
        TextInput = 0,

        /// <summary>
        /// Shuts down the system and turns off the power. The system must support the power-off feature.
        /// </summary>
        [EnumMember]
        NumberInput = 1,

        /// <summary>
        /// Shuts down the system and then restarts the system.
        /// </summary>
        [EnumMember]
        ItemSelect = 2,

        /// <summary>
        /// Shuts down the system to a point at which it is safe to turn off the power. All file buffers have been flushed to disk, and all running processes have stopped. If the system supports the power-off feature, the power is also turned off.
        /// </summary>
        [EnumMember]
        YesNoCancel = 3
    }

    [DataContract]
    public enum WebScraperState
    {
        /// <summary>
        /// Scraper = running
        /// </summary>
        [EnumMember]
        Running = 0,

        /// <summary>
        /// Scraper is paused
        /// </summary>
        [EnumMember]
        Paused = 1,

        /// <summary>
        /// Scraper is stopped
        /// </summary>
        [EnumMember]
        Stopped = 2
    }
}
