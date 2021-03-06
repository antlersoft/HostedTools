﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.CommandHost
{
    public class CommandRunner
    {
        [Import]
        public IPluginManager PluginManager { get; set; }
        [Import]
        public ISettingManager SettingManager { get; set; }

      [ImportMany]
      public IEnumerable<IAfterComposition> NeedAfterComposition { get; set; }

 
        private CompositionContainer _container;

        public CompositionContainer Container { get { return _container; } }

      public CommandRunner()
      {
//An aggregate catalog that combines multiple catalogs
         var catalog = new AggregateCatalog();

//Adds all the parts found in the same assembly as the Program class
        catalog.Catalogs.Add(new ApplicationCatalog());

//Create the CompositionContainer with the parts in the catalog
        _container = new CompositionContainer(catalog);

        Container.ComposeParts(this);
        foreach (IAfterComposition ac in NeedAfterComposition)
        {
          ac.AfterComposition();
        }
      } 

        public void Run(string[] args)
        {
            IWorkMonitor monitor = new ConsoleMonitor();

            for (int i = 0; i < args.Length;)
            {
                i = RunArg(monitor, args, i);
            }
        }

        private IPlugin FindPluginForName(string name)
        {
            IPlugin result = PluginManager[name];
            return result;
        }

        private void PrintHelpForPlugin(IPlugin plugin, IWorkMonitor monitor)
        {
            monitor.Writer.Write(plugin.Name);
            var worker = plugin.Cast<IWork>();
            if (worker != null)
            {
                var expl = worker.Cast<IExplanation>();
                if (expl != null)
                {
                    monitor.Writer.WriteLine(": " + expl.Explanation);
                }
                else
                {
                    monitor.Writer.WriteLine(":");
                }
                var ss = worker.Cast<ISettingEditList>();
                if (ss != null)
                {
                    foreach (var sd in ss.KeysToEdit.Select(k => SettingManager[k].Definition))
                    {
                        monitor.Writer.WriteLine("--{0} {1}", sd.FullKey(), sd.Description);
                    }
                }
            }
            else
            {
                monitor.Writer.WriteLine(" Not a worker");
            }
        }

        private int PrintHelp(IWorkMonitor monitor, string[] args, int i)
        {
            if (i + 1 < args.Length)
            {
                var command = args[i + 1];
                var plugin = FindPluginForName(command);
                if (plugin != null)
                {
                    PrintHelpForPlugin(plugin, monitor);
                    return i + 2;
                }
            }
            monitor.Writer.WriteLine("Available plugins:");
            foreach (var work in PluginManager.Plugins.Select(p => p.Cast<IWork>()).Where(w => w != null))
            {
                monitor.Writer.Write(work.Cast<IPlugin>().Name);
                var expl = work.Cast<IExplanation>();
                if (expl != null)
                {
                    monitor.Writer.WriteLine(": " + expl.Explanation);
                }
                else
                {
                    monitor.Writer.WriteLine();
                }
            }
            return i + 1;
        }

        private int RunArg(IWorkMonitor monitor, string[] args, int i)
        {
            string command = args[i];
            if (command == "help")
            {
                return PrintHelp(monitor, args, i);
            }
            if (command == "save")
            {
                SettingManager.Save();
                return i + 1;
            }
            if (command.StartsWith("--"))
            {
                string settingKey = command.Substring(2);
                try
                {
                    var setting = SettingManager[settingKey];
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("No value for setting");
                        return i + 1;
                    }
                    setting.SetRaw(args[i+1]);
                }
                catch (Exception)
                {
                    monitor.Writer.WriteLine("Setting "+settingKey+" not found");
                }
                return i + 2;
            }
            var plugin = FindPluginForName(command);

            if (plugin == null)
            {
                monitor.Writer.WriteLine("No plugin "+command+" found");
            }
            else
            {
                var worker = plugin.Cast<IWork>();
                if (worker == null)
                {
                    monitor.Writer.WriteLine(command + " does not represent a Work command");
                }
                else
                {
                    try
                    {
                        worker.Perform(monitor);
                    }
                    catch (Exception ex)
                    {
                        monitor.Writer.WriteLine(ex.ToString());
                    }
                }
            }
            return i + 1;
        }
    }
}
