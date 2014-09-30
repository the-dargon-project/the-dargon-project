using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Dargon.Processes.Injection;
using Dargon.Transport;
using ItzWarty.Services;
using NLog;

namespace Dargon.InjectedModule
{
   public class InjectedModuleServiceImpl : InjectedModuleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjectionService processInjectionService;
      private readonly ConcurrentDictionary<int, Session> sessionsByProcessId = new ConcurrentDictionary<int, Session>();
      private readonly IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();

      public InjectedModuleServiceImpl(IServiceLocator serviceLocator, ProcessInjectionService processInjectionService)
      {
         logger.Info("Initializing Injected Module Service");

         serviceLocator.RegisterService(typeof(InjectedModuleService), this);

         this.processInjectionService = processInjectionService;

         if (!File.Exists(GetInjectedDllPath())) {
            logger.Warn("Injected DLL does not exist at " + GetInjectedDllPath() + "!");
         }
      }

      public ISession InjectToProcess(int processId, InjectedModuleConfiguration configuration) 
      {
         logger.Info("Injecting to process " + processId);

         var session = new Session(processId, configuration, dtpNodeFactory);
         processInjectionService.InjectToProcess(processId, GetInjectedDllPath());
         sessionsByProcessId.AddOrUpdate(processId, session, (a, b) => session);
         session.Ended += HandleSessionEnded;
         return session;
      }

      private void HandleSessionEnded(ISession session, SessionEndedEventArgs sessionEndedEventArgs)
      {
         session.Ended -= HandleSessionEnded;

         Session removedSession;
         sessionsByProcessId.TryRemove(session.ProcessId, out removedSession);
      }

      private string GetInjectedDllPath()
      {
#if DEBUG
         var driveName = new DriveInfo(Process.GetCurrentProcess().Modules[0].FileName).Name;
         return Path.Combine(driveName, @"my-repositories\the-dargon-project\Debug", "Dargon - Injected Module.dll");
#else
         // TODO: Manifest for DIM.dll
#endif
      }
   }
}
