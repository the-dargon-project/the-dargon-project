namespace Dargon.Client.Controllers.Phases {
   public interface ModificationPhase {
      void HandleEnter();
      void HandleExit();
   }
}