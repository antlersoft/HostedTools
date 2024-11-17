using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;
using SQLite;

namespace com.antlersoft.HostedTools.Pipeline.Persistence {
    
    [Export(typeof(INodeStore))]
    public class NodeStore : INodeStore, IPartImportsSatisfiedNotification
    {
        [Import]
        IPluginManager PluginManager;
        [Import]
        public IJsonFactory JsonFactory { get; set; }
        private string _path;
        private SQLiteConnection GetConnection() {
            var db = new SQLiteConnection(_path);
            db.CreateTable<NodeRecord>();
            return db;
        }

        private T DeserializeString<T>(string s) {
            if (string.IsNullOrWhiteSpace(s)) {
                return default(T);
            }
            using (var sr=new StringReader(s))
            using (var jtr=new JsonTextReader(sr)) {
                return JsonFactory.GetSerializer().Deserialize<T>(jtr);
            }
        }

        private string SerializeObject(object o) {
            string result = null; 
            if (o != null) {
                using (var sr = new StringWriter()) {
                    JsonFactory.GetSerializer().Serialize(sr, o);
                    result = sr.ToString();
                }
            }
            return result;
        }

        private SavedNode RecordToNode(NodeRecord record) {
            SavedNode result = null;
            if (record!=null) {
                result = new SavedNode();
                result.Name = record.Name;
                result.State = DeserializeString<PluginState>(record.JsonState);
                result.State.Key = record.Id;
                result.MetaData = DeserializeString<JsonHtValue>(record.JsonMetadata);
            }
            return result;
        }

        public SavedNode GetByKey(string key)
        {
            using (var db = GetConnection()) {
                var record = db.Find<NodeRecord>(key);
                if (record != null) {
                    return RecordToNode(record);
                }
            }
            return null;
        }

        public IList<SavedNode> GetMatching(Type type, string namePrefix)
        {
            var result = new List<SavedNode>();
            using (var db = GetConnection()) {
                return db.Table<NodeRecord>().AsEnumerable().
                    Where(r => type.IsAssignableFrom(PluginManager[DeserializeString<PluginState>(r.JsonState)?.PluginName]
                        .GetType())).Select(r => RecordToNode(r)).ToList();
            }
        }

        public void OnImportsSatisfied()
        {
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "com.antlersoft.HostedTools.Pipeline.db");
        }

        public void Save(SavedNode node)
        {
            NodeRecord nr = new NodeRecord();
            using (var db = GetConnection()) {

                nr.Name = node.Name;
                nr.JsonState = SerializeObject(node.State);
                nr.JsonMetadata = SerializeObject(node.MetaData);
                if (string.IsNullOrWhiteSpace(node.State.Key)) {
                    node.State.Key = Guid.NewGuid().ToString();
                    nr.Id = node.State.Key;
                    db.Insert(nr);
                } else {
                    nr.Id =node.State.Key;
                    db.Update(nr);
                }
            }            
        }

        public void Delete(string key)
        {
            using (var db = GetConnection()) {
                db.Delete<NodeRecord>(key);
            }
        }
    }
}