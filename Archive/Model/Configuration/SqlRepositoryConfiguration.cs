using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model.Configuration
{
    public class SqlRepositoryConfiguration
    {
        public List<string> SchemaList { get; set; }
        public string DefaultSchema { get; set; }
        public List<TableConfiguration> TableList { get; set; }
    }
}
