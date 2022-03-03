using System.IO;
using Auto.Projects;

namespace Auto
{
    public class DotnetBuild : Builder
    {
        public DotnetBuild(Logger logger, Solution solution, DirectoryInfo rootDirectory, Measurer measurer)
            : base(logger, solution, rootDirectory, measurer) {}

        protected override string BuildProject(Logger logger, string projectPath, bool noDeps, string outputPath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var args = new Args("build", projectPath);
            if(!string.IsNullOrWhiteSpace(outputPath))
            {
                args.Add("--output", outputPath);
            }
            
            if(noDeps) args.Add("--no-restore", "--no-dependencies");

            var output = CLI.RunAndRead(logger, "dotnet", args);
            var releaseDll = GetOutputPathFromStdOut(output, projectName);
            return releaseDll.FullName;
        }

        protected override void NotifyFileChanged(string path) {}
    }
}