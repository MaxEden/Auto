using System.IO;
using Auto;
using Auto.Projects;

namespace AutoLauncher.ServiceBuild
{
    public abstract class ServiceBuildSetup : Setup
    {
        public abstract string       ServiceName       { get; }
        public abstract FileInfo     ServiceOutputPath { get; }
        public abstract Remote.Login ServerLogin       { get; }

        protected Solution      Solution      { get; }
        public    DirectoryInfo TempDirectory { get; }
        public    DirectoryInfo RootDirectory { get; }

        public Names Names { get; } = new Names();

        public IBus     Bus      { get; }
        public Logger   Logger   { get; }
        public Measurer Measurer { get; }

        public ServiceBuildSetup(Context ctx) : this(ctx.Bus,
            ctx.Logger,
            ctx.LocalFolders.Solution,
            ctx.LocalFolders.TempDir,
            ctx.LocalFolders.RootDir) {}

        public ServiceBuildSetup(IBus bus,
                                 Logger logger,
                                 Solution solution,
                                 DirectoryInfo tempDirectory,
                                 DirectoryInfo rootDirectory)
        {
            Solution = solution;
            TempDirectory = tempDirectory;
            RootDirectory = rootDirectory;
            Logger = logger;
            Bus = bus;
            Measurer = new Measurer(Logger);
        }

        private LinuxService _linuxService;
        public LinuxService LinuxService => _linuxService ??= new LinuxService(
            Logger,
            ServerLogin,
            ServiceName,
            ServiceOutputPath,
            TempDirectory,
            Builder.OutputDirectory);

        private Builder _builder;
        public  Builder Builder => _builder ??= new DotnetBuild(Logger, Solution, RootDirectory, Measurer);
    }
}