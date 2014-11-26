using Newtonsoft.Json;
using System;

namespace Dargon.Game
{
   public class GameTypeConverter : JsonConverter
   {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { writer.WriteValue(((GameType)value).Name);}
      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { return GameType.FromString(reader.Value.ToString()); }
      public override bool CanConvert(Type objectType) { return objectType == typeof(GameType); }
   }
}
