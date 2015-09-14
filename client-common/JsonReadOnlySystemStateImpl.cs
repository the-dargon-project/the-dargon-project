using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Dargon {
   public class JsonReadOnlySystemStateImpl : SystemStateImplBase {
      private readonly JObject jsonRoot;

      private JsonReadOnlySystemStateImpl(JObject jsonRoot) {
         this.jsonRoot = jsonRoot;
      }

      public override bool TryGet(string key, out string value) {
         if (string.IsNullOrWhiteSpace(key)) {
            value = null;
            return false;
         }

         string[] breadcrumbs = key.Split('.');
         var current = jsonRoot;
         for (var i = 0; i < breadcrumbs.Length; i++) {
            var breadcrumb = breadcrumbs[i];
            JToken next;
            if (!current.TryGetValue(breadcrumb, StringComparison.OrdinalIgnoreCase, out next)) {
               value = null;
               return false;
            } else if (next.Type == JTokenType.Object) {
               current = (JObject)next;
            } else if (i == breadcrumbs.Length - 1) {
               value = next.ToString();
               return true;
            } else {
               value = null;
               return false;
            }
         }

         // We're returning a json object
         value = current.ToString();
         return true;
      }

      public override void Set(string key, string value) {
         throw new NotSupportedException($"Cannot set key {key} of {nameof(JsonReadOnlySystemStateImpl)} to value {value}.");
      }

      public static SystemState FromFile(string jsonPath) {
         var json = File.ReadAllText(jsonPath);
         var jObject = JObject.Parse(json);
         return new JsonReadOnlySystemStateImpl(jObject);
      }
   }
}
