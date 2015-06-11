namespace Dargon.Processes.Injection {
   public interface ProcessInjector {
      /// <summary>
      /// Attempts to inject the given dll to the given target process numerous times. 
      /// 
      /// (Creates a remote thread that runs in the virtual address space of the target process)
      /// </summary>
      /// <returns>
      /// Whether dll was successfully injected and its entry point completed successfully.
      /// </returns>
      ProcessInjectionResult TryInjectToProcess(int targetProcessId, string dllPath, int attempts, int attemptsIntervalMilliseconds);
   }
}