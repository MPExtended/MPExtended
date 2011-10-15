#include "common.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly:AssemblyTitleAttribute("MPExtended.Libraries.VLCManaged")];
[assembly:AssemblyProductAttribute("MPExtended VLCManaged Wrapper")];
[assembly:AssemblyDescriptionAttribute("")];
[assembly:AssemblyCultureAttribute("")];

[assembly:AssemblyCompanyAttribute("mpextended.github.com")];
[assembly:AssemblyCopyrightAttribute("Copyright (c) 2011 MPExtended")];
[assembly:AssemblyTrademarkAttribute("")];
[assembly:AssemblyConfigurationAttribute("Release")];

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:

[assembly:AssemblyVersionAttribute(VERSION)];
[assembly:ComVisible(false)];
[assembly:CLSCompliantAttribute(true)];
[assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
