using System;
using System.Diagnostics;
using System.IO;

namespace Dargon.Daemon
{
   public class DaemonConfiguration : IDaemonConfiguration
   {
      private const string APP_DATA_SUBDIRECTORY = "ItzWarty/Dargon";
      private readonly string temporaryDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_DATA_SUBDIRECTORY);

      public bool IsDebugCompilation
      {
         get
         {
#if DEBUG
            return true;
#else
            return false;
#endif
         }
      }

      public string TemporaryDirectoryPath { get { return temporaryDirectoryPath; } }
   }
}