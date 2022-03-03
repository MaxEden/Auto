using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auto
{
    public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var str = reader.GetString();
            if(str == null) return null;
            return new DirectoryInfo(str);
        }

        public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
        {
            if(value == null) writer.WriteNullValue();
            else writer.WriteStringValue(value.ToString());
        }
    }
    
    public class FileInfoConverter : JsonConverter<FileInfo>
    {
        public override FileInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var str = reader.GetString();
            if(str == null) return null;
            return new FileInfo(str);
        }

        public override void Write(Utf8JsonWriter writer, FileInfo value, JsonSerializerOptions options)
        {
            if(value == null) writer.WriteNullValue();
            else writer.WriteStringValue(value.ToString());
        }
    }
    
    public class UnknownPathConverter : JsonConverter<UnknownPath>
    {
        public override UnknownPath? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            var str = reader.GetString();
            if(str == null) return null;
            return new UnknownPath(str);
        }

        public override void Write(Utf8JsonWriter writer, UnknownPath value, JsonSerializerOptions options)
        {
            if(value == null) writer.WriteNullValue();
            else writer.WriteStringValue(value.ToString());
        }
    }
}