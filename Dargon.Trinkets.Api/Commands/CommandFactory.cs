namespace Dargon.Trinkets.Commands {
   public interface CommandFactory {
      Command CreateFileRedirectionCommand(string replacedFile, string replacementPath);
      Command CreateFileRemappingCommand(string replacedFile, string vfmPath);
   }
}