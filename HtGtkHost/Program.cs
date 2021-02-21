using System;
using com.antlersoft.HostedTools.GtkHostLib;
using Gtk;

namespace HtGtkHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Application.Init();

            //Create the Window
            Window myWin = new MainWindow();
            myWin.ShowAll();

            Application.Run();
        }
    }
}
