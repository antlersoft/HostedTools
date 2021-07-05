using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    class FolderTableArchive
    {
        internal ITable Table;
        internal string Path;

        internal IEnumerable<IHtValue> ReadSerializedRows(JsonSerializer serializer)
        {
            using (var sr = new StreamReader(Path))
            using (var jr = new JsonTextReader(sr))
            {
                bool result = jr.Read(); // Read array start token
                result = jr.Read(); // Read object start token
                while (result)
                {
                    IHtValue row = serializer.Deserialize<IHtValue>(jr);
                    if (row != null)
                    {
                        yield return row;
                    }
                    else
                    {
                        break;
                    }
                    while (jr.TokenType != JsonToken.StartObject && (result=jr.Read()));
                }
            }
        }
    }
}
