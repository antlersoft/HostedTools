using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Interface
{
	public interface IHtValue
    {
        bool AsBool { get; set; }
        double AsDouble { get; set; }
		long AsLong { get; set; }
        string AsString { get; set; }
        IEnumerable<IHtValue> AsArrayElements { get; }
        IEnumerable<KeyValuePair<string, IHtValue>> AsDictionaryElements { get; }
        bool IsBool { get; }
        bool IsDouble { get; }
		bool IsLong { get; }
        bool IsString { get; }
        bool IsDictionary { get; }
        bool IsArray { get; }
        bool IsEmpty { get; }
		int Count { get; }
        IHtValue this[string i] { get; set; }
        IHtValue this[int i] { get; set; }
    }
}

