using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Commands;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public interface ILeagueInjectedModuleConfigurationFactory
   {
      IInjectedModuleConfiguration GetPreclientConfiguration();
      IInjectedModuleConfiguration GetClientConfiguration(ICommandList commandList);
      IInjectedModuleConfiguration GetGameConfiguration();
   }
}
