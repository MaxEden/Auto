using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Auto;
using Hi;

namespace AutoLauncher
{
    class AutoProgram
    {
        private readonly Logger               _logSink;
        private readonly IBus                 _bus;
        private readonly Context              _context;
        private          Func<Context, Setup> _setupCtor;
        private          HiServer             _server;
        private          IScheduler           _scheduler;

        public AutoProgram()
        {
            _bus = new InternalBus();
            _logSink = Logger.ToMethod(Log);
            _scheduler = new Scheduler();
            _context = new Context(_logSink, _bus, _scheduler, Assembly.GetExecutingAssembly().Location.AsFileInfo().Directory);
            _server = new HiServer();
        }

        public void Start(Func<Context, Setup> setupCtor, bool listen)
        {
            if(Mutex.TryOpenExisting("Auto", out var mtx))
            {
                Console.WriteLine("Already started...");
                return;
            }
            mtx = new Mutex(true, "Auto");

            Console.WriteLine("██ Hello Auto! █████████████");
            Console.WriteLine("Setup...");

            _setupCtor = setupCtor;
            var setup = _setupCtor(_context);
            Console.WriteLine("Commands set...");
            foreach(var name in _context.Commands.Keys)
            {
                Console.WriteLine(name);
            }

            ExecuteSafe(setup.DefaultCommand);
            Console.WriteLine("Server...");

            _server.ManualMessagePolling = true;
            _server.Log = Log;
            _server.Receive = Receive;
            _server.Open("Auto");

            _bus.Subscribe<string>(Shout);

            void Shout(string msg)
            {
                _server.Send(msg);
            }

            Console.WriteLine("Ready!");

            while(listen)
            {
                Thread.Sleep(10);
                if(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    break;
                }

                ExecuteSafe(() =>
                {
                    _server.PollMessages();
                });

                _scheduler.Update(p => ExecuteSafe(p));
            }

            _bus.Unsubscribe<string>(Shout);
            _server.Close();
            mtx.Dispose();
            Console.WriteLine("Bye!");
        }

        private void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
            }
        }

        private Msg Receive(Msg msg, Sender sender)
        {
            var parts = msg.Text.Split('|');
            var name = parts[0];
            var args = parts;
            if(_context.Commands.TryGetValue(name, out var action))
            {
                Console.WriteLine($"Command {name} executed");

                var stopwatch = Stopwatch.StartNew();
                ExecuteSafe(() => action(args));
                stopwatch.Stop();
                var msg1 = $"{name} done {stopwatch.ElapsedMilliseconds}ms";
                Console.WriteLine(msg1);
                return msg1;
            }
            else
            {
                Console.WriteLine($"Command {name} not found!");
                return default;
            }
        }

        private static void Log(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}