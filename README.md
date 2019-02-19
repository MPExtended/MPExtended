MPExtended
==========

This is the main code repository of MPExtended. Source code of all our projects can be found here.

Developers
----------
These are some quick instructions on getting started with MPExtended:

1. Requirements:
   * Visual Studio 2010 or 2012
   * [Wix][2] plugin (if you want to build the installers)
   * [ASP.NET MVC][3] plugin (if you want to build WebMediaPortal)
   * [IIS Express][4] (if you want to test WebMediaPortal, IIS Express 8.0 might not work)
   * Optional: [ILMerge][5] (to build a single-binary PowerScheduler plugin)
2. Before you open the solution for the first time, run the ``Tools\restore-packages.bat`` script. This downloads the
   NuGet packages used by MPExtended. (You can leave this step out, but then Visual Studio will give an error on your
   first build).
3. Start Visual Studio as an administrator (elevated permissions are needed to start the WCF services)
4. Build the whole solution or at least the MPExtended.ServiceHosts.ConsoleHost project. This is needed to force
   Visual Studio to build the plugins.
5. Start the MPExtended.ServiceHosts.ConsoleHost project to start the service in the foreground
6. Start the MPExtended.Applications.WebMediaPortal project to start WebMediaPortal
7. There is some [documentation][6] for developers available in the MediaPortal wiki.

   [2]: http://wixtoolset.org/
   [3]: http://www.asp.net/downloads
   [4]: http://www.microsoft.com/en-us/download/details.aspx?id=1038
   [5]: http://www.microsoft.com/en-us/download/details.aspx?id=17630
   [6]: http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/Remote_Access/MPExtended/Developers/Getting_started