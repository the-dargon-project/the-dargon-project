using System;

namespace Dargon.Draek.Identities.Hydar {
   public interface IdentityByTokenCache {
      bool Put(string token, Identity value);
      Identity Get(string token);
      bool Remove(string token);

#if DEBUG
      string DumpCacheContents();
#endif
   }
}
