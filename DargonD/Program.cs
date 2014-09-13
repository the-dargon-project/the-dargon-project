using System;
using System.IO;
using System.Runtime.InteropServices;
using Dargon.FileSystem;
using Dargon.IO.RADS;
using ItzWarty;

namespace Dargon
{
   class Program
   {
      public static void Main(string[] args) { 
         var core = new DaemonCore();
         core.Run();
      }
   }

   public class DaemonCore
   {
      private readonly DaemonConfiguration configuration = new DaemonConfiguration();
      public void Run() { 
//         var lolfs = new RiotFileSystem(configuration.RadsPath, RiotProjectType.GameClient);
//         Dump(lolfs.AllocateRootHandle(), lolfs);
      }

      private void Dump(IFileSystemHandle handle, IFileSystem fs)
      {
         IFileSystemHandle[] childHandles;
         if (fs.AllocateChildrenHandles(handle, out childHandles) == IoResult.Success) {
            foreach (var childHandle in childHandles) {
               Dump(childHandle, fs);
            }
            fs.FreeHandles(childHandles);
         }

         byte[] data;
         if (fs.ReadAllBytes(handle, out data) == IoResult.Success) {
            string path;
            if (fs.GetPath(handle, out path) == IoResult.Success) {
//               if (!path.Contains("dds") || !path.Contains("Menu"))
//                  return;
               byte[] bytes;
               if (fs.ReadAllBytes(handle, out bytes) == IoResult.Success) {
                  string dumpPath = "C:/Dump/" + path;
                  Util.PrepareParentDirectory(dumpPath);
                  //File.WriteAllBytes(dumpPath, bytes);
                  Console.WriteLine("Dumped " + dumpPath);
               }
            }
         }
      }
   }

   public class DaemonConfiguration
   {
      public string RadsPath { get { return @"V:\Riot Games\League of Legends\RADS"; } }
   }
}
