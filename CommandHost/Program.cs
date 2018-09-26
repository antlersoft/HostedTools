using System;

namespace com.antlersoft.HostedTools.CommandHost
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new CommandRunner().Run(args);
        }
    }
}
