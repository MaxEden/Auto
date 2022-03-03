using System.Collections.Generic;

namespace Auto
{
    public class Args
    {
        private readonly List<string> _args = new List<string>();

        public Args(params string[] args)
        {
            _args.AddRange(args);
        }

        public void Add(params string[] args)
        {
            _args.AddRange(args);
        }

        public static implicit operator string(Args args)
        {
            return CLI.GetArgsString(args._args.ToArray());
        }
        
        public static implicit operator string[](Args args)
        {
            return args._args.ToArray();
        }
    }
}