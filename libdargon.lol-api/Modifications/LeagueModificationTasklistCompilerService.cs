using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule.Tasks;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationTasklistCompilerService
   {
      ITasklist BuildTasklist(IModification modification, ModificationTargetType target);
   }
}
