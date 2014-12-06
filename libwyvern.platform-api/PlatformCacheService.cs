using Dargon.Hydar;

namespace Dargon.Wyvern {
   public interface PlatformCacheService {
      ICache<TKey, TValue> GetKeyValueCache<TKey, TValue>(string name);
   }
}
