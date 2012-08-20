using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MPExtended.Services.StreamingService")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("MPExtended StreamingService")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d8d5d4c0-010e-4400-a886-66d12737ad9d")]

// The hosting information
[assembly: MPExtended.Libraries.Service.Hosting.ServiceAssembly(
    Service = MPExtended.Libraries.Service.MPExtendedService.StreamingService,
    WCFType = typeof(MPExtended.Services.StreamingService.StreamingService),
    InitClass = typeof(MPExtended.Services.StreamingService.Code.Initialization),
    InitMethod = "Initialize")]
