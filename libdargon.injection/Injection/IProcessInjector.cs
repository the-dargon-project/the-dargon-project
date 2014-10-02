namespace Dargon.Processes.Injection
{
   public interface IProcessInjector
   {
      /// <summary>
      /// Tries [attempts] times to inject [dllPath] into the remote process.  If the injection
      /// fails, it waits [attemptInterval] milliseconds before trying again.
      /// </summary>
      /// <param name="processId">
      /// The processID of the process we want to create a remote thread in
      /// </param>
      /// <param name="dllPath">
      /// Path to the DLL which we are injecting into the process.
      /// </param>
      /// <param name="attempts">
      /// The number of times we will attempt to inject into the process before giving up.
      /// </param>
      /// <param name="attemptInterval">
      /// The interval we will wait between consecutive injections.
      /// </param>
      bool TryInject(
         int processId,
         string dllPath,
         int attempts = 100,
         int attemptInterval = 200);
   }
}