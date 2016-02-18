using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Serialization
{
    public class JsonHtValue : IHtValue
    {
        private readonly static JsonFactory Factory = new JsonFactory();
        private bool? _asBool;
        private double? _asDouble;
		private long? _asLong;
        private string _asString;
        private List<IHtValue> _asArray;
        private Dictionary<string, IHtValue> _asDictionary;

        public JsonHtValue()
        {
            
        }

        public JsonHtValue(double v)
        {
            _asDouble = v;
        }

        public JsonHtValue(bool v)
        {
            _asBool = v;
        }

        public JsonHtValue(string s)
        {
            _asString = s;
        }
		
		public JsonHtValue(long l)
		{
			_asLong = l;
		}

        public JsonHtValue(IHtValue source)
        {
            if (source != null && !source.IsEmpty)
            {
                if (source.IsBool)
                {
                    _asBool = source.AsBool;
                }

                else if (source.IsDouble)
                {
                    _asDouble = source.AsDouble;
                }

                else if (source.IsString)
                {
                    _asString = source.AsString;
                }
				else if (source.IsLong)
				{
					_asLong = source.AsLong;
				}
                else if (source.IsArray)
                {
                    _asArray = new List<IHtValue>();
                    foreach (IHtValue value in source.AsArrayElements)
                    {
                        _asArray.Add(new JsonHtValue(value));
                    }
                }

                else if (source.IsDictionary)
                {
                    _asDictionary = new Dictionary<string, IHtValue>();
                    foreach (KeyValuePair<string, IHtValue> kvp in source.AsDictionaryElements)
                    {
                        _asDictionary.Add(kvp.Key, new JsonHtValue(kvp.Value));
                    }
                }
            }
        }

        public static IHtValue GetValue(object o, JsonSerializer serializer=null)
        {
            if (o == null)
            {
                return new JsonHtValue();
            }

            var value = o as IHtValue;
            if (value != null)
            {
                return value;
            }

            var asString = o as string;
            if (asString != null)
            {
                return new JsonHtValue(asString);
            }

            if (o is Boolean)
            {
                return new JsonHtValue((bool)o);
            }

            if (o is Double)
            {
                return new JsonHtValue((double)o);
            }

            if (o is Int32)
            {
                return new JsonHtValue((int)o);
            }

            if (o is Int64)
            {
                return new JsonHtValue((long)o);
            }

            if (o is Decimal)
            {
                return new JsonHtValue((double)(decimal)o);
            }

            if (o is float)
            {
                return new JsonHtValue((float)o);
            }

            if (o is char)
            {
                return new JsonHtValue((char)o);
            }

            if (o is byte)
            {
                return new JsonHtValue((byte)o);
            }
            if (o is Int16)
            {
                return new JsonHtValue((short)o);
            }

            if (o is UInt16)
            {
                return new JsonHtValue((ushort)o);
            }
            if (o is UInt32)
            {
                return new JsonHtValue((uint)o);
            }
            if (o is UInt64)
            {
                return new JsonHtValue((ulong)o);
            }
            serializer = serializer ?? new JsonFactory().GetSerializer();
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, o);
                using (StringReader sr = new StringReader(sw.ToString()))
                {
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        return serializer.Deserialize<IHtValue>(jr);
                    }
                }
            }
        }

        public bool AsBool {
			get {
				if (_asBool.HasValue) {
					return _asBool.Value;
				}
				if (IsEmpty) {
					return false;
				}
				if (IsString) {
					return _asString.Length > 0 && _asString.ToLowerInvariant () != "false";
				}
				if (IsDouble) {
					return (_asDouble ?? 0.0) != 0.0;
				}
				return true;
			}
			set {
				_asBool = value;
				_asDouble = null;
				_asLong = null;
                _asString = null;
                _asArray = null;
                _asDictionary = null;
            }
        }

        public double AsDouble {
			get {
				if (_asDouble.HasValue) {
					return _asDouble.Value;
				}
				if (_asLong.HasValue) {
					return _asLong.Value;
				}
				var stringValue = AsString;
				return stringValue == null ? 0.0 : Double.Parse (stringValue);
			}
			set {
				_asDouble = value;
				_asBool = null;
				_asLong = null;
                _asString = null;
                _asArray = null;
                _asDictionary = null;
            }
        }

        public long AsLong {
			get {
				if (_asLong.HasValue) {
					return _asLong.Value;
				}
				if (_asDouble.HasValue) {
					return (long)_asDouble.Value;
				}
				var stringValue = AsString;
				return stringValue == null ? 0L: Int64.Parse (stringValue);
			}
			set {
				_asLong = value;
				_asDouble = null;
				_asBool = null;
				_asLong = null;
                _asString = null;
                _asArray = null;
                _asDictionary = null;
            }
        }

        public string AsString {
			get {
				if (_asString != null || IsEmpty) {
					return _asString;
				}
				if (IsDouble) {
					return _asDouble.GetValueOrDefault ().ToString (CultureInfo.InvariantCulture);
				}
				if (IsLong) {
					return _asLong.GetValueOrDefault ().ToString (CultureInfo.InvariantCulture);
				}
				if (IsBool) {
					return _asBool ?? false ? "true" : "false";
				}
				StringBuilder sb = new StringBuilder ();
				using (var sw = new StringWriter(sb)) {
					Factory.GetSerializer ().Serialize (sw, this);
				}
				return sb.ToString ();
			}
			set {
				_asString = value;
				_asBool = null;
				_asDouble = null;
				_asLong = null;
                _asArray = null;
                _asDictionary = null;
            }
        }

        public IEnumerable<IHtValue> AsArrayElements
        {
            get { return _asArray; }
        }

        public IEnumerable<KeyValuePair<string, IHtValue>> AsDictionaryElements
        {
            get { return _asDictionary; }
        }

        public bool IsBool
        {
            get { return _asBool.HasValue;}
        }

        public bool IsDouble
        {
            get { return _asDouble.HasValue; }
        }
		
		public bool IsLong
		{
			get {return _asLong.HasValue; }
		}

        public bool IsString
        {
            get { return _asString != null; }
        }

        public bool IsDictionary
        {
            get { return _asDictionary != null; }
        }
		
		public int Count
		{
			get {
				if (IsArray)
				{
					return _asArray.Count;
				}
				if (IsDictionary)
				{
					return _asDictionary.Count;
				}
				return 0;
			}
		}

        public bool IsArray
        {
            get { return _asArray != null; }
        }

        public bool IsEmpty
        {
            get { return ! IsBool && ! IsDouble && ! IsString && ! IsArray && ! IsDictionary; }
        }

        public IHtValue this[string i]
        {
            get
            {
                if (! IsDictionary)
                {
                    this[i] = new JsonHtValue();
                }
                IHtValue result;
                // For consistent semantics, the object returned must be a member of the
                // dictionary -- but what if it did not previously exist?
                // We don't want to error on this occasion, so we add it to the dictionary before
                // we return it.  However, this can cause unexpected concurrency problems, since
                // we expect this get operation to be read-only; so we don't update the dictionary,
                // we replace it using interlocked exchange
                // here to do a non-locking version of the dictionary update that is thread safe
                // Note that this does not fix the case where the object was not a dictionary to begin
                // with (partial fix with interlocked.compareexchange ugh); also does not fix
                // array processing
                if (!_asDictionary.TryGetValue(i, out result))
                {
                    Dictionary<string, IHtValue> originalDictionary, newDictionary;
                    do
                    {
                        originalDictionary = _asDictionary;
                        if (originalDictionary.TryGetValue(i, out result))
                        {
                            break;
                        }
                        newDictionary = new Dictionary<string, IHtValue>(originalDictionary);
                        result = new JsonHtValue();
                        newDictionary.Add(i, result);
                    } while (Interlocked.CompareExchange(ref _asDictionary,  newDictionary, originalDictionary)!=originalDictionary);
                }
                return result;
            }
            set
            {
                MakeDictionary();
                _asDictionary[i] = value;
            }
        }

        internal void MakeDictionary ()
		{
			if (null == Interlocked.CompareExchange (ref _asDictionary, new Dictionary<string, IHtValue> (), null)) {
				_asBool = null;
				_asArray = null;
				_asDouble = null;
				_asLong = null;
                _asString = null;
            }
        }

        internal void MakeArray()
        {
            if (! IsArray)
            {
                _asArray = new List<IHtValue>();
            }
        }

        public IHtValue this[int i]
        {
            get
            {
                if (i < 0)
                {
                    throw new IndexOutOfRangeException("IHtValue array index must be >= 0");
                }
                if (! IsArray || i >= _asArray.Count)
                {
                    this[i] = new JsonHtValue();
                }
                IHtValue result = _asArray[i];
                if (result == null)
                {
                    result = new JsonHtValue();
                    _asArray[i] = result;
                }
                return result;
            }
            set
            {
                if (! IsArray)
                {
                    _asArray = new List<IHtValue>(i+1);
                }
                if (i >= _asArray.Count)
                {
                    int toAdd = i - _asArray.Count;
                    for (int j = 0; j < toAdd; j++)
                    {
                        _asArray.Add(new JsonHtValue());
                    }
                    _asArray.Add(value);
                }
                else
                {
                    _asArray[i] = value;
                }
            }
        }
    }
}

