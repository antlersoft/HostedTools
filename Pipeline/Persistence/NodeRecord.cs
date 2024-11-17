using SQLite;

namespace com.antlersoft.HostedTools.Pipeline.Persistence {
    [Table("SavedNode")]
    public class NodeRecord {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set;}

        // Human readable but mutable name
        [Column("name")]
        public string Name { get; set;}

        // Enum-ish data for future optimizations
        [Column("type_id")]
        public int TypeId { get; set;}

        [Column("json_state")]
        public string JsonState {get; set;}

        [Column("json_metadata")]
        public string JsonMetadata { get; set; }
    }
}