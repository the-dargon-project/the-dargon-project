using Dargon.Hydar;

namespace Dargon.Wyvern.Specialized.Hydar {
   public class Caches {
      public const string kCountingCacheName = "counting-cache";

      private readonly PlatformCacheService platformCacheService;
      private readonly ICache<string, long> countingCache;

      public Caches(PlatformCacheService platformCacheService) {
         this.platformCacheService = platformCacheService;
         this.countingCache = platformCacheService.GetKeyValueCache<string, long>(kCountingCacheName);
      }

      public ICache<string, long> CountingCache { get { return countingCache; } }
   }
}
