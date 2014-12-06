using System;
using System.Windows.Forms;
using Dargon.Hydar;
using Dargon.Management.Server;
using Dargon.PortableObjects;
using Dargon.Wyvern.Accounts;
using Dargon.Wyvern.Accounts.Hydar;
using Dargon.Wyvern.Accounts.Management;
using Dargon.Wyvern.Specialized;
using Dargon.Wyvern.Specialized.Hydar;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Dargon.Wyvern {
   public class Program {
      public const int kPlatformPort = 21001;

      public static void Main(string[] args) {
         InitializeLogging();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IPofContext pofContext = new PlatformRootPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);

         var serverEndpoint = tcpEndPointFactory.CreateAnyEndPoint(kPlatformPort);
         IMessageFactory messageFactory = new MessageFactory();
         IManagementSessionFactory managementSessionFactory = new ManagementSessionFactory(collectionFactory, threadingProxy, pofSerializer, messageFactory);
         ILocalManagementServerContext serverContext = new LocalManagementServerContext(collectionFactory, managementSessionFactory);
         IManagementContextFactory managementContextFactory = new ManagementContextFactory(pofContext);
         ILocalManagementRegistry localManagementRegistry = new LocalManagementRegistry(pofSerializer, managementContextFactory, serverContext);
         IManagementServerConfiguration configuration = new ManagementServerConfiguration(serverEndpoint);
         var server = new LocalManagementServer(threadingProxy, networkingProxy, managementSessionFactory, serverContext, configuration);
         server.Initialize();

         ICacheFactory cacheFactory = new CacheFactory();
         PlatformCacheService platformCacheService = new PlatformCacheServiceImpl(collectionFactory, cacheFactory).With(x => x.Initialize());
         Caches specializedCaches = new Caches(platformCacheService);
         SpecializedCacheService specializedCacheService = new SpecializedCacheServiceImpl(specializedCaches);

         ICache<string,long> emailToAccountIdCache = new InMemoryCache<string, long>(Accounts.Hydar.CacheNames.kEmailToAccountIdCache, new ICacheIndex[0]);
         ICache<long,AccountInformation> accountInfoByIdCache = new InMemoryCache<long, AccountInformation>(Accounts.Hydar.CacheNames.kAccountInfoByIdCache, new ICacheIndex[0]);
         IDistributedCounter accountIdCounter = specializedCacheService.GetCountingCache(Accounts.Hydar.CacheNames.kAccountIdCountingCacheName);
         IPasswordUtilities passwordUtilities = new PasswordUtilities();
         AccountCache accountCache = new AccountCache(emailToAccountIdCache, accountInfoByIdCache, accountIdCounter, passwordUtilities);
         var accountService = new AccountServiceImpl(accountCache);
         localManagementRegistry.RegisterInstance(new AccountCacheMob(accountCache));

         Application.Run();
      }

      private static void InitializeLogging() {
         var config = new LoggingConfiguration();
         var debuggerTarget = new DebuggerTarget();
         config.AddTarget("debugger", debuggerTarget);

         var rule2 = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(rule2);
         LogManager.Configuration = config;
      }
   }
}
