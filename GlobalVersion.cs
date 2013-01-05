#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Author information
[assembly: AssemblyCompany("mpextended.github.com")]
[assembly: AssemblyCopyright("Copyright © 2011 - 2013 MPExtended")]
[assembly: AssemblyTrademark("")]

// BUild type
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// This reflects the API version, only update this if the API changes, as dependent applications need to be rebuild when
// the assembly is replaced with an assembly with a different AssemblyVersion attribute. 
[assembly: AssemblyVersion("0.5.0.0")]

// This reflects the version of this unique build, and should be changed with each build. Sadly it's not easily possible
// to let this change with each build in Visual Studio, so we don't have that in our version numbers now and hope people
// don't mess around too much with our builds. For stable releases, this is of the format major.minor.bugfix.0. For new 
// minor versions we use major.prev-minor.99.x, with x incrementing with each alpha/beta/RC release. For test versions of
// bugfix releases this is major.minor.prev-bugfix.x, with x incrementing with each alpha/beta/RC release. This number
// is also used to check for new versions in the configurator, so please follow this scheme strictly. 
[assembly: AssemblyFileVersion("0.5.2.0")]

// This is just an informational string, which has no technical meaning. We use it to show our version information in 
// the logs and communicate it to the client.
[assembly: AssemblyInformationalVersion("0.5.2")]
