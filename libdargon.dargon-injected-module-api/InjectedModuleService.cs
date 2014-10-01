
namespace Dargon.InjectedModule
{
   public interface InjectedModuleService
   {
      ISession InjectToProcess(int processId, IInjectedModuleConfiguration configuration);
   }
}
