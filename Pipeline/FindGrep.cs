using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline.Extensions;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline {
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IRootNode))]
    [Export(typeof(IHasSettingChangeActions))]
    public class FindGrep : GridWorker, IGridItemCommandList, ISettingDefinitionSource, IRootNode, IHtValueRoot, IWorkNode, IHasSettingChangeActions
    {
        [Import]
        public IPluginManager PluginManager {get; set;}
        [Import]
        public IWorkMonitorSource MonitorSource { get; set;}

        static readonly string DefaultMatch = "^.*\\.((cxx)|c|(cpp)|h|(sql)|(xsl)|(xml)|(cc)|(hpp)|(rc)|(java)|(cs)|(pl)|(pm)|(py)|(yml)|(conf))$";
        static ISettingDefinition StartFolder = new PathSettingDefinition("StartFolder", "FindGrep", "Start folder", false, true, null, "Folder to start search");
        static ISettingDefinition MatchExpression = new SimpleSettingDefinition("MatchExpresson", "FindGrep", "Match exdpression", "CLR Regular Expression to match against file contents; if blank will match all files");
        static ISettingDefinition CaseInsensitive = new SimpleSettingDefinition("CaseInsensitive", "FindGrep", "Case-insensitive", "Regular expression match is case-insensitive if checked", typeof(bool), "false", false, 0);
        static ISettingDefinition ShowMatching = new SimpleSettingDefinition("ShowMatching", "FindGrep", "Show matching", "Show all lines that match expression", typeof(bool), "false", false, 0);
        static ISettingDefinition FileMatch = new SimpleSettingDefinition("FileMatch", "FindGrep", "File match", "Expression matching files to search", typeof(string), DefaultMatch, false, 20);
        static ISettingDefinition SkipFolder = new SimpleSettingDefinition("SkipFolder", "FindGrep", "Folders to skip", "Regular expression to match folder names to skip");
        static ISettingDefinition ResetFileMatch = new ButtonsDefinition("ResetFileMatch", "FindGrep", new[] { "Reset file match" }, string.Empty, "Sets file match expression back to default");

        private readonly Dictionary<string, Action<IWorkMonitor, ISetting>> _actionsBySettingKey;

        private readonly ISettingDefinition[] _definitions = new [] { StartFolder, MatchExpression, CaseInsensitive, ShowMatching, FileMatch, SkipFolder, ResetFileMatch };

        public IEnumerable<ISettingDefinition> Definitions => _definitions;

        public string NodeDescription => $"FindGrep {MatchExpression.Value<string>(SettingManager)} in {StartFolder.Value<string>(SettingManager)}";

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => _actionsBySettingKey;

        public FindGrep()
        : base(new MenuItem("DevTools.FindGrep", "Findgrep", typeof(FindGrep).FullName, "DevTools"),
            new [] { StartFolder.FullKey(), MatchExpression.FullKey(), CaseInsensitive.FullKey(), ShowMatching.FullKey(), FileMatch.FullKey(), SkipFolder.FullKey(), ResetFileMatch.FullKey()})
        {
            _actionsBySettingKey = new Dictionary<string, Action<IWorkMonitor, ISetting>>{
                { ResetFileMatch.FullKey(), (m,s)=> { SettingManager[FileMatch.FullKey()].SetRaw(DefaultMatch);} }
            };
        }

        public IEnumerable<IGridItemCommand> GetGridItemCommands(string title)
        {
            return new [] { new SimpleGridItemCommand("Open in VS Code", (row, s) => {
                var args = $"-g {row["file"]}";
                if (row.ContainsKey("index")) {
                    args=$"{args}:{row["index"]}";
                }
                ProcessStartInfo pi = new ProcessStartInfo("/usr/bin/code", args);
                ConsoleCommandHelper.InvokeConsoleCommand(MonitorSource.GetMonitor(), pi);
            })};
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new FindGrepSource(state);
        }

        public PluginState GetPluginState(ISet<string> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public void Perform(PluginState state, IWorkMonitor monitor)
        {
            ClearGrid(monitor);
            var source = GetHtValueSource(state);
            foreach (var row in source.GetRows(monitor)) {
                this.WriteRecord(monitor, row);
            }
            if (source.Cast<IDisposable>() is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager, visited);
        }

        public override void Perform(IWorkMonitor monitor)
        {
            var state = GetPluginState();
            CanBackground(monitor, NodeDescription);
            Perform(state, monitor);
        }

        class FindGrepSource : HostedObjectBase, IHtValueSource, IDisposable
        {
            private string _startFolder;
            private Regex _fileMatch;
            private Regex _matcher;
            private bool _showAll;
            private Regex _skipFolders;
            private TextReader _currentFile;
            internal FindGrepSource(PluginState state) {
                _startFolder=state.SettingValues[StartFolder.FullKey()];
                var matchex = state.SettingValues[MatchExpression.FullKey()];
                var matchOptions = (bool)Convert.ChangeType(state.SettingValues[CaseInsensitive.FullKey()], typeof(bool)) ? RegexOptions.IgnoreCase : RegexOptions.None;
                if (! string.IsNullOrWhiteSpace(matchex)) {
                    _matcher = new Regex(matchex, matchOptions);
                }
                var fileEx = state.SettingValues[FileMatch.FullKey()];
                if (! string.IsNullOrWhiteSpace(fileEx)) {
                    _fileMatch = new Regex(fileEx);
                }
                _showAll = (bool)Convert.ChangeType(state.SettingValues[ShowMatching.FullKey()], typeof(bool));
                var skipEx = state.SettingValues[SkipFolder.FullKey()];
                if (! string.IsNullOrWhiteSpace(skipEx)) {
                    _skipFolders = new Regex(skipEx);
                }
            }

            public void Dispose()
            {
                if (_currentFile != null) {
                    _currentFile.Dispose();
                    _currentFile = null;
                }
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                return ProcessFolder(_startFolder, monitor);               
            }

            private IEnumerable<IHtValue> ProcessFolder(string path, IWorkMonitor monitor) {
                var cancelable = monitor.Cast<ICancelableMonitor>();
                foreach (var file in Directory.GetFiles(path)) {
                    if (cancelable?.IsCanceled??false) {
                        break;
                    }
                    if (_fileMatch == null || _fileMatch.IsMatch(Path.GetFileName(file))) {
                        JsonHtValue resultBase = new JsonHtValue();
                        resultBase["file"]=new JsonHtValue(file);
                        if (_matcher == null) {
                            yield return resultBase;
                        } else {
                            if (_currentFile !=null) {
                                _currentFile.Dispose();
                                _currentFile = null;
                            }
                            _currentFile = new StreamReader(file);
                            int index = 0;
                            for (var line = _currentFile.ReadLine(); line!=null; line=_currentFile.ReadLine()) {
                                if (cancelable?.IsCanceled??false) {
                                    break;
                                }
                                index++;
                                if (_matcher.IsMatch(line)) {
                                    if (! _showAll) {
                                        yield return resultBase;
                                        break;
                                    }
                                    var result = new JsonHtValue(resultBase);
                                    result["index"]=new JsonHtValue(index);
                                    result["line"] = new JsonHtValue(line);
                                    yield return result;
                                }
                            }
                        }
                    }
                }
                foreach (var subdir in Directory.GetDirectories(path)) {
                    if (_skipFolders!=null && _skipFolders.IsMatch(subdir)) {
                        monitor.Writer.WriteLine("Skipping folder "+subdir);
                    }
                    foreach (var r in ProcessFolder(subdir, monitor)) {
                        yield return r;
                    }
                }
            }
        }
    }
}