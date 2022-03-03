using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Auto.Projects;

namespace Auto
{
    public abstract class Builder
    {
        public readonly Solution      Solution;
        public readonly DirectoryInfo RootDirectory;
        public          DirectoryInfo OutputDirectory => RootDirectory.S("output").AsDir;

        private readonly Measurer _measurer;
        private readonly Logger   _logger;

        public Builder(Logger logger, Solution solution, DirectoryInfo rootDirectory, Measurer measurer)
        {
            _logger = logger;
            Solution = solution;
            RootDirectory = rootDirectory;
            _measurer = measurer;
        }

        public bool IsBuilt(string projectName) => BuiltOutputFiles.ContainsKey(projectName);

        public void EnsureBuilt(string projectName)
        {
            if(!IsBuilt(projectName))
            {
                throw new Exception($"{projectName} hasn't been built yet");
            }
        }

        public FileInfo OutputPath(string projectName)
        {
            EnsureBuilt(projectName);
            BuiltOutputFiles.TryGetValue(projectName, out var output);
            return output;
        }

        public DirectoryInfo OutputDirectoryOf(string projectName)
        {
            return OutputPath(projectName).Directory;
        }

        public Project Proj(string name) => this.Solution.GetProject(name);

        public Dictionary<string, FileInfo> BuiltOutputFiles = new Dictionary<string, FileInfo>();

        public FileInfo Build(string projectName, bool noDeps = false, string output = null)
        {
            using(_measurer.Measure(projectName))
            {
                var proj = Proj(projectName);
                if(proj == null)
                    throw new ArgumentException(
                        $"Project \"{projectName}\" not found! Possibly not added to solution {Solution.Path}");


                var targetPath = BuildProject(_logger, proj.File.FullName, noDeps, output).AsFileInfo();
                if(targetPath != null) BuiltOutputFiles[projectName] = targetPath;

                return targetPath;
            }
        }

        public FileInfo BuildAtPath(FileInfo projectPath, bool noDeps = false, string output = null)
        {
            using(_measurer.Measure(projectPath.Name))
            {
                if(Directory.Exists(output))
                    output = Path.Join(output, Path.GetFileNameWithoutExtension(projectPath.Name) + ".dll");

                var targetPath = BuildProject(_logger, projectPath.FullName, noDeps,  output).AsFileInfo();
                if(targetPath != null) BuiltOutputFiles[projectPath.Name] = targetPath;

                return targetPath;
            }
        }

        public FileInfo Publish(string projectName, DirectoryInfo outputDir)
        {
            using(_measurer.Measure(projectName))
            {
                var proj = Proj(projectName);
                if(proj == null)
                    throw new ArgumentException(
                        $"Project \"{projectName}\" not found! Possibly not added to solution {Solution.Path}");

                var conf = "Release";
                var output = CLI.RunAndRead(
                    _logger,
                    "dotnet",
                    "publish",
                    proj.File.FullName,
                    "--no-restore",
                    "--verbosity",
                    "m",
                    "--configuration",
                    conf,
                    "--nologo",
                    "--output",
                    outputDir.FullName
                );

                var targetPath = GetPublishedPathFromStdOut(output, projectName);
                BuiltOutputFiles[projectName] = targetPath;
                return targetPath;
            }
        }

        public static FileInfo GetOutputPathFromStdOut(List<CLI.OutputLine> output, string projectName)
        {
            var lines = output.ToList();
            var outputLine = lines.First(p => p.Type == CLI.OutputType.Std && p.Text.Trim().StartsWith(projectName));
            var targetFile = outputLine.Text.Split("->", StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            return targetFile.AsFileInfo();
        }

        public FileInfo GetPublishedPathFromStdOut(List<CLI.OutputLine> output, string projectName)
        {
            var releaseDll = GetOutputPathFromStdOut(output, projectName);
            var dllName = releaseDll.Name;

            var lines = output.ToList();
            var outputLine = lines.Last(p => p.Type == CLI.OutputType.Std && p.Text.Trim().StartsWith(projectName));
            var targetFile = outputLine.Text.Split("->", StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            targetFile = new DirectoryInfo(targetFile).GetFiles(dllName, SearchOption.AllDirectories).First().FullName;

            var targetPath = targetFile.AsFileInfo();
            return targetPath;
        }

        public List<FileInfo> GetBuildOutput(string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            var files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            var ends = new[]
            {
                ".dll",
                ".pdb",
                ".orig"
            };

            files.RemoveAll(p => !ends.Any(p.Name.EndsWith));
            return files;
        }

        public void FileChanged(string path)
        {
            NotifyFileChanged(path);
        }

        //=============

        protected abstract string BuildProject(Logger logger, string outputPath, bool noDeps, string s);
        protected abstract void NotifyFileChanged(string path);
    }
}