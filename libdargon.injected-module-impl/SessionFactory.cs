using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Transport;

namespace Dargon.InjectedModule
{
   public class SessionFactory : ISessionFactory
   {
      private readonly IDtpNodeFactory dtpNodeFactory;

      public SessionFactory(IDtpNodeFactory dtpNodeFactory) {
         this.dtpNodeFactory = dtpNodeFactory;
      }

      public ISession CreateSession(int processId, IInjectedModuleConfiguration configuration) {
         return new Session(processId, configuration, dtpNodeFactory);
      }
   }
}
