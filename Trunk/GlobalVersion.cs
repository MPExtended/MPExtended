#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Author information
[assembly: AssemblyCompany("mpextended.codeplex.com")]
[assembly: AssemblyCopyright("Copyright © 2011 MPExtended")]
[assembly: AssemblyTrademark("")]

// BUild type
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// This reflects the API version, only update this if the API changes, as dependent applications need to be rebuild when
// the assembly is replaced with an assembly with a different AssemblyVersion attribute. 
[assembly: AssemblyVersion("0.4.0.0")]

// This reflects the version of this unique build, and should be changed with each build. I hoped that this was possible
// using the TFS revision number but apparantly that's only possible with an add-in for Visual Studio. So instead, we
// put a static version here and hope people don't mess around too much with our assemblies. I use the .9.x release of 
// the previous version for beta and dev versions: 0.3.9.0 is a dev version of 0.4
[assembly: AssemblyFileVersion("0.3.9.0")]

// This is just an informational string, which has no technical meaning. We use it to show our version information in 
// the logs and communicate it to the client.
[assembly: AssemblyInformationalVersion("0.4.0-dev")]