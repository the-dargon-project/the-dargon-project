using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationResolutionService
   {
      IResolutionTask ResolveModification(IModification modification, ModificationTargetType type);
   }
}
