using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Hydar;
using Dargon.PortableObjects;
using Dargon.Wyvern;
using Dargon.Wyvern.Specialized.Hydar;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon {
   public class PlatformCacheServiceImpl : PlatformCacheService {
      private readonly ICollectionFactory collectionFactory;
      private readonly ICacheFactory cacheFactory;
      private readonly IDictionary<string, ICache> cachesByName;

      public PlatformCacheServiceImpl(ICollectionFactory collectionFactory, ICacheFactory cacheFactory) {
         this.collectionFactory = collectionFactory;
         this.cacheFactory = cacheFactory;
         this.cachesByName = collectionFactory.CreateDictionary<string, ICache>();
      }

      public void Initialize() {
         InitializeSpecializedCaches();
         InitializeAccountCaches();
      }

      private void InitializeSpecializedCaches() {
         RegisterPersistentNamedCache<string,  long>(Caches.kCountingCacheName);
      }

      private void InitializeAccountCaches() {
         // todo: this or DI?
      }

      
      private void RegisterPersistentNamedCache<TKey, TValue>(string name, IReadOnlyList<ICacheIndex> indices = null) {
         Console.WriteLine("Register Persistent Cache " + name);
         cachesByName.Add(name, cacheFactory.CreatePersistentCache<TKey, TValue>(name, indices));
      }

      private ICacheIndex<TKey, TValue, TProjection> ConstructPersistentNamedCacheIndex<TKey, TValue, TProjection>(string name, ICacheProjector<TKey, TValue, TProjection> projector) {
         return cacheFactory.CreatePersistentCacheIndex(name, projector);
      }

      public ICache<TKey, TValue> GetKeyValueCache<TKey, TValue>(string name) { return (ICache<TKey, TValue>)cachesByName.GetValueOrDefault(name); }
   }
}
