﻿#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
//
// The use and distribution terms for this software are covered by the
// Common Public License 1.0 (http://opensource.org/licenses/cpl1.0.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by
// the terms of this license.
//    
// You must not remove this notice, or any other, from this software.
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
        private const string UPDATE_FILE = @"https://raw.githubusercontent.com/MPExtended/WifiRemote/master/Installer/update.xml";

        private static Version installedVersion;
        private static Version onlineVersion;

        public static ActionResult Install(Session session)
        {
            // fail very early if we can't find MPEI
            string mpei = LookupMPEI();
            if (mpei == null)
            {
                Log.Write("WifiRemote: MPEI not available");
                return ActionResult.NotExecuted;
            }
            Log.Write("WifiRemote: Found MPEI at {0}", mpei);
    
            // download version information
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Downloading WifiRemote update information...", ""));
            Uri downloadUri;
            InitializeVersionInformation(session, out downloadUri);

            // fail early if we couldn't find the online version
            if (onlineVersion == null)
            {
                Log.Write("WifiRemote: Online version unavailable, aborting installation...");
                return ActionResult.Success;
            }

            // don't do anything if installed version is the newest one
            if (installedVersion != null && installedVersion >= onlineVersion)
            {
                Log.Write("WifiRemote: Installed version is the last one, doing nothing...");
                return ActionResult.Success;
            }

            // setup for installation
            string mpeiPackagePath = Path.GetTempFileName();
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Downloading WifiRemote installer...", ""));
            if (!DownloadWifiRemote(downloadUri, mpeiPackagePath))
            {
                Log.Write("WifiRemote: Failed to download version from internet, aborting installation...");
                return ActionResult.Success;
            }
            Log.Write("WifiRemote: Got WifiRemote installer in {0}", mpeiPackagePath);

            // execute
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Installing WifiRemote MediaPortal plugin with MPEI...", ""));
            ProcessStartInfo param = new ProcessStartInfo();
            param.FileName = mpei;
            param.Arguments = mpeiPackagePath + " /S";
            Log.Write("WifiRemote: Starting MPEI with arguments {0}", param.Arguments);
            Process proc = new Process();
            proc.StartInfo = param;
            proc.Start();
            proc.WaitForExit();
            Log.Write("WifiRemote: MPEI finished with exit code {0}", proc.ExitCode);

            // cleanup
            File.Delete(mpeiPackagePath);
            return ActionResult.Success;
        }

        private static void InitializeVersionInformation(Session session, out Uri downloadUri)
        {
            onlineVersion = GetOnlineVersion(out downloadUri);
            installedVersion = GetCurrentlyInstalledVersion();

            Log.Write("WifiRemote: {0} is available online and {1} is installed", onlineVersion, installedVersion);
        }

        private static Version GetOnlineVersion(out Uri downloadUri)
        {
            downloadUri = null;
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
                return null;
            }

            // parse it
            try
            {
                XElement updateFile = XElement.Parse(xmlData);

                Version latest = null;
                foreach (XElement e in updateFile.Element("Items").Elements("PackageClass"))
                {
                    // get wifiremote version
                    XElement versionNode = e.Element("GeneralInfo").Element("Version");

                    Version v = new Version(
                                Int32.Parse(versionNode.Element("Major").Value),
                                Int32.Parse(versionNode.Element("Minor").Value),
                                Int32.Parse(versionNode.Element("Build").Value),
                                Int32.Parse(versionNode.Element("Revision").Value)
                             );

                    if (latest == null || v > latest)
                    {
                        downloadUri = new Uri(e.Element("GeneralInfo").Element("OnlineLocation").Value.ToString());
                        latest = v;
                    }
                }
                return latest;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to parse update.xml: {0}\r\n{1}\r\n{2}", ex.Message, ex.ToString(), ex.StackTrace.ToString());
                return null;
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

        private static Version GetCurrentlyInstalledVersion()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Team MediaPortal", "MediaPortal", "Installer", "V2", "InstalledExtensions.xml");

                if (!File.Exists(path))
                {
                    Log.Write("WifiRemote: Did not find MPEI database at {0}", path);
                    return null;
                }

                // load file
                XElement file = XElement.Load(path);
                var nodes = file.Element("Items").Elements("PackageClass").Where(x => x.Element("GeneralInfo").Element("Name").Value == "WifiRemote");
                if (nodes.Count() == 0)
                {
                    Log.Write("WifiRemote: No local installation found");
                    return null;
                }

                // create version
                XElement versionNode = nodes.First().Element("GeneralInfo").Element("Version");
                Version version = new Version(
                    Int32.Parse(versionNode.Element("Major").Value),
                    Int32.Parse(versionNode.Element("Minor").Value),
                    Int32.Parse(versionNode.Element("Build").Value),
                    Int32.Parse(versionNode.Element("Revision").Value)
                );
                Log.Write("WifiRemote: Version {0} currently installed", version);
                return version;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to load currently installed version", ex.Message);
                return null;
            }
        }

        private static string LookupMPEI()
        {
            try
            {
                string[] keys = new string[] {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal",
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
                };
                RegistryKey key = null;
                foreach (var keyPath in keys)
                {
                    key = Registry.LocalMachine.OpenSubKey(keyPath);
                    if (key != null)
                    {
                        break;
                    }
                }

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
