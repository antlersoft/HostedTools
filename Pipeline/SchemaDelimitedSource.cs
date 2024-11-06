using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
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
    [Export(typeof(IRootNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class SchemaDelimitedSource : AbstractPipelineNode, IHtValueRoot, ISettingDefinitionSource
    {
        static internal ISettingDefinition CommandFile = new PathSettingDefinition("CommandFile", "SchemaDelimited", "File to run data command", false, false, ".exe|*.exe");
        static internal ISettingDefinition CommandArgs = new SimpleSettingDefinition("CommandArgs", "SchemaDelimited", "Command arguments to get delimited data");
        static internal ISettingDefinition SchemaFile = new PathSettingDefinition("SchemaFile", "SchemaDelimited", "Schema file", false, false, ".schema|*.schema");

        public SchemaDelimitedSource()
        : base(new MenuItem("DevTools.Pipeline.Input.SchemaDelimitedSource", "Schema delimited", typeof(SchemaDelimitedSource).FullName, "DevTools.Pipeline.Input"), new[] { CommandFile.FullKey(), CommandArgs.FullKey(), SchemaFile.FullKey() })
        { }

        public IEnumerable<ISettingDefinition> Definitions => new[] { CommandFile, CommandArgs, SchemaFile };

        public override string NodeDescription => $"Schema delimited with schema {SchemaFile.Value<string>(SettingManager)} from {CommandFile.Value<string>(SettingManager)} {CommandArgs.Value<string>(SettingManager)}";

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(
                state.SettingValues[CommandFile.FullKey()],
                state.SettingValues[CommandArgs.FullKey()],
                state.SettingValues[SchemaFile.FullKey()]
            );
        }

        class Source : HostedObjectBase, IHtValueSource {
            private readonly string _commandFile;
            private readonly string _commandArgs;
            private readonly string _schemaFile;

            internal Source(string commandFile, string commandArgs, string schemaFile) {
                _commandFile = commandFile;
                _commandArgs = commandArgs;
                _schemaFile = schemaFile;
            }
            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                string[] fieldNames;
                using (var reader = new StreamReader(_schemaFile))
                {
                    fieldNames = reader.ReadToEnd().Split(',');
                }
                var startInfo = new ProcessStartInfo(_commandFile, _commandArgs);
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
    }

    [Export(typeof(IRootNode))]
    public class CommandLineLineSource : AbstractPipelineNode, IHtValueRoot
    {
        internal const string LINE = "line";
        public CommandLineLineSource()
        : base(new MenuItem("DevTools.Pipeline.Input.CommandLineLineSource", "Command line", typeof(CommandLineLineSource).FullName, "DevTools.Pipeline.Input"), new[] { SchemaDelimitedSource.CommandFile.FullKey(), SchemaDelimitedSource.CommandArgs.FullKey() })
        { }

        public override string NodeDescription => $"Lines from {SchemaDelimitedSource.CommandFile.Value<string>(SettingManager)} {SchemaDelimitedSource.CommandArgs.Value<string>(SettingManager)}";

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(
                state.SettingValues[SchemaDelimitedSource.CommandFile.FullKey()],
                state.SettingValues[SchemaDelimitedSource.CommandArgs.FullKey()]
            );
        }

        class Source : HostedObjectBase, IHtValueSource {
            private readonly string _commandFile;
            private readonly string _commandArgs;

            internal Source(string commandFile, string commandArgs) {
                _commandFile = commandFile;
                _commandArgs = commandArgs;
            }
            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                var startInfo = new ProcessStartInfo(_commandFile, _commandArgs);
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
    }

    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class RegexLineTransform : AbstractPipelineNode, ISettingDefinitionSource, IHtValueStem
    {
        static ISettingDefinition MatchingRegex = new SimpleSettingDefinition("MatchingRegex", "RegexLineTransform", "Matching regex");
        static ISettingDefinition OutputFormat = new SimpleSettingDefinition("OutputFormat", "RegexLineTransform", "Output format", "(blank to return match)");

        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }

        public RegexLineTransform()
        : base(new MenuItem("DevTools.Pipeline.Transform.RegexLineTransform", "Regex Line", typeof(RegexLineTransform).FullName, "DevTools.Pipeline.Transform"), new string[] {MatchingRegex.FullKey(), OutputFormat.FullKey()})
        { 
        }

        public override string NodeDescription => $"s/{MatchingRegex.Value<string>(SettingManager)}/{OutputFormat.Value<string>(SettingManager)}" + (string.IsNullOrWhiteSpace(OutputFormat.Value<string>(SettingManager)) ? string.Empty : "/");

        public IEnumerable<ISettingDefinition> Definitions => new[] { MatchingRegex, OutputFormat };

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(
                state.SettingValues[MatchingRegex.FullKey()],
                state.SettingValues[OutputFormat.FullKey()],
                ConditionBuilder
            );
        }

        class Transform : HostedObjectBase, IHtValueTransform {
            private readonly string _matchingRegex;
            private readonly string _outputFormat;
            private readonly IConditionBuilder _conditionBuilder;

            internal Transform(string matchingRegex, string outputFormat, IConditionBuilder builder) {
                _matchingRegex = matchingRegex;
                _outputFormat = outputFormat;
                _conditionBuilder = builder;
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
                Regex regex = new Regex(_matchingRegex);
                IHtExpression output = null;
                var outputExpr = _outputFormat;
                if (! string.IsNullOrEmpty(outputExpr))
                {
                    output = _conditionBuilder.ParseCondition(outputExpr);
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
    }

    [Export(typeof(IStemNode))]
    public class JsonFromLineTransform : AbstractPipelineNode, IHtValueTransform, IHtValueStem
    { 
        private JsonSerializerSettings _settings;

        public JsonFromLineTransform()
        : base(new MenuItem("DevTools.Pipeline.Input.JsonFromLineTransform", "Strings -> Json", typeof(JsonFromLineTransform).FullName, "DevTools.Pipeline.Transform"), new string[0])
        { }

        public override string NodeDescription => $"String to json";

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return this;
        }

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
