using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {
    public interface IFunctionStore : IHostedObject {
        void Save(SavedFunction functionDef);
        SavedFunction GetByKey(string key);
        void Delete(string key);
        IEnumerable<string> GetNamespaces();
        IEnumerable<SavedFunction> GetNamespaceFunctions(string space);
    }
}