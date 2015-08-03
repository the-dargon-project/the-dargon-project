namespace Dargon.Client.Controllers.Phases {
   public class ModificationPhaseManager {
      private readonly object synchronization = new object();
      private ModificationPhase currentPhase = null;

      public void Transition(ModificationPhase phase) {
         lock (synchronization) {
            currentPhase?.HandleExit();
            currentPhase = phase;
            currentPhase.HandleEnter();
         }
      }
   }
}