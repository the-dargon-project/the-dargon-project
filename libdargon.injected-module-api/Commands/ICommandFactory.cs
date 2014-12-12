namespace Dargon.InjectedModule.Commands
{
   public interface ICommandFactory
   {
      ICommand CreateFileSwapTask(string replacedFile, string replacementPath);
   }
}