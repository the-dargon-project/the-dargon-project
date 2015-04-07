using Castle.DynamicProxy;
using Dargon.FinalFantasyXIII;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.LeagueOfLegends;
using Dargon.Management.Server;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.PortableObjects;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using Dargon.Services;
using Dargon.Services.Server;
using Dargon.Services.Server.Phases;
using Dargon.Services.Server.Sessions;
using Dargon.Transport;
using Dargon.Tray;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Processes;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Dargon.Daemon
{
   public static class Program {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int kDaemonManagementPort = 21000;

      public static void Main(string[] args) {
         new CoreDaemonApplicationEgg().Start(null);
      }
   }
}
