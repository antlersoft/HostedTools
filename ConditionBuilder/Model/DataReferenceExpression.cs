
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class DataReferenceExpression : IDataReferenceExpression
    {
        private string _name;

        public DataReferenceExpression(string name)
        {
            _name = name;
        }

        public string ValueName
        {
            get { return _name; }
        }

        public IHtValue Evaluate(IHtValue data)
        {
            return data[_name]??new JsonHtValue();
        }
    }
}
