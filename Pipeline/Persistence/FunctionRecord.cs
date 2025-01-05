using SQLite;

namespace com.antlersoft.HostedTools.Pipeline.Persistence {
    [Table("SavedFunction")]
    public class FunctionRecord {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set;}

        // Human readable but mutable name
        [Column("name")]
        public string Name { get; set;}

        // Enum-ish data for future optimizations
        [Column("namespace")]
        public string Namespace { get; set;}

        [Column("json_definition")]
        public string JsonDefinition {get; set;}
    }
}