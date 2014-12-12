namespace Dargon.InjectedModule.Commands
{
   public interface ICommandFactory
   {
      ICommand CreateFileRedirectionCommand(string replacedFile, string replacementPath);
   }
}