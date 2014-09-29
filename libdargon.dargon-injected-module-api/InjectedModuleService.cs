namespace Dargon.InjectedModule
{
   public interface InjectedModuleService
   {
      ISession InjectToProcess(int processId, BootstrapConfiguration bootstrapConfiguration);
   }

   public interface ISession
   {

   }

}
