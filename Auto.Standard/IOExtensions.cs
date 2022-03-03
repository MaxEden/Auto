using System;
using System.IO;
using System.Linq;
using Solution = Auto.Projects.Solution;

namespace Auto
{
    public static class IOExtensions
    {
        public static FileInfo S(this DirectoryInfo dir, FileInfo file)
        {
            return new FileInfo(Path.Join(dir.ToString(), file.ToString()));
        }

        public static UnknownPath S(this DirectoryInfo dir, params string[] parts)
        {
            var combine = new[] { dir.FullName }.Concat(parts).ToArray();
            return new UnknownPath(Path.Join(combine));
        }

        public static UnknownPath S(this FileInfo dir, params string[] parts)
        {
            var combine = new[] { dir.FullName }.Concat(parts).ToArray();
            return new UnknownPath(Path.Join(combine));
        }

        public static UnknownPath S(this UnknownPath dir, params string[] parts)
        {
            var combine = new[] { dir.FullName }.Concat(parts).ToArray();
            return new UnknownPath(Path.Join(combine));
        }

        public static FileInfo AsFileInfo(this string filePath)
        {
            if(filePath == null) return null;
            return new FileInfo(filePath);
        }

        public static DirectoryInfo AsDirInfo(this string filePath)
        {
            if(filePath == null) return null;
            return new DirectoryInfo(filePath);
        }
        
        public static void CleanBinObj(this Solution solution)
        {
            if(solution?.Projects == null || solution.Projects.Count == 0) return;
            
            foreach(var project in solution.Projects)
            {
                if(project.Name.Contains("_build")) continue;
                project.Directory.S( @"bin").Delete();
                project.Directory.S( @"obj").Delete();
            }
        }
        
        public static void Clear(this DirectoryInfo path)
        {
            if(path.Exists)
            {
                path.Delete( true);
            }
            path.Create();
        }

        public static void CopyTo(this DirectoryInfo source, DirectoryInfo destination, CopyOptions options = null)
        {
            foreach(var directoryInfo in source.GetDirectories())
            {
                if(options?.DirectoryFilter != null && !options.DirectoryFilter(directoryInfo))
                {
                    continue;
                }

                var subPath = directoryInfo.FullName.Substring(source.FullName.Length);
                var newDirPath = Path.Join(destination.FullName, subPath);
                var newDir = Directory.CreateDirectory(newDirPath);
                CopyTo(directoryInfo, newDir, options);
            }

            foreach(var fileInfo in source.GetFiles())
            {
                if(options?.FileFilter != null && !options.FileFilter(fileInfo))
                {
                    continue;
                }

                var subPath = fileInfo.FullName.Substring(source.FullName.Length);
                var newFile = Path.Join(destination.FullName, subPath);
                fileInfo.CopyTo(newFile, true);
            }
        }
        
        public static void CopyToDirectoryIfDiffers(
            this FileInfo source,
            string targetDirectory,
            bool createDirectories = true)
        {
            var path = Path.Join(targetDirectory, source.Name);
            if(IsText(source.Name))
            {

                if(File.Exists(path))
                {
                    var oldContent = File.ReadAllText(path);
                    var newContent = File.ReadAllText(source.FullName);
                    if(oldContent == newContent) return;
                }
            }

            Directory.CreateDirectory(targetDirectory);
            source.CopyTo(path, true);
        }

        private static bool IsText(string path)
        {
            return path.EndsWith(".txt") || path.EndsWith(".xml") || path.EndsWith(".json") || path.EndsWith(".jslib") || path.EndsWith(".cs");
        }
        
        public static void WriteAllTextIfDiffers(this FileInfo path, string content)
        {
            if(path.Exists)
            {
                var oldContent = File.ReadAllText(path.FullName);
                if(oldContent == content) return;
            }

            File.WriteAllText(path.FullName, content);
        }
    }

    public class UnknownPath
    {
        public readonly string FullName;

        public UnknownPath(string fullName)
        {
            FullName = fullName;
        }

        public FileInfo      AsFile => new FileInfo(FullName);
        public DirectoryInfo AsDir  => new DirectoryInfo(FullName);

        public static implicit operator string(UnknownPath path)
        {
            return path.FullName;
        }

        public override string ToString()
        {
            return FullName;
        }
        
        public void Delete()
        {
            if(Directory.Exists(FullName))
            {
                Directory.Delete(FullName, true);
            }

            if(File.Exists(FullName))
            {
                File.Delete(FullName);
            }
        }
    }
    
    public class CopyOptions
    {
        public Func<FileInfo, bool>      FileFilter;
        public Func<DirectoryInfo, bool> DirectoryFilter;
    }
}