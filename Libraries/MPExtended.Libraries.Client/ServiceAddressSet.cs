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
using System.Security.Cryptography;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Libraries.Client
{
    public class ServiceAddressSet : IServiceAddressSet
    {
        public string MAS { get; private set; }
        public string MASStream { get; private set; }

        public string TAS { get; private set; }
        public string TASStream { get; private set; }

        public string USS { get; private set; }

        public ServiceAddressSet(string mas, string masstream, string tas, string tasstream, string uss)
        {
            MAS = mas;
            MASStream = masstream;
            TAS = tas;
            TASStream = tasstream;
            USS = uss;
        }

        public ServiceAddressSet(string mas, string tas, string uss)
            : this (mas, mas, tas, tas, uss)
        {
        }

        public ServiceAddressSet(string mas, string tas)
            : this(mas, mas, tas, tas, null)
        {
        }

        public ServiceAddressSet(WebServiceSet set)
            : this (set.MAS, set.MASStream, set.TAS, set.TASStream, set.UI)
        {
        }

        public string GetSetIdentifier()
        {
            string name = String.Format("{0};{1};{2};{3};{4}", MAS, MASStream, TAS, TASStream, USS);

            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(name));
                return BitConverter.ToString(data).Replace("-", "");
            }
        }

        public IServiceSet Connect()
        {
            return new ServiceSet(this);
        }

        public IServiceSet Connect(string username, string password)
        {
            return new ServiceSet(this, username, password);
        }
    }
}
