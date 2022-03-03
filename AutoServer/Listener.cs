using Auto;
using Hi;

namespace AutoLauncher.Standard
{
    public class Listener
    {
        private readonly IBus     _bus;
        private readonly HiServer _server;
        private readonly Logger   _log;

        public Listener(Logger logger, IBus bus)
        {
            _bus = bus;
            _log = logger;
            _server = new HiServer
            {
                Log = Log,
                Receive = Receive
            };
            
            _bus.Subscribe<string>(Send);
        }

        private void Send(string msg)
        {
            _server.Send(msg);
        }

        private Msg Receive(Msg msg, Sender sender)
        {
            _bus.Shout(msg.Text);
            return default;
        }

        private void Log(string msg)
        {
            _log.Trace?.Invoke(msg);
        }

        public void Start()
        {
            _server.Open("Auto");
        }

        public void Stop()
        {
            _bus.Unsubscribe<string>(Send);
            _server.Close();
        }
    }
}