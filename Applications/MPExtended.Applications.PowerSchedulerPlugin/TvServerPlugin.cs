#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Linq;
using System.Text;
using MediaPortal.Common.Utils;
using SetupTv;
using TvControl;
using TvEngine;
using TvEngine.PowerScheduler;
using TvLibrary.Log;

namespace MPExtended.Applications.PowerSchedulerPlugin
{
    [CompatibleVersion("1.2.0.0")]
    [UsesSubsystem("TVE.Plugins.PowerScheduler")]
    public class TvServerPlugin : ITvServerPlugin
    {
        private PowerHandler handler;

        public string Author
        {
            get { return "MPExtended Developers"; }
        }

        public bool MasterOnly
        {
            get { return false; }
        }

        public string Name
        {
            get { return "MPExtended PowerScheduler plugin"; }
        }

        public SectionSettings Setup
        {
            get { return null; }
        }


        public string Version
        {
            get
            {
                // this is VersionUtil.GetVersionName(), but loading that whole assembly for these two lines is a bit overkill
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
            }
        }

        public void Start(IController controller)
        {
            try
            {
                Log.Debug("Starting and registering MPExtended powerhandler");
                handler = new PowerHandler();
                PowerScheduler.Instance.Register(handler);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to register MPExtended powerhandler: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }

        public void Stop()
        {
            try
            {
                Log.Debug("Removing MPExtended powerhandler");
                PowerScheduler.Instance.Unregister(handler);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to unregister MPExtended powerhandler: {0}\r\n{1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
