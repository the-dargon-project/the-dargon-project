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
      }

      public ISession InjectToProcess(int processId, BootstrapConfiguration bootstrapConfiguration) 
      {
         logger.Info("Injecting to process " + processId);

         var session = new Session(processId, bootstrapConfiguration, dtpNodeFactory);
         processInjectionService.InjectToProcess(processId, GetInjectedDllPath());
         sessionsByProcessId.AddOrUpdate(processId, session, (a, b) => session);
         return session;
      }

      private string GetInjectedDllPath()
      {
         var path = new FileInfo(Process.GetCurrentProcess().Modules[0].FileName).Directory.FullName;
         return Path.Combine(path, "Dargon - Injected Module.dll");
      }
   }
}
