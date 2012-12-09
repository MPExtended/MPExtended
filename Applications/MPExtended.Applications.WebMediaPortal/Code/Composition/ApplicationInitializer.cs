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
using System.Linq;
using System.Web;
using System.Web.Compilation;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Composition;
using MPExtended.Libraries.Service.Hosting;

namespace MPExtended.Applications.WebMediaPortal.Code.Composition
{
    public sealed class ApplicationInitializer
    {
        public static void Initialize()
        {
            // This method is called by the ASP.NET infrastructure using a PreApplicationStartMethodAttribute.
            LogRotation.Rotate();
            Log.Setup("WebMediaPortal.log", false);
            Log.Debug("WebMediaPortal starting!");
            Installation.Load(MPExtendedProduct.WebMediaPortal);
            InitializeExtensions();
        }

        private static void InitializeExtensions()
        {
            // This method is only needed to make references to types in an extension assembly from Razor working. Unfortunately
            // the BuildManager.AddReferencedAssembly() method has to be called very early in the application life cycle, so that
            // we cannot do it in Global.asax.cs as usual.

            var directoriesAdded = new List<string>();

            Composer.Instance.Compose();
            var assemblies = Composer.Instance.GetAllAssemblies();
            foreach (var assembly in assemblies)
            {
                var directory = Path.GetDirectoryName(assembly.Location);
                Log.Debug("Loading assembly {0} into Razor BuildManager", assembly.Location);
                if (!directoriesAdded.Contains(directory))
                {
                    AssemblyLoader.Install();
                    AssemblyLoader.AddSearchDirectory(directory);
                }

                BuildManager.AddReferencedAssembly(assembly);
            }
        }
    }
}