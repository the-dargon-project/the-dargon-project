using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationCompilationService
   {
      ICompilationTask CompileModification(IModification modification, ModificationTargetType target);
   }
}
