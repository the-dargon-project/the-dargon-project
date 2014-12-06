using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Hydar;

namespace Dargon {
   public class CacheFactory : ICacheFactory {
      public ICache<TKey, TValue> CreatePersistentCache<TKey, TValue>(string name, IReadOnlyList<ICacheIndex> indices) {
         return new InMemoryCache<TKey, TValue>(name, indices);
      }

      public ICacheIndex<TKey, TValue, TProjection> CreatePersistentCacheIndex<TKey, TValue, TProjection>(string name, ICacheProjector<TKey, TValue, TProjection> projector) {
         return new InMemoryCache<TKey, TValue>.InMemoryCacheIndex<TKey, TValue, TProjection>(name, projector);
      }
   }
}
