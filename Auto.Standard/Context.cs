using System;
using System.Collections.Generic;
using System.IO;

namespace Auto
{
    public class Context
    {
        public readonly Dictionary<string, Action<string[]>> Commands = new Dictionary<string, Action<string[]>>();
        public readonly Logger                               Logger;
        public readonly IBus                                 Bus;
        public readonly IScheduler                           Scheduler;
        public readonly LocalFolders                         LocalFolders;

        public Context(Logger logger, IBus bus, IScheduler scheduler, DirectoryInfo scriptDirectory)
        {
            Bus = bus;
            Scheduler = scheduler;
            Logger = logger;
            LocalFolders = new LocalFolders(logger, scriptDirectory);
        }
    }
}