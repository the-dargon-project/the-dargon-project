using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ItzWarty.IO;

namespace Dargon.Trinkets {
   public interface TrinketInternalUtilities {
      string GetInjectedModulePath();
   }

   public class TrinketInternalUtilitiesImpl : TrinketInternalUtilities {
      private readonly IFileSystemProxy fileSystemProxy;

      public TrinketInternalUtilitiesImpl(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public string GetInjectedModulePath() {
         // Todo: This is so ghetto I "can't even".
         var fi = fileSystemProxy.GetFileInfo(Assembly.GetAssembly(typeof(TrinketInternalUtilities)).Location);
         return Path.Combine(fi.Parent.FullName, "..", "trinket-dim", "Dargon - Injected Module.dll");
      }
   }
}
