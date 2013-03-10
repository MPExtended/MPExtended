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
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using ZeroconfService;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Libraries.Client
{
    public class ServiceDiscoverer : IServiceDiscoverer
    {
        public event EventHandler<SetDiscoveredEventArgs> SetDiscovered;

        internal const int DEFAULT_PORT = 4322;

        private const int CONNECT_TIMEOUT = 2500; // in milliseconds
        private const int READ_TIMEOUT = 15000; // in milliseconds

        private Thread discoveringThread;
        private List<Task> waitingTasks;
        private List<WebServiceSet> serviceSets = new List<WebServiceSet>();
        private List<string> hintedAddresses = new List<string>();

        public ServiceDiscoverer()
        {
            Hint("127.0.0.1");
        }

        public void Hint(string ip)
        {
            hintedAddresses.Add(ip);
        }

        public IEnumerable<IServiceAddressSet> DiscoverSets(TimeSpan timeout)
        {
            StartDiscoveryAsync();
            Thread.Sleep(timeout);
            StopDiscovery();

            return serviceSets.Select(x => new ServiceAddressSet(x));
        }

        public void StartDiscoveryAsync()
        {
            if (discoveringThread != null)
            {
                throw new InvalidOperationException("Service discovery is already in process");
            }

            waitingTasks = new List<Task>();
            discoveringThread = new Thread(DoDiscovery);
            discoveringThread.Start();
        }

        public void StopDiscovery()
        {
            if (discoveringThread == null)
            {
                throw new InvalidOperationException("Service discovery not started");
            }

            discoveringThread.Abort();
            discoveringThread = null;

            // make sure to finish all still waiting tasks
            Task.WaitAll(waitingTasks.ToArray());
        }

        private void DoDiscovery()
        {
            // setup zeroconf, if available
            Zeroconf zc = null;
            if(Zeroconf.CheckAvailability())
            {
                zc = new Zeroconf();
                zc.StartDiscovery(delegate(string ip, int port, string meta)
                {
                    lock (waitingTasks)
                    {
                        waitingTasks.Add(Task.Factory.StartNew(() => FoundAddress(meta)));
                    }
                });
            }

            // do all hinted ports
            lock (waitingTasks)
            {
                foreach (string ip in hintedAddresses)
                {
                    waitingTasks.Add(Task.Factory.StartNew(() => FoundAddress(String.Format("http://{0}:{1}/", ip, DEFAULT_PORT))));
                }
            }
        }

        private void FoundAddress(string metaAddress)
        {
            // establish a meta connection and load all sets, if possible
            IEnumerable<WebServiceSet> sets;
            try
            {
                IMetaService meta = CreateMeta(metaAddress);
                if (meta == null)
                {
                    return;
                }

                sets = meta.GetActiveServiceSets();
                if (sets == null || sets.Count() == 0)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            // loop through sets and remove all subsets of this set (and dump this set if we already have a bigger one)
            lock (serviceSets)
            {
                foreach (WebServiceSet newSet in sets)
                {
                    bool loopAgain = false;
                    bool add = true;
                    do
                    {
                        loopAgain = false;
                        foreach (WebServiceSet acceptedSet in serviceSets)
                        {
                            if (acceptedSet.IsSubsetOf(newSet))
                            {
                                serviceSets.Remove(acceptedSet);
                                loopAgain = true;
                                break;
                            }
                            else if (newSet.IsSubsetOf(acceptedSet) || newSet.IsSameAs(acceptedSet))
                            {
                                add = false;
                                break;
                            }
                        }
                    } while (loopAgain && add);

                    if (add)
                    {
                        // yeah, this seems to be a valid set. Add it and notify clients.
                        serviceSets.Add(newSet);

                        if (SetDiscovered != null)
                        {
                            SetDiscovered(this, new SetDiscoveredEventArgs(new ServiceAddressSet(newSet)));
                        }
                    }
                }
            }
        }

        private IMetaService CreateMeta(string rootAddress)
        {
            Console.WriteLine("Creating meta for {0}", rootAddress);
            try
            {
                BasicHttpBinding binding = new BasicHttpBinding()
                {
                    MaxReceivedMessageSize = 100000000,

                    OpenTimeout = TimeSpan.FromMilliseconds(CONNECT_TIMEOUT),
                    SendTimeout = TimeSpan.FromMilliseconds(READ_TIMEOUT),
                    ReceiveTimeout = TimeSpan.FromMilliseconds(READ_TIMEOUT),
                    CloseTimeout = TimeSpan.FromMilliseconds(CONNECT_TIMEOUT),
                };

                ChannelFactory<IMetaService> factory = new ChannelFactory<IMetaService>(
                    binding,
                    new EndpointAddress(rootAddress + "MPExtended/MetaService")
                );

                IMetaService channel = factory.CreateChannel();
                if (!channel.TestConnection())
                {
                    return null;
                }

                return channel;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
