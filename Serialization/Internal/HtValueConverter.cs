using System;

using Newtonsoft.Json;

using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Serialization.Internal
{
    internal class HtValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IHtValue).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JsonHtValue toRead;
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            if (existingValue != null)
            {
                toRead = (JsonHtValue) existingValue;
            }
            else
            {
                toRead = new JsonHtValue();
            }
            ReadValueStartingWith(reader, toRead);
            return toRead;
        }

        private void ReadValueStartingWith(JsonReader reader, JsonHtValue toRead)
        {
            JsonToken tokenType = reader.TokenType;
            switch (tokenType)
            {
                case JsonToken.Boolean:
                    toRead.AsBool = (bool)reader.Value;
                    break;
                case JsonToken.Date:
                    toRead.AsString = ((DateTime) reader.Value).ToString("o");
                    break;
                case JsonToken.Float:
                    toRead.AsDouble = (double)reader.Value;
                    break;
                case JsonToken.String:
                    toRead.AsString = (string)reader.Value;
                    break;
                case JsonToken.Integer:
                    toRead.AsLong = (long)reader.Value;
                    break;
                case JsonToken.Null:
                    break;
                case JsonToken.StartObject:
                    toRead.MakeDictionary();
                    ReadDictionaryStartingWith(reader, toRead);
                    break;
                case JsonToken.StartArray:
                    toRead.MakeArray();
                    ReadArrayStartingWith(reader, toRead);
                    break;
                case JsonToken.None:
                    break;
            }
        }

        private void ReadDictionaryStartingWith(JsonReader reader, JsonHtValue toRead)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.EndObject:
                        return;
                    case JsonToken.PropertyName:
                        string name = (string)reader.Value;
                        if (! reader.Read())
                        {
                            throw new JsonException("No more tokens when expecting serialized property value");
                        }
                        JsonHtValue propertyValue = new JsonHtValue();
                        ReadValueStartingWith(reader, propertyValue);
                        toRead[name] = propertyValue;
                        break;
                    default:
                        throw new JsonException("Unexpected token type read "+reader.TokenType.ToString()+": "+reader.Value.ToString());
                }
            }
            throw new JsonException("No more tokens when expecting serialized object");
        }

        private void ReadArrayStartingWith(JsonReader reader, JsonHtValue toRead)
        {
            int i = 0;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.EndArray:
                        return;
                    default:
                        JsonHtValue arrayElement = new JsonHtValue();
                        ReadValueStartingWith(reader, arrayElement);
                        toRead[i++] = arrayElement;
                        break;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IHtValue toWrite = value as IHtValue;
            if (toWrite.IsArray)
            {
                writer.WriteStartArray();
                foreach (var v in toWrite.AsArrayElements)
                {
                    WriteJson(writer,v,serializer);
                }
                writer.WriteEndArray();
            }
            else if (toWrite.IsBool)
            {
                writer.WriteValue(toWrite.AsBool);
            }
            else if (toWrite.IsDictionary)
            {
                writer.WriteStartObject();
                foreach (var kvp in toWrite.AsDictionaryElements)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteJson(writer,kvp.Value,serializer);
                }
                writer.WriteEndObject();
            }
            else if (toWrite.IsDouble)
            {
                writer.WriteValue(toWrite.AsDouble);
            }
            else if (toWrite.IsLong)
            {
                writer.WriteValue(toWrite.AsLong);
            }
            else if (toWrite.IsEmpty)
            {
                writer.WriteNull();
            }
            else if (toWrite.IsString)
            {
                writer.WriteValue(toWrite.AsString);
            }
        }
    }
}

