using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Transport;

namespace Dargon.Trinkets.Transport.Helpers {
   public interface StatelessRithFactory {
      RemotelyInitializedTransactionHandler Create(uint transactionId);
   }
}
