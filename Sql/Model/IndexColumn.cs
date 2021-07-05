using com.antlersoft.HostedTools.Sql.Interface;


namespace com.antlersoft.HostedTools.Sql.Model
{
    public class IndexColumn : IIndexColumn
    {
        public IndexColumn(IField field, bool descending = false)
        {
            Field = field;
            Descending = descending;
        }
        public IField Field { get; private set; }

        public bool Descending { get; private set; }
    }
}
