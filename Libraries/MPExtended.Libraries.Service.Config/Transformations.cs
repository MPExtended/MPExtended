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
using System.IO;
using System.Text.RegularExpressions;

namespace MPExtended.Libraries.Service.Config
{
    public static class Transformations
    {
        public enum Transformation
        {
            FolderNames,
            Encrypt,
            Decrypt
        }

        private static Dictionary<Transformation, Func<string, string>> _transformers = new Dictionary<Transformation, Func<string, string>>();

        public static void RegisterCallback(Transformation type, Func<string, string> callback)
        {
            _transformers[type] = callback;
        }

        public static string Perform(Transformation type, string input)
        {
            return _transformers.ContainsKey(type) ? _transformers[type].Invoke(input) : input;
        }

        // convenience methods
        public static string FolderNames(string input)
        {
            return Perform(Transformation.FolderNames, input);
        }

        public static string Decrypt(string input)
        {
            return Perform(Transformation.Decrypt, input);
        }

        public static string Encrypt(string input)
        {
            return Perform(Transformation.Encrypt, input);
        }
    }
}
