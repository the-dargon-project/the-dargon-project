using Dargon.Hydar;

namespace Dargon {
   public class PlatformSystemStateImpl : SystemState {
      private readonly ICache<string, string> systemStateCache;

      public PlatformSystemStateImpl(ICache<string, string> systemStateCache) {
         this.systemStateCache = systemStateCache;
      }

      public string Get(string key, string defaultValue) {
         string value;
         if (!systemStateCache.TryGetValue(key, out value)) {
            value = defaultValue;
         }
         return value;
      }

      public void Set(string key, string value) {
         systemStateCache[key] = value;
      }
   }
}
