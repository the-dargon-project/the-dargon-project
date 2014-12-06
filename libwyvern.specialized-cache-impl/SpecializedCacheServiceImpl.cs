using Dargon.Hydar;
using Dargon.Wyvern.Specialized.Hydar;
using ItzWarty;

namespace Dargon.Wyvern.Specialized {
   public class SpecializedCacheServiceImpl : SpecializedCacheService {
      private readonly Caches caches;

      public SpecializedCacheServiceImpl(Caches caches) {
         this.caches = caches;
      }

      public IDistributedCounter GetCountingCache(string name) {
         return new DistributedCounterImpl(name, caches.CountingCache);
      }
   }

   public class DistributedCounterImpl : IDistributedCounter {
      private readonly string name;
      private readonly ICache<string, long> countCache;

      public DistributedCounterImpl(string name, ICache<string, long> countCache) {
         this.name = name;
         this.countCache = countCache;
      }

      public string Name { get { return name; } }

      public long PeekCurrentValue() {
         return countCache.GetValueOrDefault(name);
      }

      public long TakeNextValue() { return countCache.Invoke(name, new PostIncrementProcessor()); }

      public class PostIncrementProcessor : IEntryProcessor<string, long, long> {
         public long Process(IEntry<string, long> entry) {
            entry.FlagAsDirty();
            if (!entry.IsPresent) {
               return entry.Value = 0;
            } else {
               return entry.Value++;
            }
         }
      }
   }
}
