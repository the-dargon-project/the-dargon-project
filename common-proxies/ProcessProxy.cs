using System;
using System.Diagnostics;

namespace Dargon
{
   public class ProcessProxy : IProcessProxy
   {
      public Process GetProcessById(int id) { return Process.GetProcessById(id); }

      public Process GetProcessOrNull(int id)
      {
         try {
            return GetProcessById(id);
         } catch (ArgumentException e) {
            return null; // process already exited
         }
      }
   }
}
