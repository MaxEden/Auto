using System;

namespace Auto
{
    public class Logger
    {
        public Action<string> Error;
        public Action<string> Warning;
        public Action<string> Info;
        public Action<string> Trace;

        public void SubscribeAll(Level level, Action<string> action)
        {
            if(level <= Level.Error) Error += action;
            if(level <= Level.Warning) Warning += action;
            if(level <= Level.Info) Info += action;
            if(level <= Level.Trace) Trace += action;
        }

        public void UnsubscribeAll(Action<string> action)
        {
            Error -= action;
            Warning -= action;
            Info -= action;
            Trace -= action;
        }

        public enum Level
        {
            Error,
            Warning,
            Info,
            Trace
        }

        public static Logger ToMethod(Action<string> action, Level level = Level.Info)
        {
            var logger = new Logger();
            logger.SubscribeAll(level, action);
            return logger;
        }
        
        public static Logger ToConsole()
        {
            var logger = new Logger()
            {
                Error = Console.Error.WriteLine,
                Warning = Console.WriteLine,
                Info = Console.WriteLine
            };
            return logger;
        }
    }
}