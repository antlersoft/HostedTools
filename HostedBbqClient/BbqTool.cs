using System.ComponentModel.Composition;
using System.Diagnostics;
using com.antlersoft.BBQClient;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.HostedBbqClient {
    [Export(typeof(ISettingDefinitionSource))]
    public class BbqTool : GridWorker, ISettingDefinitionSource, IGridItemCommandList {
        [Import]
        IWorkMonitorSource? MonitorSource { get; set; }
        public IEnumerable<ISettingDefinition> Definitions =>  new [] { Query };

        public BbqTool()
        : base(new MenuItem("DevTools.BbqTool", "BBQ Tool", typeof(BbqTool).FullName, "DevTools"), new [] { Query.FullKey() })
        {

        }

        public IEnumerable<IGridItemCommand> GetGridItemCommands(string title)
        {
            return new [] { new SimpleGridItemCommand("Open in VS Code", (row, s) => {
                var args = $"-g {row["file"]}";
                if (row.ContainsKey("index")) {
                    args=$"{args}:{row["index"]}";
                }
                ProcessStartInfo pi = new ProcessStartInfo("/usr/bin/code", args);
                ConsoleCommandHelper.InvokeConsoleCommand(MonitorSource?.GetMonitor(), pi);
            })};        }

        public override void Perform(IWorkMonitor monitor)
        {
            ClearGrid(monitor);
            QueryRequest query = new QueryRequest(Query.Value<string>(SettingManager));
            using (var connect = new BrowseByQueryBySocket()) {
                connect.Connect();
                QueryResponse response = connect.PerformQuery(query);
                var exc = response.RequestException;
                if (exc != null) {
                    monitor.Writer.WriteLine(exc.Message);
                    if (exc.StackTrace != null) {
                        monitor.Writer.WriteLine(exc.StackTrace);
                    }
                }
                if (response.Responses != null ) {
                    for (int i=0; i<response.ResponseCount; i++) {
                        JsonHtValue row = new JsonHtValue();
                        var item=response.Responses[i];
                        row["file"] = new JsonHtValue(item.FileName??string.Empty);
                        row["index"] = new JsonHtValue(item.LineNumber);
                        row["type"]=new JsonHtValue(item.ObjectType??string.Empty);
                        row["description"] = new JsonHtValue(item.Description??string.Empty);
                        WriteRecord(monitor, row);
                    }
                }
            }
        }

        class BasicConfig : IBbqConfig
        {
            public bool UseLegacyService => true;

            public string WebServiceUrl => string.Empty;

            public string UserName => string.Empty;

            public string ApiKey => string.Empty;

            public List<Substitution> Substitutions => new List<Substitution>();
        }

        private IBbqConfig _config = new BasicConfig();

        static ISettingDefinition Query = new MultiLineSettingDefinition("Query","BbqTool", 5, null, "Browse-by-Query Query Expression");
    }
}