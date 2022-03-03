using Log;
using Log.Sinks;
using MandarinAuto;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace Auto.Build
{
    public class Program : AutoProgram
    {
        private MBSetup _mbs;

        public override void Setup(Context context)
        {
            _mbs = new MBSetup(new ConsoleLogSink(), new Solution(), null, null);
            
            context.Commands.Add("watch", () => _mbs.Watch());
            context.Commands.Add("compile", () => _mbs.Mandarin.CompileFull());
            context.Commands.Add("unity before build", () => _mbs.Mandarin.UnityBeforeBuild());
            context.Commands.Add("unity after build", () => _mbs.Mandarin.UnityAfterBuild());
        }

        public static void Main()
        {
            var program = new Program();
            program.Start();
        }
    }

    class MBSetup : MandarinBuildSetup
    {
        public override string GameName    => "Checkers Boom";
        public override string ServiceName => "chex_game";

        public override string PackageName      => "com.Poska.ChexBoom";
        public override string UnityGameDirName => "UnityGame";

        public MBSetup(ILogSink logSink, Solution solution, AbsolutePath temporaryDirectory, AbsolutePath rootDirectory) : base(logSink, solution,
            temporaryDirectory,
            rootDirectory) {}
    }
}