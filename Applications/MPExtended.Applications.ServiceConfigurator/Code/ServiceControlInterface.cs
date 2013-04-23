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
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class ServiceControlInterface
    {
        private const int _restartTimeout = 10000;

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
                stateLabel.Foreground = Brushes.Red;
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
                    ErrorHandling.ShowError(ex); // TODO: is this correct?
                    serviceWatcher.Stop();
                }
            };
            serviceWatcher.Start();
        }

        public void TriggerButtonClick()
        {
            if (!UacServiceHelper.IsAdmin())
            {
                Log.Debug("TriggerButtonClick: no admin rights, use UacServiceHandler for service '{0}'", ServiceName);
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
                Log.Debug("TriggerButtonClick: have admin rights, start or stop service '{0}' ourselves", ServiceName);
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

        public void RestartService()
        {
            RestartService(false);
        }

        public void RestartService(bool background)
        {
            if (!UacServiceHelper.IsAdmin())
            {
                Log.Debug("RestartService: no admin rights, use UacServiceHandler to restart service '{0}'", ServiceName);
                UacServiceHelper.RestartService(ServiceName);
            }
            else
            {
                Log.Debug("RestartService: have admin rights, restart service '{0}' ourselves", ServiceName);
                var task = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            PerformRestart(_restartTimeout);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn(String.Format("RestartService: failed to restart service '{0}'", ServiceName), ex);
                        }
                    });
                if (!background)
                    task.Wait();
            }
        }

        private void PerformRestart(int timeoutMilliseconds)
        {
            // The timeout is the total timeout for two operations, so we reduce the timeout with the elapsed time after the first operation.
            int startTime = Environment.TickCount;
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            if (serviceController.Status == ServiceControllerStatus.Running)
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }

            timeout -= TimeSpan.FromTicks(Environment.TickCount - startTime);
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
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
