namespace Dargon.InjectedModule
{
   public interface InjectedModuleService
   {
      ISession InjectToProcess(int processId, InjectedModuleConfiguration configuration);
   }

   public interface ISession
   {

   }

}
