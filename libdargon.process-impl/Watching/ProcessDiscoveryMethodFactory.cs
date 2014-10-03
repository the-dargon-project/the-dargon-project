using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Dargon.Processes.Watching
{
   public class ProcessDiscoveryMethodFactory : IProcessDiscoveryMethodFactory
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public IProcessDiscoveryMethod CreateOptimalProcessDiscoveryMethod()
      {
         try {
            return new WmiProcessDiscoveryMethod();
         } catch (Exception e) {
            logger.Warn("Unable to init WMIStalkerMethod: " + e.ToString());
            logger.Warn("Fall back to poll method - dargon will be less memory/cpu efficient");
            return new PollingProcessDiscoveryMethod();
         }
      }
   }

   public interface IProcessDiscoveryMethodFactory
   {
      IProcessDiscoveryMethod CreateOptimalProcessDiscoveryMethod();
   }
}
