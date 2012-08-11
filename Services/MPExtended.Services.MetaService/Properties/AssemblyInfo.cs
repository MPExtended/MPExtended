using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MPExtended.Services.MetaService")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("MPExtended MetaService")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("49906529-1749-43c1-b7f5-9e609e9df23d")]

// The hosting information
[assembly: MPExtended.Libraries.Service.Hosting.ServiceAssembly(
    Service = MPExtended.Libraries.Service.MPExtendedService.MetaService,
    WCFType = typeof(MPExtended.Services.MetaService.MetaService))]
