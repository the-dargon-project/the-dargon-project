using Dargon.PortableObjects;
using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Components;

namespace Dargon.Trinkets {
   public class TrinketsApiPofContext : PofContext {
      private const int kBasePofId = 11000;

      public TrinketsApiPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(TrinketStartupConfiguration));
         RegisterPortableObjectType(kBasePofId + 1, typeof(TrinketStartupConfigurationImpl));
         RegisterPortableObjectType(kBasePofId + 2, typeof(TrinketComponent));
         RegisterPortableObjectType(kBasePofId + 3, typeof(NameComponent));
         RegisterPortableObjectType(kBasePofId + 4, typeof(DebugComponent));
         RegisterPortableObjectType(kBasePofId + 5, typeof(VerboseLoggerComponent));
         RegisterPortableObjectType(kBasePofId + 6, typeof(FilesystemComponent));
         RegisterPortableObjectType(kBasePofId + 7, typeof(ProcessSuspensionComponent));
         RegisterPortableObjectType(kBasePofId + 8, typeof(CommandListComponent));
         RegisterPortableObjectType(kBasePofId + 9, typeof(Command));
      }
   }
}
