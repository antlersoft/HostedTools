
namespace com.antlersoft.HostedTools.Pipeline
{
    public class TableAlias
    {
        private int index;
        private string prefix;

        public TableAlias(string p=null)
        {
            prefix = p ?? "A";
        }
        public string Current => $"{prefix}{index}";
        public string Next => $"{prefix}{++index}";
    }
}
