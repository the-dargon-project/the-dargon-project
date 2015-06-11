using Dargon.PortableObjects;

namespace Dargon.Trinkets {
   public class TrinketsApiPofContext : PofContext {
      private const int kBasePofId = 11000;

      public TrinketsApiPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(TrinketStartupConfiguration));
         RegisterPortableObjectType(kBasePofId + 1, typeof(TrinketStartupConfigurationImpl));
      }
   }
}
