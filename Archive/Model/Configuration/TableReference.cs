using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model.Configuration
{
    public class TableReference
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public override int GetHashCode()
        {
            return (Schema ?? string.Empty).ToLowerInvariant().GetHashCode() ^ Name.ToLowerInvariant().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return ToString().ToLowerInvariant() == obj.ToString().ToLowerInvariant();
        }

        public override string ToString()
        {
            return $"{Schema ?? string.Empty}{(Schema != null ? "." : string.Empty)}{Name}";
        }
    }
}
