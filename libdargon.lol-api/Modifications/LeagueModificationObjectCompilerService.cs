using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationObjectCompilerService
   {
      ICompilationTask CompileObjects(IModification modification, ModificationTargetType target);
   }
}
