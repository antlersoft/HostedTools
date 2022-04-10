using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueSource))]
    [Export(typeof(ISettingDefinitionSource))]
    public class SchemaDelimitedSource : EditOnlyPlugin, IHtValueSource, ISettingDefinitionSource
    {
        static internal ISettingDefinition CommandFile = new PathSettingDefinition("CommandFile", "SchemaDelimited", "File to run data command", false, false, ".exe|*.exe");
        static internal ISettingDefinition CommandArgs = new SimpleSettingDefinition("CommandArgs", "SchemaDelimited", "Command arguments to get delimited data");
        static internal ISettingDefinition SchemaFile = new PathSettingDefinition("SchemaFile", "SchemaDelimited", "Schema file", false, false, ".schema|*.schema");

        public SchemaDelimitedSource()
        : base(new MenuItem("DevTools.Pipeline.Input.SchemaDelimitedSource", "Schema delimited", typeof(SchemaDelimitedSource).FullName, "DevTools.Pipeline.Input"), new[] { CommandFile.FullKey(), CommandArgs.FullKey(), SchemaFile.FullKey() })
        { }

        public IEnumerable<ISettingDefinition> Definitions => new[] { CommandFile, CommandArgs, SchemaFile };

        public string SourceDescription => $"Schema delimited with schema {SchemaFile.Value<string>(SettingManager)} from {CommandFile.Value<string>(SettingManager)} {CommandArgs.Value<string>(SettingManager)}";

        public IEnumerable<IHtValue> GetRows()
        {
            string[] fieldNames;
            using (var reader = new StreamReader(SchemaFile.Value<string>(SettingManager)))
            {
                fieldNames = reader.ReadToEnd().Split(',');
            }
            var startInfo = new ProcessStartInfo(CommandFile.Value<string>(SettingManager), CommandArgs.Value<string>(SettingManager));
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = Process.Start(startInfo);
            using (var reader = process.StandardOutput)
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var row = new JsonHtValue();
                    string[] fields = line.Split('\t');
                    var cols = fields.Length > fieldNames.Length ? fieldNames.Length : fields.Length;
                    for (int i = 0; i < cols; i++)
                    {
                        string txt = fields[i];
                        string name = fieldNames[i];
                        long lval;
                        double dval;
                        bool bval;
                        if (long.TryParse(txt, out lval))
                        {
                            row[name] = new JsonHtValue(lval);
                        }
                        else if (double.TryParse(txt, out dval))
                        {
                            row[name] = new JsonHtValue(dval);
                        }
                        else if (bool.TryParse(txt, out bval))
                        {
                            row[name] = new JsonHtValue(bval);
                        }
                        else
                        {
                            row[name] = new JsonHtValue(txt);
                        }
                    }
                    yield return row;
                }
            }
        }
    }

    [Export(typeof(IHtValueSource))]
    public class CommandLineLineSource : EditOnlyPlugin, IHtValueSource
    {
        internal const string LINE = "line";
        public CommandLineLineSource()
        : base(new MenuItem("DevTools.Pipeline.Input.CommandLineLineSource", "Command line", typeof(CommandLineLineSource).FullName, "DevTools.Pipeline.Input"), new[] { SchemaDelimitedSource.CommandFile.FullKey(), SchemaDelimitedSource.CommandArgs.FullKey() })
        { }

        public string SourceDescription => $"Lines from {SchemaDelimitedSource.CommandFile.Value<string>(SettingManager)} {SchemaDelimitedSource.CommandArgs.Value<string>(SettingManager)}";

        public IEnumerable<IHtValue> GetRows()
        {
            var startInfo = new ProcessStartInfo(SchemaDelimitedSource.CommandFile.Value<string>(SettingManager), SchemaDelimitedSource.CommandArgs.Value<string>(SettingManager));
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = Process.Start(startInfo);
            using (var reader = process.StandardOutput)
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var row = new JsonHtValue();
                    row[LINE] = new JsonHtValue(line);
                    yield return row;
                }
            }
        }
    }

    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class RegexLineTransform : EditOnlyPlugin, ISettingDefinitionSource, IHtValueTransform
    {
        static ISettingDefinition MatchingRegex = new SimpleSettingDefinition("MatchingRegex", "RegexLineTransform", "Matching regex");
        static ISettingDefinition OutputFormat = new SimpleSettingDefinition("OutputFormat", "RegexLineTransform", "Output format", "(blank to return match)");

        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }

        public RegexLineTransform()
        : base(new MenuItem("DevTools.Pipeline.Transform.RegexLineTransform", "Regex Line", typeof(RegexLineTransform).FullName, "DevTools.Pipeline.Transform"), new string[] {MatchingRegex.FullKey(), OutputFormat.FullKey()})
        { 
        }

        public string TransformDescription => $"s/{MatchingRegex.Value<string>(SettingManager)}/{OutputFormat.Value<string>(SettingManager)}" + (string.IsNullOrWhiteSpace(OutputFormat.Value<string>(SettingManager)) ? string.Empty : "/");

        public IEnumerable<ISettingDefinition> Definitions => new[] { MatchingRegex, OutputFormat };

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
            Regex regex = new Regex(MatchingRegex.Value<string>(SettingManager));
            IHtExpression output = null;
            var outputExpr = OutputFormat.Value<string>(SettingManager);
            if (! string.IsNullOrEmpty(outputExpr))
            {
                output = ConditionBuilder.ParseCondition(outputExpr);
            }

            if (!cancelable?.IsCanceled ?? false)
            {
                foreach (var l in input)
                {
                    var match = regex.Match(l[CommandLineLineSource.LINE].AsString);
                    if (match.Success)
                    {
                        IHtValue row = new JsonHtValue();
                        var groups = new JsonHtValue();
                        for (int i=0; i<match.Groups.Count; i++)
                        {
                            groups[i] = new JsonHtValue(match.Groups[i].Value);
                        }
                        row[CommandLineLineSource.LINE] = l[CommandLineLineSource.LINE];
                        row["groups"] = groups;
                        if (output != null)
                        {
                            row = output.Evaluate(row);
                        }
                        yield return row;
                    }
                    if (cancelable?.IsCanceled ?? false)
                    {
                        break;
                    }
                }
            }
        }
    }

    [Export(typeof(IHtValueTransform))]
    public class JsonFromLineTransform : EditOnlyPlugin, IHtValueTransform
    { 
        private JsonSerializerSettings _settings;

        public JsonFromLineTransform()
        : base(new MenuItem("DevTools.Pipeline.Input.JsonFromLineTransform", "Strings -> Json", typeof(JsonFromLineTransform).FullName, "DevTools.Pipeline.Transform"), new string[0])
        { }

        public string TransformDescription => $"String to json";

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
            var settings = new JsonFactory().GetSettings();

            if (! cancelable?.IsCanceled??false)
            {
                foreach (var l in input)
                {
                    IHtValue row;
                    try
                    {
                        row = JsonConvert.DeserializeObject<IHtValue>(l[CommandLineLineSource.LINE].AsString, settings);
                    }
                    catch (Exception)
                    {
                        row = null;
                    }
                    if (row != null)
                    {
                        yield return row;
                    }
                    if (cancelable?.IsCanceled??false)
                    {
                        break;
                    }
                }
            }
        }
    }
}
