using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MPExtended.Services.TVAccessService")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("MPExtended TVAccessService")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7a7401f2-ea7d-4b02-ba47-9e21c620f7a8")]

// The hosting information
[assembly: MPExtended.Libraries.Service.Hosting.ServiceAssembly(
    Service = MPExtended.Libraries.Service.MPExtendedService.TVAccessService,
    WCFType = typeof(MPExtended.Services.TVAccessService.TVAccessService),
    InitClass = typeof(MPExtended.Services.TVAccessService.LogoDownloader),
    InitMethod = "Setup")]