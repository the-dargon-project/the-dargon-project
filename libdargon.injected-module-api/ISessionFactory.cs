
namespace Dargon.InjectedModule
{
   public interface ISessionFactory
   {
      ISession CreateSession(int processId, IInjectedModuleConfiguration configuration);
   }
}
