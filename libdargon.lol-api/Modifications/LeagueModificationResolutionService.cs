using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationResolutionService
   {
      IResolutionTask StartModificationResolution(IModification modification, ModificationTargetType target);
   }
}
