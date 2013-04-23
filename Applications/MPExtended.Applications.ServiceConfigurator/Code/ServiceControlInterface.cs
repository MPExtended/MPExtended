#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class ServiceControlInterface
    {
        public string ServiceName { get; private set; }
        public bool IsServiceAvailable { get; private set; }

        private ServiceController serviceController;
        private DispatcherTimer serviceWatcher;
        private Label stateLabel;
        private Button triggerButton;

        public ServiceControlInterface(string serviceName, Label label, Button button)
        {
            ServiceName = serviceName;
            stateLabel = label;
            triggerButton = button;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                serviceController = new ServiceController(ServiceName);
                HandleServiceState(serviceController.Status);
                IsServiceAvailable = true;

                triggerButton.IsEnabled = true;
            }
            catch (InvalidOperationException)
            {
                serviceController = null;
                IsServiceAvailable = false;

                stateLabel.Content = UI.ServiceNotInstalled;
                triggerButton.IsEnabled = false;
            }
        }

        public void StartServiceWatcher()
        {
            if (!IsServiceAvailable)
                throw new InvalidOperationException("This service is unavailable");

            serviceWatcher = new DispatcherTimer();
            serviceWatcher.Interval = TimeSpan.FromSeconds(1);
            serviceWatcher.Tick += delegate(object sender, EventArgs e)
            {
                try
                {
                    serviceController.Refresh();
                    HandleServiceState(serviceController.Status);
                }
                catch (Win32Exception ex)
                {
                    Log.Info("Win32Exception in serviceWatcher (this can be caused by an upgrade or other normal action): {0}", ex.Message);
                    serviceWatcher.Stop();
                }
                catch (Exception ex)
                {
                    ErrorHandling.ShowError(ex); // TODO: is this correct
                    serviceWatcher.Stop();
                }
            };
            serviceWatcher.Start();
        }

        public void TriggerButtonClick()
        {
            if (!UacServiceHelper.IsAdmin())
            {
                Log.Debug("StartStopService: no admin rights, use UacServiceHandler");
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        UacServiceHelper.StartService(ServiceName);
                        break;
                    case ServiceControllerStatus.Running:
                        UacServiceHelper.StopService(ServiceName);
                        break;
                }
            }
            else
            {
                Log.Debug("StartStopService: have admin rights, start/stop ourselves");
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        serviceController.Start();
                        break;
                    case ServiceControllerStatus.Running:
                        serviceController.Stop();
                        break;

                }
            }
        }

        private void HandleServiceState(ServiceControllerStatus _status)
        {
            switch (_status)
            {
                case ServiceControllerStatus.Stopped:
                    triggerButton.Content = UI.Start;
                    stateLabel.Content = UI.ServiceStopped;
                    stateLabel.Foreground = Brushes.Red;
                    break;
                case ServiceControllerStatus.Running:
                    triggerButton.Content = UI.Stop;
                    stateLabel.Content = UI.ServiceStarted;
                    stateLabel.Foreground = Brushes.Green;
                    break;
                case ServiceControllerStatus.StartPending:
                    triggerButton.Content = UI.Stop;
                    stateLabel.Content = UI.ServiceStartingFixed;
                    stateLabel.Foreground = Brushes.Teal;
                    break;
                default:
                    triggerButton.Foreground = Brushes.Teal;
                    stateLabel.Content = UI.ServiceUnknown;
                    stateLabel.Foreground = Brushes.Teal;
                    break;
            }
        }
    }
}
