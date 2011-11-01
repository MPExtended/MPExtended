#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace MPExtended.Installers.CustomActions
{
    internal static class WifiRemote
    {
        private const string UPDATE_FILE = @"http://wifiremote.googlecode.com/svn/trunk/Installer/update.xml";

        private enum WifiRemoteState
        {
            Error,
            NewerVersionAvailable,
            SameVersion,
            OlderVersionAvailable,
        }

        public static bool Install(Session session)
        {
            // download WifiRemote update information
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Downloading WifiRemote update information...", ""));
            Uri downloadUri;
            WifiRemoteState infoState = GetWifiRemoteVersionInfo(session, out downloadUri);
            Dictionary<WifiRemoteState, string> messages = new Dictionary<WifiRemoteState,string>() {
                { WifiRemoteState.Error, "WifiRemote: failed to download update information, using packaged version" },
                { WifiRemoteState.OlderVersionAvailable, "WifiRemote: older version available online then we shipped... strange, using packaged version" },
                { WifiRemoteState.SameVersion, "WifiRemote: last version packaged, installing from MSI" },
                { WifiRemoteState.NewerVersionAvailable, "WifiRemote: newer version available, downloading it" }
            };
            Log.Write(messages[infoState]);

            // download newer WifiRemote if needed
            string mpeiPackagePath = Path.GetTempFileName();
            bool downloadFailed = false;
            if (infoState == WifiRemoteState.NewerVersionAvailable)
            {
                session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Downloading WifiRemote installer...", ""));
                if (!DownloadWifiRemote(downloadUri, mpeiPackagePath))
                {
                    Log.Write("WifiRemote: failed to download it, continuing with extracting from MSI");
                    downloadFailed = true;
                }
            }

            // else extract from package
            if(infoState != WifiRemoteState.NewerVersionAvailable || downloadFailed)
            {
                Log.Write("WifiRemote: extracting package from MSI");
                if (!InstallerDatabase.ExtractBinaryToFile(session, "WifiRemoteInstallerBin", mpeiPackagePath))
                {
                    Log.Write("WifiRemote: failed to extract from MSI");
                    return false;
                }
            }
            Log.Write("WifiRemote: got WifiRemote installer in {0}", mpeiPackagePath);

            // lookup MPEI installer location
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Installing WifiRemote MediaPortal plugin with MPEI...", ""));
            string mpei = LookupMPEI();
            if (mpei == null)
            {
                return false;
            }
            Log.Write("WifiRemote: found MPEI at {0}", mpei);

            // execute
            ProcessStartInfo param = new ProcessStartInfo();
            param.FileName = mpei;
            param.Arguments = mpeiPackagePath + " /S";
            Log.Write("WifiRemote: starting MPEI with arguments {0}", param.Arguments);
            Process proc = new Process();
            proc.StartInfo = param;
            proc.Start();
            proc.WaitForExit();
            Log.Write("WifiRemote: MPEI finished with exit code {0}", proc.ExitCode);

            // cleanup
            File.Delete(mpeiPackagePath);
            return true;
        }

        private static WifiRemoteState GetWifiRemoteVersionInfo(Session session, out Uri downloadLast)
        {
            downloadLast = null;
            string xmlData;

            // download update.xml
            try
            {
                Log.Write("WifiRemote: Downloading update.xml from {0}", UPDATE_FILE);
                using (WebClient client = new WebClient())
                {
                    xmlData = client.DownloadString(UPDATE_FILE);
                }
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to download update.xml: {0}", ex.Message);
                return WifiRemoteState.Error;
            }

            // parse it
            try
            {
                XElement updateFile = XElement.Parse(xmlData);

                // set uri
                string uri = updateFile.Element("Items").Element("PackageClass").Element("GeneralInfo").Element("OnlineLocation").Value.ToString();
                downloadLast = new Uri(uri);
                
                // get wifiremote version
                XElement versionNode = updateFile.Element("Items").Element("PackageClass").Element("GeneralInfo").Element("Version");
                Version remoteVersion = new Version(
                    Int32.Parse(versionNode.Element("Major").Value),
                    Int32.Parse(versionNode.Element("Minor").Value),
                    Int32.Parse(versionNode.Element("Build").Value),
                    Int32.Parse(versionNode.Element("Revision").Value)
                );
                Version ourVersion = new Version(InstallerDatabase.GetProductProperty(session, "WifiRemotePackagedVersion"));
                Log.Write("WifiRemote: We packaged {0} and {1} is available online", ourVersion.ToString(), remoteVersion.ToString());

                // compare and return
                int compare = ourVersion.CompareTo(remoteVersion);
                if (compare == 0)
                    return WifiRemoteState.SameVersion;
                return compare < 0 ? WifiRemoteState.NewerVersionAvailable : WifiRemoteState.OlderVersionAvailable;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to parse update.xml: {0}\r\n{1}\r\n{2}", ex.Message, ex.ToString(), ex.StackTrace.ToString());
                return WifiRemoteState.Error;
            }
        }

        private static bool DownloadWifiRemote(Uri url, string tempPath)
        {
            // download it
            try
            {
                Log.Write("WifiRemote: Downloading from {0}", url.ToString());
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, tempPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to download WifiRemote", ex.Message);
                return false;
            }
        }

        private static string LookupMPEI()
        {
            try
            {
                string keyPath = Environment.Is64BitOperatingSystem ?
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal" :
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key == null)
                {
                    Log.Write("WifiRemote: Could not find MediaPortal installation path key");
                    return null;
                }

                object value = key.GetValue("InstallPath", null);
                if (value == null)
                {
                    Log.Write("WifiRemote: Could not find MediaPortal installation path value");
                    return null;
                }

                string path = value.ToString();
                string mpei = Path.Combine(path, "MpeInstaller.exe");
                if (!File.Exists(mpei))
                {
                    Log.Write("WifiRemote: MPEI path {0} does not exists", mpei);
                    return null;
                }

                return mpei;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to lookup MPEI path", ex);
                return null;
            }
        }
    }
}
