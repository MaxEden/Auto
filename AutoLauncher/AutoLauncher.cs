using System;
using Auto;

namespace AutoLauncher
{
    public static class AutoLauncher
    {
        private static readonly AutoProgram Program = new AutoProgram();

        public static void Start(Func<Context, Setup> setupCtor, bool listen = true)
        {
            Program.Start(setupCtor, listen);
        }
    }
}