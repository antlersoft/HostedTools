using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueSource))]
    [Export(typeof(ISettingDefinitionSource))]
    public class SchemaDelimitedSource : EditOnlyPlugin, IHtValueSource, ISettingDefinitionSource
    {
        static ISettingDefinition CommandFile = new PathSettingDefinition("CommandFile", "SchemaDelimited", "File to run data command", false, false, ".exe|*.exe");
        static ISettingDefinition CommandArgs = new SimpleSettingDefinition("CommandArgs", "SchemaDelimited", "Command arguments to get delimited data");
        static ISettingDefinition SchemaFile = new PathSettingDefinition("SchemaFile", "SchemaDelimited", "Schema file", false, false, ".schema|*.schema");

        public SchemaDelimitedSource()
        : base(new MenuItem("DevTools.Pipeline.Input.SchemaDelimitedSource", "Schema delimited", typeof(SchemaDelimitedSource).FullName, "DevTools.Pipeline.Input"), new [] {CommandFile.FullKey(), CommandArgs.FullKey(), SchemaFile.FullKey()})
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
                for (string line = reader.ReadLine(); line!=null; line=reader.ReadLine())
                {
                    var row = new JsonHtValue();
                    string[] fields = line.Split('\t');
                    var cols = fields.Length > fieldNames.Length ? fieldNames.Length : fields.Length;
                    for (int i=0; i<cols; i++)
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
