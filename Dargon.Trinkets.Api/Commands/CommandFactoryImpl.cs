using System.IO;
using ItzWarty;

namespace Dargon.Trinkets.Commands {
   public class CommandFactoryImpl : CommandFactory {
      private const string kFileRedirectionCommandType = "FILE_REDIRECTION_COMMAND";
      private const string kFileRemappingCommandType = "FILE_REMAPPING_COMMAND";

      public Command CreateFileRedirectionCommand(string replacedFile, string replacementPath) {
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

      public Command CreateFileRemappingCommand(string replacedFile, string vfmPath) {
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
            return new Command(kFileRemappingCommandType, ms.ToArray());
         }
      }
   }
}
