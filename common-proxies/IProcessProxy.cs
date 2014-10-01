using System.Diagnostics;

namespace Dargon
{
   public interface IProcessProxy
   {
      Process GetProcessById(int id);
      Process GetProcessOrNull(int id);
   }
}