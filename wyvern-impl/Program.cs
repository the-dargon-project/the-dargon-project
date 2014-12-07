using System;
using System.Windows.Forms;
using Castle.DynamicProxy;
using Dargon.Draek.Identities;
using Dargon.Draek.Identities.Hydar;
using Dargon.Draek.Identities.Management;
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
using ItzWarty.Processes;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Dargon.Wyvern {
   public class Program {
      public const int kPlatformManagementPort = 31000;
      public const int kPlatformServicePort = 31337;

      public static void Main(string[] args) {
         InitializeLogging();
         // construct libwarty dependencies
         ICollectionFactory collectionFactory = new CollectionFactory();

         // construct libwarty-proxies dependencies
         IStreamFactory streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IProcessProxy processProxy = new ProcessProxy();

         // construct Castle.Core dependencies
         ProxyGenerator proxyGenerator = new ProxyGenerator();

         // construct Platform Root Portable Object Format dependencies
         IPofContext pofContext = new PlatformRootPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);

         // construct libdargon.management dependencies
         var managementServerEndpoint = tcpEndPointFactory.CreateAnyEndPoint(kPlatformManagementPort);
         IMessageFactory managementMessageFactory = new MessageFactory();
         IManagementSessionFactory managementSessionFactory = new ManagementSessionFactory(collectionFactory, threadingProxy, pofSerializer, managementMessageFactory);
         ILocalManagementServerContext managementServerContext = new LocalManagementServerContext(collectionFactory, managementSessionFactory);
         IManagementContextFactory managementContextFactory = new ManagementContextFactory(pofContext);
         ILocalManagementRegistry localManagementServerRegistry = new LocalManagementRegistry(pofSerializer, managementContextFactory, managementServerContext);
         IManagementServerConfiguration managementServerConfiguration = new ManagementServerConfiguration(managementServerEndpoint);
         var server = new LocalManagementServer(threadingProxy, networkingProxy, managementSessionFactory, managementServerContext, managementServerConfiguration);
         server.Initialize();

         // construct system-state dependencies
         SystemState systemState = null;//new SystemStateFileSystemImpl(fileSystemProxy, configuration);
         localManagementServerRegistry.RegisterInstance(new SystemStateMob(systemState));

         // construct platform foundational dependencies
         ICacheFactory cacheFactory = new CacheFactory();
         PlatformCacheService platformCacheService = new PlatformCacheServiceImpl(collectionFactory, cacheFactory).With(x => x.Initialize());
         Caches specializedCaches = new Caches(platformCacheService);
         SpecializedCacheService specializedCacheService = new SpecializedCacheServiceImpl(specializedCaches);

         // construct backend account service dependencies
         ICache<string,long> emailToAccountIdCache = new InMemoryCache<string, long>(Accounts.Hydar.CacheNames.kEmailToAccountIdCache, new ICacheIndex[0]);
         ICache<long,AccountInformation> accountInfoByIdCache = new InMemoryCache<long, AccountInformation>(Accounts.Hydar.CacheNames.kAccountInfoByIdCache, new ICacheIndex[0]);
         IDistributedCounter accountIdCounter = specializedCacheService.GetCountingCache(Accounts.Hydar.CacheNames.kAccountIdCountingCacheName);
         IPasswordUtilities passwordUtilities = new PasswordUtilities();
         AccountCache accountCache = new AccountCache(emailToAccountIdCache, accountInfoByIdCache, accountIdCounter, passwordUtilities);
         var accountService = new AccountServiceImpl(accountCache);
         localManagementServerRegistry.RegisterInstance(new AccountCacheMob(accountCache));

         // construct frontend identity service dependencies
         ICache<string, Identity> identityByTokenHydarCache = new InMemoryCache<string, Identity>(Draek.Identities.Hydar.CacheNames.kIdentityByTokenCache, new ICacheIndex[0]);
         AuthenticationTokenFactory authenticationTokenFactory = new AuthenticationTokenFactoryImpl();
         IdentityByTokenCache identityByTokenCache = new IdentityByTokenCacheImpl(identityByTokenHydarCache);
         IAuthenticationServiceConfiguration authenticationServiceConfiguration = new AuthenticationServiceConfiguration(systemState);
         AuthenticationService authenticationService = new AuthenticationServiceImpl(accountService, authenticationTokenFactory, identityByTokenCache, authenticationServiceConfiguration);
         var identityService = new IdentityServiceProxyImpl(authenticationService);
         localManagementServerRegistry.RegisterInstance(new AuthenticationServiceMob(authenticationService));
         localManagementServerRegistry.RegisterInstance(new IdentityCacheMob(identityByTokenCache));
         localManagementServerRegistry.RegisterInstance(new AuthenticationServiceConfigurationMob(authenticationServiceConfiguration));

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

