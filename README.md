# Auto  
Auto is extremely lightweight, minimal dependensies, low ceremony, easily embedable build system highly inspired by NUKE.
Auto mostly consists of static methods that ensures that all dependencies are obvious and allows you to use it in any context.
Auto aims to be mostly a set of helper scripts that are independent from each other.
Some of auto methods such as related to SSH require installed GNU toolchain or cygwin.
Auto includes a launcher tool and a console build service but can be used without them directly and be integrated in you build pipeline as a library.

Example of a build script as a single file console app
```c#
using System.Linq;
using Auto;

var logger = Logger.ToConsole();
var folders = Folders.FromCurrentDll(logger);
var projectName = "BigBadSite";
var publishDir = folders.OutputDir.S("AutoPublish").AsDir;
var mesurer = new Measurer(logger);
var builder = new DotnetBuild(logger, folders.Solution, folders.RootDir, mesurer);

publishDir.Clear();
var output = builder.Publish(projectName, publishDir);

if (args.Contains("deploy"))
{
    var login = new Remote.Login("deploy", "36.138.16.182");
    var serviceName = "big-bad-site";
    var linuxService = new LinuxService(logger, login, serviceName, output, folders.TempDir, folders.OutputDir);
    linuxService.InstallAndDeploy();
}
```

the result is a crossplatform build tool of 198 KB size