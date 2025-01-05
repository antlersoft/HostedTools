using System.Collections.Generic;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {
    public interface IFunctionStore {
        void Save(SavedFunction functionDef);
        SavedFunction GetByKey(string key);
        void Delete(string key);
        IEnumerable<string> GetNamespaces();
        IEnumerable<SavedFunction> GetNamespaceFunctions(string space);
    }
}