using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedditQuoteBot.Core.Converters
{
    internal class MicrosecondEpochConverter : JsonConverter<DateTime>
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return _epoch.AddMilliseconds((long)reader.GetDouble() * 1000);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(((value - _epoch).TotalMilliseconds / 1000).ToString());
        }
    }
}
