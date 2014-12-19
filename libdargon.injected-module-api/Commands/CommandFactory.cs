using System.IO;
using ItzWarty;

namespace Dargon.InjectedModule.Commands {
   public class CommandFactory : ICommandFactory {
      private const string kFileRedirectionCommandType = "FILE_REDIRECTION_COMMAND";
      private const string kFileRemappingCommandType = "FILE_REMAPPING_COMMAND";

      public ICommand CreateFileRedirectionCommand(string replacedFile, string replacementPath) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               using (var targetFile = File.Open(replacedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                  WinAPI.BY_HANDLE_FILE_INFORMATION info;
                  WinAPI.GetFileInformationByHandle(targetFile.Handle, out info);
                  writer.Write((uint)info.VolumeSerialNumber);
                  writer.Write((uint)info.FileIndexHigh);
                  writer.Write((uint)info.FileIndexLow);
               }
               writer.WriteLongText(replacementPath);
            }
            return new Command(kFileRedirectionCommandType, ms.ToArray());
         }
      }

      public ICommand CreateFileRemappingCommand(string replacedFile, string vfmPath) {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               using (var targetFile = File.Open(replacedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                  WinAPI.BY_HANDLE_FILE_INFORMATION info;
                  WinAPI.GetFileInformationByHandle(targetFile.Handle, out info);
                  writer.Write((uint)info.VolumeSerialNumber);
                  writer.Write((uint)info.FileIndexHigh);
                  writer.Write((uint)info.FileIndexLow);
               }
               writer.WriteLongText(vfmPath);
            }
            return new Command(kFileRedirectionCommandType, ms.ToArray());
         }
      }

      private class Command : ICommand {
         private readonly string type;
         private readonly byte[] data;

         public Command(string type, byte[] data) {
            this.type = type;
            this.data = data;
         }

         public string Type { get { return type; } }
         public byte[] Data { get { return data; } }
      }
   }
}
