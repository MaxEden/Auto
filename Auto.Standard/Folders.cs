using System.IO;
using System.Linq;
using System.Reflection;
using Solution = Auto.Projects.Solution;

namespace Auto
{
    public abstract class Folders
    {
        protected readonly Logger _logger;

        public abstract FileInfo      SolutionPath { get; }
        public abstract DirectoryInfo RootDir      { get; }

        private DirectoryInfo _tempDir;
        public virtual DirectoryInfo TempDir
        {
            get
            {
                if (_tempDir != null) return _tempDir;

                _tempDir = RootDir.S("Temp").AsDir;
                if (!_tempDir.Exists)
                {
                    _tempDir.Create();
                    _logger.Info?.Invoke($"TempDir:{_tempDir.FullName} created");
                }
                return _tempDir;
            }
        }
        public virtual DirectoryInfo OutputDir
        {
            get
            {
                if (_outputDir != null) return _outputDir;

                _outputDir = RootDir.S("Output").AsDir;
                if (!_outputDir.Exists)
                {
                    _outputDir.Create();
                    _logger.Info?.Invoke($"OutputDir:{_outputDir.FullName} created");
                }
                return _outputDir;
            }
        }

        private Solution      _solution;
        private DirectoryInfo _outputDir;
        public Solution Solution
        {
            get
            {
                if (_solution != null) return _solution;
                _solution = new Solution();
                _solution.Parse(SolutionPath.FullName);
                return _solution;
            }
        }

        public static string SearchUp(string path, string pattern, params string[] ignore)
        {
            if (path == null) return null;

            var dir = Path.GetDirectoryName(path);
            if (dir == null) return null;
            string result = null;
            while (true)
            {
                result = Directory
                    .GetFiles(dir, pattern, SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(p => !ignore.Any(x => p.Contains(x)));

                if (result != null) return result;
                dir = Path.GetDirectoryName(dir);
            }
        }

        public static string[] SearchAllDown(string path, string pattern, params string[] ignore)
        {
            if (path == null) return null;

            var dir = path;
            if (!Directory.Exists(dir))
            {
                dir = Path.GetDirectoryName(path);
            }

            if (dir == null) return null;
            return Directory.GetFiles(dir, pattern, SearchOption.AllDirectories)
                .Where(p => !ignore.Any(x => p.Contains(x)))
                .ToArray();
        }

        public static string SearchDown(string path, string pattern, params string[] ignore)
        {
            return SearchAllDown(path, pattern).FirstOrDefault();
        }

        public Folders(Logger logger)
        {
            _logger = logger;
        }

        public static LocalFolders FromCurrentDll(Logger logger)
        {
            var dllDirectory = Assembly.GetEntryAssembly().Location.AsFileInfo().Directory;
            return new LocalFolders(logger, dllDirectory);
        }
    }

    public class LocalFolders : Folders
    {
        public DirectoryInfo DllDirectory { get; }
        
        public LocalFolders(Logger logger, DirectoryInfo dllDirectory) : base(logger)
        {
            this.DllDirectory = dllDirectory;
        }

        private FileInfo _solutionPath;

        public override FileInfo SolutionPath
        {
            get
            {
                if (_solutionPath != null) return _solutionPath;

                var dir = DllDirectory;
                string slnFile = SearchUp(dir.FullName, "*.sln");
                _solutionPath = slnFile.AsFileInfo();
                _logger.Info?.Invoke("SolutionPath resolved: " + _solutionPath);
                return _solutionPath;
            }
        }

        private DirectoryInfo _rootDirectory;

        public override DirectoryInfo RootDir
        {
            get
            {
                if (_rootDirectory != null) return _rootDirectory;
                _rootDirectory = SolutionPath.Directory;
                _logger.Info?.Invoke("RootDirectory resolved: " + _rootDirectory);
                return _rootDirectory;
            }
        }
    }
}