using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Threading;
using Dargon.Game.LeagueOfLegends;
using Dargon.ModificationRepositories;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using ItzWarty;
using ItzWarty.Services;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;

namespace Dargon.Daemon
{
   public class DaemonService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly DaemonConfiguration configuration = new DaemonConfiguration();
      private readonly ServiceLocator serviceLocator = new ServiceLocator();
      private readonly ProcessInjectionService processInjectionService;
      private readonly ProcessWatcherServiceImpl processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly LeagueGameService leagueGameService;
      private readonly CountdownEvent quitCountdownEvent = new CountdownEvent(1);

      public DaemonService()
      {
         InitializeLogging();
         logger.Info("Initializing Daemon");

         serviceLocator.RegisterService(typeof(IDaemonService), this);

         processInjectionService = new ProcessInjectionServiceImpl(serviceLocator);
         processWatcherService = new ProcessWatcherServiceImpl(serviceLocator);
         modificationRepositoryService = new ModificationRepositoryServiceImpl(serviceLocator);

         leagueGameService = new LeagueGameService(processWatcherService, modificationRepositoryService);
      }

      private void InitializeLogging()
      {
//         Console.WindowWidth = Console.BufferWidth = Math.Min(160, Console.LargestWindowWidth - 10);
//         WinAPI.SetWindowPos(WinAPI.GetConsoleWindow(), IntPtr.Zero, 100, 100, 0, 0, WinAPI.SetWindowPosFlags.SWP_NOSIZE);

         // Step 1. Create configuration object 
         var config = new LoggingConfiguration();

         // Step 2. Create targets and add them to the configuration 
//         var consoleTarget = new ColoredConsoleTarget();
//         config.AddTarget("console", consoleTarget);

         var debuggerTarget = new NLog.Targets.DebuggerTarget();
         config.AddTarget("debugger", debuggerTarget);

//         var fileTarget = new FileTarget();
//         config.AddTarget("file", fileTarget);

         // Step 3. Set target properties 
//         consoleTarget.Layout = @"${date:format=HH\\:MM\\:ss} ${logger} ${message}";
//         fileTarget.FileName = "${basedir}/file.txt";
//         fileTarget.Layout = "${message}";

         // Step 4. Define rules
//         var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
//         config.LoggingRules.Add(rule1);

         var rule2 = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(rule2);

//         var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
//         config.LoggingRules.Add(rule2);

         // Step 5. Activate the configuration
         LogManager.Configuration = config;
      }

      public void Run() { quitCountdownEvent.Wait(); }
   }
}