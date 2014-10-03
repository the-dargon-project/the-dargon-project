using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.InjectedModule.Tasks
{
   public class TaskFactory : ITaskFactory
   {
      private const string kFileSwapTaskType = "FILE_SWAP";

      public ITask CreateFileSwapTask(string replacedFile, string replacementPath)
      {
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
            return new Task(kFileSwapTaskType, ms.ToArray());
         }
      }

      private class Task : ITask
      {
         private readonly string type;
         private readonly byte[] data;

         public Task(string type, byte[] data)
         {
            this.type = type;
            this.data = data;
         }

         public string Type { get { return type; } }
         public byte[] Data { get { return data; } }
      }
   }
}
