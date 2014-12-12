using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule.Commands;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationCommandListCompilerService
   {
      ICommandList BuildCommandList(IModification modification, ModificationTargetType target);
   }
}
