#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    [XmlType(Namespace="http://mpextended.github.com/schema/config/Services/1")]
    public class NetworkImpersonation
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public bool ReadInStreamingService { get; set; }

        public NetworkImpersonation()
        {
            ReadInStreamingService = true;
        }

        public string GetPassword()
        {
            return EncryptedPassword == String.Empty ? String.Empty : Transformations.Decrypt(EncryptedPassword);
        }

        public void SetPasswordFromPlaintext(string password)
        {
            EncryptedPassword = password == String.Empty ? String.Empty : Transformations.Encrypt(password);
        }

        public bool IsEnabled()
        {
            return !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(GetPassword());
        }
    }

    [XmlType(Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class ExternalAddressConfiguration
    {
        public bool Autodetect { get; set; }
        public string Custom { get; set; }
    }

    [XmlType(Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class DiagnosticConfiguration
    {
        public bool EnableTraceLogging { get; set; }
    }

    [XmlRoot(Namespace="http://mpextended.github.com/schema/config/Services/1")]
    public class Services
    {
        public bool AccessRequestEnabled { get; set; }

        public bool BonjourEnabled { get; set; }
        public string BonjourName { get; set; }
        public ExternalAddressConfiguration ExternalAddress { get; set; }

        public string ServiceAddress { get; set; }
        public int Port { get; set; }
        public bool EnableIPv6 { get; set; }

        public string DefaultLanguage { get; set; }

        public NetworkImpersonation NetworkImpersonation { get; set; }

        public DiagnosticConfiguration Diagnostic { get; set; }

        public Services()
        {
            Port = 4322;
            ExternalAddress = new ExternalAddressConfiguration();
            NetworkImpersonation = new NetworkImpersonation();
            Diagnostic = new DiagnosticConfiguration();
        }

        public string GetServiceName()
        {
            if (!String.IsNullOrWhiteSpace(BonjourName))
            {
                return BonjourName;
            }

            try
            {
                return System.Environment.MachineName;
            }
            catch (Exception)
            {
                return "MPExtended Services";
            }
        }
    }
}
