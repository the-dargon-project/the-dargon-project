using Dargon.Daemon;
using Dargon.Game;
using Dargon.IO;
using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Management.Server;
using Dargon.Processes.Watching;
using Dargon.RADS;
using Dargon.RADS.Archives;
using Dargon.RADS.Manifest;
using Dargon.Services;
using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Spawner;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Processes;
using ItzWarty.Threading;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dargon.LeagueOfLegends.Utilities;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends {
   public class LeagueGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly RadsServiceImpl radsService;
      private readonly LeagueProcessWatcherServiceImpl leagueProcessWatcherService;

      public LeagueGameServiceImpl(ClientConfiguration clientConfiguration, IThreadingProxy threadingProxy, IFileSystemProxy fileSystemProxy, ILocalManagementRegistry localManagementRegistry, IServiceClient localServiceClient, DaemonService daemonService, TemporaryFileService temporaryFileService, IProcessProxy processProxy, ProcessWatcherService processWatcherService, ModificationLoader modificationLoader, TrinketSpawner trinketSpawner)
      {
         logger.Info("Initializing League Game Service");
         this.fileSystemProxy = fileSystemProxy;

         var leagueConfiguration = new LeagueConfiguration();
         var riotSolutionLoader = new RiotSolutionLoader();
         this.radsService = new RadsServiceImpl(configuration.RadsPath);
         var commandFactory = new CommandFactoryImpl();
         this.leagueProcessWatcherService = new LeagueProcessWatcherServiceImpl(processWatcherService);
         var leagueSessionService = new LeagueSessionServiceImpl(processProxy, leagueProcessWatcherService);
         var riotFileSystem = new RiotFileSystem(radsService, RiotProjectType.GameClient);
         var leagueTrinketSpawnConfigurationFactory = new LeagueTrinketSpawnConfigurationFactoryImpl();
         var leagueBuildHelperSomething = new LeagueBuildUtilities(leagueConfiguration, fileSystemProxy, riotSolutionLoader, temporaryFileService, commandFactory);
         localManagementRegistry.RegisterInstance(new LeagueModificationsMob(clientConfiguration, modificationLoader, leagueBuildHelperSomething));
         var lifecycleService = new LeagueLifecycleServiceImpl(trinketSpawner, leagueBuildHelperSomething, leagueSessionService, radsService, leagueTrinketSpawnConfigurationFactory, modificationLoader);
         lifecycleService.Initialize();
         RunDebugActions();
      }

      private void RunDebugActions() {
//         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
//         var gameResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Game)).ToList();
//         gameResolutionTasks.ForEach(task => task.WaitForChainCompletion());
//
//         var gameCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Game)).ToList();
//         gameCompilationTasks.ForEach(task => task.WaitForChainCompletion());
//
//         var commands = leagueGameModificationLinkerService.LinkModificationObjects();

//         var dpmf = @"C:\Users\ItzWarty\.dargon\repositories\tencent-art-pack\.dpm\objects\1c\f1bb96b12ce58bf10ae57e2988abc61c2517a068";
//         var dpmfBytes = File.ReadAllBytes(dpmf);
//         for (var offset = 0; offset < 128; offset++) {
//            for (var cut = 0; cut < 128; cut++) {
//               try {
//                  var ms = new MemoryStream(dpmfBytes, offset, dpmfBytes.Length - offset - cut);
//
//                  ZInputStream zinputStream = new ZInputStream(ms);
//                  MemoryStream memoryStream = new MemoryStream();
//                  int num;
//                  while ((num = zinputStream.Read()) != -1)
//                     memoryStream.WriteByte((byte)num);
//                  Console.WriteLine(offset + ": " + cut + " succeeded");
//                  while (true) ;
//               } catch (Exception e) {
//                  Console.WriteLine(offset + ": " + cut + " failed");
//               }
//            }
//            //            File.WriteAllBytes("C:/DargonDump/out.dds", memoryStream.ToArray());
//         }

//         var loader = new ReleaseManifestLoader();
//         var manifest = loader.LoadFile(@"C:\Users\ItzWarty\.dargon\temp\5d957b6fb5d9dd4c9a8127ba7c0e31bc\releasemanifest");
//         var resolver = new Resolver(manifest.Root);
//         var annieSquare = (ReleaseManifestFileEntry)resolver.Resolve("Annie_Square.dds").First();
//
//         var raf = new RiotArchive(@"C:\Users\ItzWarty\.dargon\temp\5d957b6fb5d9dd4c9a8127ba7c0e31bc\0.0.0.235\Archive_3.raf", "");
//         var annieSquareRafEntry = raf.GetDirectoryFile().GetFileList().SearchFileEntries("Annie_Square.dds").First();
//         SectorCollectionSerializer scs = new SectorCollectionSerializer();
//         var vfmSectors = scs.Deserialize(fileSystemProxy.OpenFile(@"C:\Users\ItzWarty\.dargon\temp\5d957b6fb5d9dd4c9a8127ba7c0e31bc\0.0.0.235\Archive_3.raf.dat.vfm").Reader.__Reader);
//         var vfmFile = new VirtualFile(vfmSectors);
//         var data = vfmFile.Read(annieSquareRafEntry.FileOffset, annieSquareRafEntry.FileSize);
//         using (var decompressedStream = new FileStream("C:/DargonDump/out.dds", FileMode.Create, FileAccess.Write)) {
//            using (var compressedStream = new MemoryStream(data))
//            using (var decompressor = new ZlibStream(decompressedStream, CompressionMode.Decompress)) {
//               compressedStream.CopyTo(decompressor);
//            }
//         }
//            File.WriteAllBytes("C:/DargonDump/out.dds", data);


//         for (var offset = 0; offset < 128; offset++) {
//            bool k = false;
//            for (var cut = 0; cut < 128; cut++) {
//               try {
//                  var ms = new MemoryStream(data, offset, data.Length - offset - cut);
//
//                  //                  ZInputStream zinputStream = new ZInputStream(ms);
//                  //                  MemoryStream memoryStream = new MemoryStream();
//                  //                  int num;
//                  //                  while ((num = zinputStream.Read()) != -1)
//                  //                     memoryStream.WriteByte((byte)num);
//
//                  var decompressedStream = new MemoryStream();
//                  using (var deflateStream = new DeflateStream(ms, CompressionMode.Decompress)) {
//                     deflateStream.CopyTo(decompressedStream);
//                  }
//                  Console.WriteLine(offset + ": " + cut + " succeeded");
//                  k = true;
//               } catch (Exception e) {
//                  Console.WriteLine(offset + ": " + cut + " failed");
//               }
//            }
//            if (k)
//               while (true) ;
//         }
         //            File.WriteAllBytes("C:/DargonDump/out.dds", memoryStream.ToArray());
                   

            //
            ////         var decompressedStream = new MemoryStream();
            ////         using (var deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress)) {
            ////            deflateStream.CopyTo(decompressedStream);
            ////         }
            ////         File.WriteAllBytes("C:/DargonDump/out.dds", decompressedStream.ToArray());
            //                  ZInputStream zinputStream = new ZInputStream((Stream)new MemoryStream(data));
            //                  MemoryStream memoryStream = new MemoryStream();
            //                  int num;
            //                  while ((num = zinputStream.Read()) != -1)
            //                     memoryStream.WriteByte((byte)num);
            //                  File.WriteAllBytes("C:/DargonDump/out.dds", memoryStream.ToArray());
            //
            //
            //         //         Dump("DATA/Characters/Annie", "C:/DargonDump");

            //foreach (var mod in modificationRepositoryService.EnumerateModifications(GameType.Any)) {
            //   modificationRepositoryService.DeleteModification(mod);
            //}
            //
//            modificationRepositoryService.ImportLegacyModification(
//               "tencent-art-pack",
//               @"C:\lolmodprojects\Tencent Art Pack 8.74 Minier",
//               Directory.GetFiles(@"C:\lolmodprojects\Tencent Art Pack 8.74 Minier\ArtPack", "*", SearchOption.AllDirectories),
//               GameType.LeagueOfLegends);

            // foreach (var mod in modificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends)) {
            //    logger.Info(mod.RepositoryName);
            // 
            //    var metadata = mod.Metadata;
            //    logger.Info("mod: {0} {1} by {2} at {3} for {4}".F(metadata.Name, metadata.Version, metadata.Authors.Join(", "), metadata.Website, metadata.Targets.Select(t=>t.Name).Join(", ")));
            // 
            //    leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Client | ModificationTargetType.Game).WaitForChainCompletion();
            //    leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client | ModificationTargetType.Game).WaitForChainCompletion();
            //    leagueGameModificationLinkerService.LinkModificationObjects();
            // }
            // for (var mod in modificationRepositoryService)
            // modificationRepositoryService.ClearModifications();
            //         var mod = modificationImportService.ImportLegacyModification(
            //            GameType.LeagueOfLegends,
            //            @"C:\lolmodprojects\Tencent Art Pack 8.74",
            //            Directory.GetFiles(@"C:\lolmodprojects\Tencent Art Pack 8.74\ArtPack\Client\Assets", "*", SearchOption.AllDirectories));
            //         modificationRepositoryService.AddModification(mod);
            //
            // var mod = modificationImportService.ImportLegacyModification(
            //    GameType.LeagueOfLegends,
            //    @"C:\lolmodprojects\Alm1ghty UI 4.4 Foxe Style",
            //    Directory.GetFiles(@"C:\lolmodprojects\Alm1ghty UI 4.4 Foxe Style", "*", SearchOption.AllDirectories));
            // modificationRepositoryService.AddModification(mod);
         }

      private void Dump(string sourcePath, string outputDirectory) {
         var gameClientManifest = radsService.GetReleaseManifestUnsafe(RiotProjectType.GameClient);
         var levels = ((IReadableDargonNode)gameClientManifest.Root).GetRelativeOrNull(sourcePath);
         var archiveLoader = new RiotArchiveLoader(configuration.RadsPath);
         var archiveIds = new HashSet<uint>(gameClientManifest.Files.Select(x => x.ArchiveId));
         var archivesById = new Dictionary<uint, IReadOnlyList<RiotArchive>>();
         foreach (var archiveId in archiveIds) {
            IReadOnlyList<RiotArchive> archives;
            if (archiveLoader.TryLoadArchives(archiveId, out archives)) {
               archivesById.Add(archiveId, archives);
            }
         }

         var stack = new Stack<IReadableDargonNode>();
         stack.Push(levels);
         while (stack.Any()) {
            var node = stack.Pop();
            if (node.Children.Any()) {
               node.Children.ForEach(stack.Push);
            } else {
               var archives = archivesById[((ReleaseManifestFileEntry)node).ArchiveId];
               foreach (var archive in archives) {
                  var entry = archive.GetDirectoryFile().GetFileList().GetFileEntryOrNull(node.GetPath());
                  if (entry != null) {
                     var outputPath = outputDirectory + "/" + node.GetPath();
                     var outputData = entry.GetContent();
                     
                     fileSystemProxy.PrepareParentDirectory(outputPath);
                     File.WriteAllBytes(outputPath, outputData);
                     Console.WriteLine("Happily dumped " + node.GetPath());
                     goto yeehaw;
                  }
               }
               Console.WriteLine("Couldn't dump " + node.GetPath());
            }
            yeehaw:
            if (true) { }
         }
      }
   }
}
