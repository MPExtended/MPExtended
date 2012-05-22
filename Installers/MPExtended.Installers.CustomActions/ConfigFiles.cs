#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Installers.CustomActions
{
    internal static class ConfigFiles
    {
        public static void Remove(string name)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", name);
                Log.Write("Deleting config file {0} at path {1}", name, path);
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Log.Write("Failed to delete config file: {0}", ex.Message);
            }
        }

        public static void RemoveDirectory(string name)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", name);
                Log.Write("Deleting config directory {0} at path {1}", name, path);
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                Log.Write("Failed to delete config directory: {0}", ex.Message);
            }
        }
    }
}
