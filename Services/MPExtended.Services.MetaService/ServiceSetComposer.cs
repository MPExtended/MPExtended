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
using System.Linq;
using System.Text;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal class ServiceSetComposer
    {
        public bool HasActiveMAS { get; set; }
        public bool HasActiveTAS { get; set; }
        public bool HasActiveWSS { get; set; }
        public bool HasActiveUI { get; set; }

        public string OurAddress { get; set; }

        private CompositionHinter hinter;

        public ServiceSetComposer()
        {
            hinter = new CompositionHinter();
        }

        public IEnumerable<WebServiceSet> ComposeUnique()
        {
            // This method removes all subsets of other sets, making sure only the largest sets are returned
            List<WebServiceSet> uniqueSets = new List<WebServiceSet>();

            foreach (WebServiceSet set in ComposeAll())
            {
                bool loopAgain = false;
                bool add = true;
                do
                {
                    foreach (WebServiceSet acceptedSet in uniqueSets)
                    {
                        if (acceptedSet.IsSubsetOf(set))
                        {
                            uniqueSets.Remove(acceptedSet);
                            loopAgain = true;
                            break;
                        }
                        else if (set.IsSubsetOf(acceptedSet) || set.IsSameAs(acceptedSet))
                        {
                            add = false;
                            break;
                        }
                    }
                } while (loopAgain);

                if (add)
                {
                    uniqueSets.Add(set);
                }
            }

            return uniqueSets;
        }

        protected IEnumerable<WebServiceSet> ComposeAll()
        {
            string tveAddress = hinter.GetConfiguredTVServerAddress();
            List<WebServiceSet> sets = new List<WebServiceSet>();

            // Start with the most simple case: full singleseat
            if (HasActiveMAS && HasActiveTAS && HasActiveWSS && HasActiveUI)
            {
                sets.Add(CreateServiceSet(OurAddress, OurAddress, OurAddress));
            }

            // Singleseat installation without using TV server
            if (HasActiveMAS && HasActiveWSS && HasActiveUI)
            {
                sets.Add(CreateServiceSet(OurAddress, null, OurAddress));
            }

            // Multiseat installation with UI + MAS + WSS on client and TAS + WSS on server (and we're the client)
            if (HasActiveMAS && HasActiveWSS && HasActiveUI && !HasActiveTAS && tveAddress != null)
            {
                // TODO: check tveAddress and validate it doesn't have MAS and UI
                sets.Add(CreateServiceSet(OurAddress, tveAddress, OurAddress));
            }

            // Same as previous, but now we're the server
            if (HasActiveTAS && HasActiveWSS && !HasActiveMAS && !HasActiveUI)
            {
                sets.Add(CreateServiceSet(null, OurAddress, null));
            }

            // Multiseat installation with MAS + TAS + WSS on server and only UI on the client (we're the client)
            if (HasActiveUI && !HasActiveMAS && !HasActiveTAS && !HasActiveWSS)
            {
                // TODO: check tveAddress and validate it has MAS
                sets.Add(CreateServiceSet(tveAddress, tveAddress, OurAddress));
            }

            // Idem, but now we're the server
            if (!HasActiveUI && HasActiveMAS && HasActiveTAS && HasActiveWSS)
            {
                sets.Add(CreateServiceSet(OurAddress, OurAddress, null));
            }

            // TODO: external WSS

            return sets;
        }

        private WebServiceSet CreateServiceSet(string mas, string masstream, string tas, string tasstream, string ui)
        {
            return new WebServiceSet()
            {
                MAS = mas,
                MASStream = masstream,
                TAS = tas,
                TASStream = tasstream,
                UI = ui
            };
        }

        private WebServiceSet CreateServiceSet(string mas, string tas, string ui)
        {
            return CreateServiceSet(mas, mas, tas, tas, ui);
        }
    }
}
