using System;
using Newtonsoft.Json;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationCategoryConverter : JsonConverter
   {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { writer.WriteValue(((LeagueModificationCategory)value).Name);}
      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { return LeagueModificationCategory.FromString(reader.Value.ToString()); }
      public override bool CanConvert(Type objectType) { return objectType == typeof(LeagueModificationCategory); }
   }
}
