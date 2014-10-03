using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Tasks;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public interface ILeagueInjectedModuleConfigurationFactory
   {
      IInjectedModuleConfiguration GetPreclientConfiguration();
      IInjectedModuleConfiguration GetClientConfiguration(ITasklist tasklist);
      IInjectedModuleConfiguration GetGameConfiguration();
   }
}
