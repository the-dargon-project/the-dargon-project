using System.Windows.Forms;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using Dargon.Processes.Injection;
using Dargon.Transport;
using Dargon.Trinkets.Transport;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Trinkets.Proxy {
   public class TrinketProxyEgg : INestApplicationEgg {
      private readonly StreamFactory streamFactory;
      private readonly PofSerializer pofSerializer;
      private readonly ProcessInjectionService processInjectionService;
      private readonly TrinketInternalUtilities trinketInternalUtilities;
      private readonly TrinketDtpServerFactoryImpl trinketDtpServerFactory;
      private TrinketBridgeImpl trinketBridge; // Reference prevents GC

      public TrinketProxyEgg() {
         streamFactory = new StreamFactory();
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new TrinketsApiPofContext());
         });
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         pofSerializer = new PofSerializer(pofContext);
         var processInjector = new ProcessInjectorImpl();
         ProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfigurationImpl(injectionAttempts: 10, injectionAttemptsDelay: 200);
         processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
         IDtpNodeFactory transportNodeFactory = new DefaultDtpNodeFactory();
         BootstrapConfigurationGenerator bootstrapConfigurationGenerator = new BootstrapConfigurationGeneratorImpl();
         trinketInternalUtilities = new TrinketInternalUtilitiesImpl(fileSystemProxy);
         trinketDtpServerFactory = new TrinketDtpServerFactoryImpl(streamFactory, transportNodeFactory, bootstrapConfigurationGenerator);
      }

      public NestResult Start(IEggParameters parameters) {
         var configuration = pofSerializer.Deserialize<TrinketStartupConfiguration>(streamFactory.CreateMemoryStream(parameters.Arguments).Reader);
         var trinketDtpServer = trinketDtpServerFactory.Create(configuration);
         trinketBridge = new TrinketBridgeImpl(processInjectionService, trinketInternalUtilities, configuration, trinketDtpServer);
         var success = trinketBridge.Initialize();
         return success ? NestResult.Success : NestResult.Failure;
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }
   }
}
