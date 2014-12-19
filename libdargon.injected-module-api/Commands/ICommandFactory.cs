namespace Dargon.InjectedModule.Commands {
   public interface ICommandFactory {
      ICommand CreateFileRedirectionCommand(string replacedFile, string replacementPath);
      ICommand CreateFileRemappingCommand(string replacedFile, string vfmPath);
   }
}