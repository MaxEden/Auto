using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Auto;
using AutoLauncher.Standard;
using McMaster.NETCore.Plugins;

namespace AutoServer
{
    public class ProjectsManager
    {
        private List<Project> _projects = new List<Project>();
        private AutoService   _auto;

        public IReadOnlyList<Project> Projects => _projects;

        public ProjectsManager(AutoService autoService)
        {
            _auto = autoService;
        }

        public void Step()
        {
            foreach(var project in _projects)
            {
                if(project.NeedsRecompilation && DateTime.Now - project.CheckTime > TimeSpan.FromSeconds(1))
                {
                    Build(project);
                }

                if(project.State == Project.ProjState.Waiting)
                {
                    project.State = Project.ProjState.Valid;
                    OnReload(project);
                }
            }
        }

        public Project Create(string path)
        {
            var proj = new Project();

            if(path.EndsWith(".exe"))
            {
                var dllPath = path.Replace(".exe", ".dll");

                if(File.Exists(dllPath)) path = dllPath;

                proj.DllPath     = path;
                proj.Name        = Path.GetFileNameWithoutExtension(path);
                proj.ProjectFile = Folders.SearchUp(path, "*.csproj", "obj", "bin");
            }
            else if(path.EndsWith(".dll"))
            {
                proj.DllPath     = path;
                proj.Name        = Path.GetFileNameWithoutExtension(path);
                proj.ProjectFile = Folders.SearchUp(path, "*.csproj", "obj", "bin");
            }
            else if(path.EndsWith(".csproj"))
            {
                proj.ProjectFile = path;
                proj.Name        = Path.GetFileNameWithoutExtension(path);
                proj.DllPath     = Folders.SearchDown(path, "*.dll", "obj");
            }
            else if(Directory.Exists(path))
            {
                proj.ProjectFile = Folders.SearchDown(path, "*.csproj");
                proj.Name        = Path.GetFileNameWithoutExtension(proj.ProjectFile);
                proj.DllPath     = Folders.SearchDown(path, "*.dll", "obj");
            }

            if(proj.ProjectFile != null)
            {
                //_auto.ExecuteSafely(() => { Build(proj); });

                if(proj.DllPath == null) proj.DllPath = Folders.SearchDown(proj.ProjectFile, "*.dll", "obj");
                proj.ProjectFolder = Path.GetDirectoryName(proj.ProjectFile);
                proj.Scripts       = Folders.SearchAllDown(proj.ProjectFolder, "*.cs", "obj", "bin");

                if(proj.Scripts != null && proj.Scripts.Length > 0)
                {
                    var watcher = new FileSystemWatcher(proj.ProjectFolder);
                    foreach(var script in proj.Scripts)
                    {
                        watcher.Filters.Add("*" + Path.GetFileName(script));
                    }

                    watcher.Changed += proj.ScriptChanged;
                    watcher.Renamed += proj.ScriptChanged;
                    watcher.Deleted += proj.ScriptChanged;
                    watcher.Created += proj.ScriptChanged;

                    proj.Watcher                     = watcher;
                    proj.Watcher.EnableRaisingEvents = true;

                    Console.WriteLine(
                        "Watching for:" + string.Join(", ", proj.Scripts.Select(p => Path.GetFileName(p))));
                }
            }

            proj.Loader = PluginLoader.CreateFromAssemblyFile(
                proj.DllPath,
                new[] {typeof(Context)},
                config =>
                {
                    config.ReloadDelay     = TimeSpan.FromSeconds(0.5);
                    config.IsUnloadable    = true;
                    config.EnableHotReload = true;
                });

            proj.Loader.Reloaded += proj.ReloadedHook;
            return proj;
        }

        private void Build(Project proj)
        {
            proj.NeedsRecompilation = false;
            var start   = new ProcessStartInfo("dotnet", $@"build ""{proj.ProjectFile}""");
            var process = Process.Start(start);
            process?.WaitForExit(30000);
        }

        private void OnReload(Project proj)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Loading: {proj.DllPath}");
            _auto.ExecuteSafely(() =>
            {
                var assembly = proj.Loader.LoadDefaultAssembly();
                var type     = assembly.GetTypes().FirstOrDefault(p => p.IsSealed && p.Name == "Program");
                var method = type.GetMethod("Launch",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                proj.Context = _auto.GetContext(Path.GetDirectoryName(proj.DllPath));

                Console.WriteLine("Script is loaded. Setup...");
                var setup = (Setup)method.Invoke(null, new object[] {proj.Context});

                Console.WriteLine("Commands set...");
                foreach(var name in proj.Context.Commands.Keys)
                {
                    _auto.AddCommand(name);
                    Console.WriteLine(name);
                }

                Console.WriteLine("Default command...");
                setup.DefaultCommand();
            });
            Console.WriteLine("Project is loaded");
            Console.ResetColor();
        }

        public void Add(AutoService.Std.Add add)
        {
            if(_projects.Any(p => p.DllPath == add.Path)) return;

            var proj = Create(add.Path);
            if(proj == null) return;
            proj.OnReload = OnReload;

            _projects.Add(proj);

            proj.State = Project.ProjState.Waiting;
        }
    }
}