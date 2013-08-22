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
        /// The scraper is expecting a text input
        /// </summary>
        [EnumMember]
        TextInput = 0,

        /// <summary>
        /// The scraper is expecting a numeric input
        /// </summary>
        [EnumMember]
        NumberInput = 1,

        /// <summary>
        /// A item is to be selected from a list of possible results
        /// </summary>
        [EnumMember]
        ItemSelect = 2,

        /// <summary>
        /// A yes/no/cancel choice
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
