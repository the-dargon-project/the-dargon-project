namespace Dargon.Processes.Injection {
   public enum ProcessInjectionResult {
      None,

      /// <summary>
      /// The dll was successfully injected and its entry point completed within
      /// a certain timeout window.
      /// </summary>
      Success,

      /// <summary>
      /// The dll could not be injected, usually due to the target process dying
      /// or the injector having insufficient privileges.
      /// </summary>
      InjectionFailed,

      /// <summary>
      /// The dll was successfully injected but its main entry point failed to
      /// complete within the expected time window.
      /// </summary>
      DllFailed
   }
}