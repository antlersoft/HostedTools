using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Pipeline {
    public interface INodeStore {
        SavedNode GetByKey(string key);
        void Save(SavedNode node);
        void Delete(string key);
        IList<SavedNode> GetMatching(Type type, string namePrefix);
    }
}