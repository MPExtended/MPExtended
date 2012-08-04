#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ServiceModel;
using System.Text;
using MPExtended.Services.MetaService.Interfaces;
using TvEngine.PowerScheduler.Interfaces;
using TvLibrary.Log;

namespace MPExtended.Applications.PowerSchedulerPlugin
{
    internal class PowerHandler : IStandbyHandler
    {
        private readonly TimeSpan maxIdleTime = TimeSpan.FromSeconds(5 * 60);
        private long invocationCounter = 0;
        private bool cachedResult = true;

        public bool DisAllowShutdown
        {
            get
            {
                // The TVEngine calls this method once a second. Yes, each second. This happens because of some complicated internals of power handling in Windows and
                // the TV Server. See MediaPortal pull request #11 (https://github.com/MediaPortal/MediaPortal-1/pull/11) for details, but it can't be easily fixed.
                // As such, we have to workaround this. We only load the real activity once a minute. All other invocations we just return the value we returned the last
                // time we checked. 

                if (invocationCounter++ % 60 != 0)
                {
                    return cachedResult;
                }

                try
                {
                    DateTime? lastDateTime = LoadLastActivity();

                    if (lastDateTime.HasValue)
                    {
                        TimeSpan diff = DateTime.Now - lastDateTime.Value;
                        cachedResult = diff < maxIdleTime;
                        Log.Debug("MPExtendedPowerHandler.DisAllowShutdown: last activity is {0}, difference is {1}, thus returning {2}", 
                            lastDateTime.Value, diff, cachedResult);
                    }
                    else
                    {
                        Log.Debug("MPExtendedPowerHandler.DisAllowShutdown: failed to load last activity, returning false (allowing shutdown)");
                        cachedResult = true;
                    }

                    return cachedResult;
                }
                catch (Exception ex)
                {
                    Log.Error("Error in MPExtended.Applications.PowerSchedulerPlugin.PowerHandler.DisAllowShutdown: {0}", ex);
                    return false;
                }
            }
        }

        public string HandlerName
        {
            get { return "MPExtendedKeepAwake"; }
        }

        public void UserShutdownNow()
        {
            // hmm, that's too bad but we can't do anything about it
        }

        private DateTime? LoadLastActivity()
        {
            DateTime? retValue;
            IProtectedMetaService connection = null;

            try
            {
                ChannelFactory<IProtectedMetaService> factory = new ChannelFactory<IProtectedMetaService>(
                    new NetNamedPipeBinding() { MaxReceivedMessageSize = 100000000 },
                    new EndpointAddress("net.pipe://127.0.0.1/MPExtended/MetaService")
                );

                connection = factory.CreateChannel();
                retValue = connection.GetLastClientActivity();
            }
            catch (Exception ex)
            {
                Log.Error("Error in MPExtended.Applications.PowerSchedulerPlugin.PowerHandler.LoadLastActivity: {0}", ex);
                retValue = null;
            }
            finally
            {
                DisposeConnection(connection);
            }

            return retValue;
        }

        private void DisposeConnection<T>(T connection)
        {
            if (connection != null)
            {
                try
                {
                    (connection as ICommunicationObject).Close();
                }
                catch (Exception)
                {
                    // ignore it, we don't need the connection anyway
                }
            }
        }
    }
}
