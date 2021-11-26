using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.Archive.Model
{
    [Export(typeof(ISpecialColumnValueGetter))]
    public class CommaDelimitedSpecialValueGetter : HostedObjectBase, ISpecialColumnValueGetter
    {
        public string Name => "CommaDelimited";

        public IEnumerable<Dictionary<string, IHtValue>> GetColumnValueSets(IIndexSpec columns, IHtValue row)
        {
            var result = new List<Dictionary<string, IHtValue>>();
            if (columns.Columns.Count != 1)
            {
                throw new Exception("CommaDelimited Special Value Getter only supports a single column");
            }
            var field = columns.Columns[0];
            var rowValue = row[field.Field.Name];
            if (rowValue != null)
            {
                if (rowValue.IsArray)
                {
                    foreach (var subValue in rowValue.AsArrayElements)
                    {
                        result.Add(new Dictionary<string, IHtValue> { { field.Field.Name, subValue } });
                    }
                }
                else if (rowValue.IsString)
                {
                    foreach (var subValue in rowValue.AsString.Split(','))
                    {
                        if (subValue.Length != 0)
                        {
                            long longVal;
                            double doubleVal;
                            IHtValue parsed;
                            if (long.TryParse(subValue, out longVal))
                            {
                                parsed = new JsonHtValue(longVal);
                            }
                            else if (double.TryParse(subValue, out doubleVal))
                            {
                                parsed = new JsonHtValue(doubleVal);
                            }
                            else
                            {
                                parsed = new JsonHtValue(subValue);
                            }
                            result.Add(new Dictionary<string, IHtValue> { { field.Field.Name, parsed } });
                        }
                    }
                }
            }
            return result;
        }
    }
}
