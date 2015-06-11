using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using Dargon.Processes.Injection;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Trinkets.Proxy {
   public class TrinketProxyEgg : INestApplicationEgg {
      private readonly StreamFactory streamFactory;
      private readonly PofSerializer pofSerializer;
      private readonly ProcessInjectionService processInjectionService;

      public TrinketProxyEgg() {
         streamFactory = new StreamFactory();
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new TrinketsApiPofContext());
         });
         pofSerializer = new PofSerializer(pofContext);
         var processInjector = new ProcessInjectorImpl();
         ProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfigurationImpl(injectionAttempts: 10, injectionAttemptsDelay: 200);
         processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
      }

      public NestResult Start(IEggParameters parameters) {
         var configuration = pofSerializer.Deserialize<TrinketStartupConfiguration>(streamFactory.CreateMemoryStream(parameters.Arguments).Reader);
         var injectionSuccessful = processInjectionService.InjectToProcess(configuration.TargetProcessId, configuration.TrinketDllPath);
         return injectionSuccessful ? NestResult.Success : NestResult.Failure;
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }
   }
}
