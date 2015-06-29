using Dargon.Modifications;
using Dargon.Trinkets.Commands;

namespace Dargon.LeagueOfLegends.Modifications {
   public interface LeagueModificationCommandListCompilerService
   {
      CommandList BuildCommandList(IModification modification, ModificationTargetType target);
   }
}
