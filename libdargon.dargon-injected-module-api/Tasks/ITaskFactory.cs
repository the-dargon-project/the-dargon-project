namespace Dargon.InjectedModule.Tasks
{
   public interface ITaskFactory
   {
      ITask CreateFileSwapTask(string replacedFile, string replacementPath);
   }
}