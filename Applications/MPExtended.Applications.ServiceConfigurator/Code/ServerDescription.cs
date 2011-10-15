using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    /// <summary>
    /// Class for holding basic information about this htpc
    /// </summary>
    public class ServerDescription
    {
        [DataMember(Name = "ServiceType")]
        public String ServiceType { get; set; }

        [DataMember(Name = "GeneratorApp")]
        public String GeneratorApp { get; set; }

        [DataMember(Name = "Addresses")]
        public String Addresses { get; set; }

        [DataMember(Name = "Port")]
        public int Port { get; set; }

        [DataMember(Name = "Hostname")]
        public String Hostname { get; set; }

        [DataMember(Name = "HardwareAddresses")]
        public String HardwareAddresses { get; set; }

        [DataMember(Name = "Name")]
        public String Name { get; set; }

        [DataMember(Name = "User")]
        public String User { get; set; }

        [DataMember(Name = "Password")]
        public String Password { get; set; }

        [DataMember(Name = "Passcode")]
        public String Passcode { get; set; }

        [DataMember(Name = "AuthOptions")]
        public int AuthOptions { get; set; }

    }
}
