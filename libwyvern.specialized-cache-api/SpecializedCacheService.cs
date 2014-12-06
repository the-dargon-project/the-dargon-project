namespace Dargon.Wyvern.Specialized {
   public interface SpecializedCacheService {
      IDistributedCounter GetCountingCache(string name);
   }
}
