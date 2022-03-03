using System;
using System.Diagnostics;
using Auto;
using Mini;

namespace AutoServer
{
    public class AutoService : ServiceBase
    {
        private Logger          _logger;
        private IBus            _bus;
        private IScheduler      _scheduler;
        private ProjectsManager _projectsManager;

        protected override string ServiceName => "Auto";

        protected override void PrintHello()
        {
            Console.WriteLine("██ Hello Auto! █████████████");
        }

        protected override void OnStart()
        {
            _bus = new InternalBus();
            _logger = Logger.ToConsole();
            _scheduler = new Scheduler();
            _projectsManager = new ProjectsManager(this);
            _bus.Subscribe<string>(Send);

            //InputCommand(@"add ""H:\GITS\Mandarin\Templates\SidePocket\_Auto\bin\Debug\net5.0\_Auto.dll""");
        }

        protected override void AddCommands(Commands commands)
        {
            commands.AddType<Std.Add>();
        }

        protected override void OnKeyAvailable(ConsoleKeyInfo key) {}

        protected override void FullStep()
        {
            _scheduler.Update(p => ExecuteSafely(p));
            _projectsManager.Step();
        }

        protected override void ExecuteCommand(object command)
        {
            if(command is Std.Add add)
            {
                _projectsManager.Add(add);
            }
        }

        protected override void ExecuteCommandText(string name, string[] args)
        {
            foreach(var project in _projectsManager.Projects)
            {
                if(project.Context.Commands.TryGetValue(name, out var action))
                {
                    Console.WriteLine($"Command {name} executing...");
                    var stopwatch = Stopwatch.StartNew();
                    ExecuteSafely(() => action(args));
                    stopwatch.Stop();
                    Console.WriteLine($"{name} done {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    Console.WriteLine($"Command {name} not found!");
                }
            }
        }

        public class Std
        {
            public class Add
            {
                public string Path { get; set; }
            }
        }

        public Context GetContext(string directory)
        {
            return new Context(_logger, _bus, _scheduler, directory.AsDirInfo());
        }

        public void AddCommand(string name)
        {
            Commands.Add(name);
        }
    }
}