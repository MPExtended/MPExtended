using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WebMediaPortal")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("WebMediaPortal")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version and copyright information is handled by GlobalVersion.cs

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7f418da7-f6af-4f27-8bfb-e418c12f5760")]

// Application initialization method
[assembly: PreApplicationStartMethod(typeof(MPExtended.Applications.WebMediaPortal.Code.Composition.ApplicationInitializer), "Initialize")]