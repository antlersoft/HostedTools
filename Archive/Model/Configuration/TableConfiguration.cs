using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model.Configuration
{
    public class TableConfiguration
    {
        public TableReference Table { get; set; }
        public List<ConstraintConfiguration> AddedConstraints { get; set; }
        public List<ConstraintConfiguration> RemovedConstraints { get; set; }
        public List<string> ForceNullOnInsert { get; set; }
    }
}
