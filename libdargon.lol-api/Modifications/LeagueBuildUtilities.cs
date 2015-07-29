using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dargon.IO;
using Dargon.IO.Resolution;
using Dargon.Modifications;
using Dargon.PortableObjects;
using Dargon.RADS;
using Dargon.RADS.Archives;
using Dargon.RADS.Manifest;
using Dargon.Trinkets.Commands;
using Dargon.VirtualFileMaps;
using Ionic.Zlib;
using ItzWarty;
using ItzWarty.IO;
using NLog;
using Component = Dargon.Modifications.Component;

namespace Dargon.LeagueOfLegends.Modifications {
   public class LeagueBuildUtilities {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration leagueConfiguration;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly TemporaryFileService temporaryFileService;
      private readonly CommandFactory commandFactory;
      private readonly Dictionary<RiotProjectType, Tuple<WeakReference<RiotProject>, DateTime, WeakReference<Resolver>>> cache = new Dictionary<RiotProjectType, Tuple<WeakReference<RiotProject>, DateTime, WeakReference<Resolver>>>();

      public LeagueBuildUtilities(LeagueConfiguration leagueConfiguration, IFileSystemProxy fileSystemProxy, RiotSolutionLoader riotSolutionLoader, TemporaryFileService temporaryFileService, CommandFactory commandFactory) {
         this.leagueConfiguration = leagueConfiguration;
         this.fileSystemProxy = fileSystemProxy;
         this.riotSolutionLoader = riotSolutionLoader;
         this.temporaryFileService = temporaryFileService;
         this.commandFactory = commandFactory;
      }

      public void LoadProjectAndResolver(RiotProjectType projectType, out RiotProject riotProject, out Resolver resolver) {
         Tuple<WeakReference<RiotProject>, DateTime, WeakReference<Resolver>> tuple;
         if (cache.TryGetValue(projectType, out tuple) &&
             tuple.Item1.TryGetTarget(out riotProject) &&
             tuple.Item3.TryGetTarget(out resolver) &&
             File.GetLastWriteTime(riotProject.ReleaseManifest.Path) == tuple.Item2) {
            logger.Info("Successfully saved project and resolver from GCing: " + projectType);
         } else {
            logger.Info("Constructing new project and resolver: " + projectType);
            var riotProjectLoader = new RiotProjectLoader(leagueConfiguration.RadsPath);
            riotProject = riotProjectLoader.LoadProject(projectType);
            resolver = new Resolver(riotProject.ReleaseManifest.Root);
            var manifestLastModified = File.GetLastWriteTime(riotProject.ReleaseManifest.Path);
            cache[projectType] = new Tuple<WeakReference<RiotProject>, DateTime, WeakReference<Resolver>>(new WeakReference<RiotProject>(riotProject), manifestLastModified, new WeakReference<Resolver>(resolver));
         }
      }

      public bool ResolveModification(Modification modification, CancellationToken cancellationToken) {
         logger.Info($"Begin resolving modification {modification.RepositoryName}.");
         if (cancellationToken.IsCancellationRequested) {
            logger.Info($"End resolving modification {modification.RepositoryName}.");
            return false;
         }

         RiotProject airProject, gameProject;
         Resolver airResolver, gameResolver;

         LoadProjectAndResolver(RiotProjectType.AirClient, out airProject, out airResolver);
         LoadProjectAndResolver(RiotProjectType.GameClient, out gameProject, out gameResolver);

         var resolutionTableComponent = modification.GetComponent<LeagueResolutionTableComponent>();
         var resolutionTable = resolutionTableComponent.Table;
         var contentPath = Path.Combine(modification.RepositoryPath, "content");
         var contentDirectory = fileSystemProxy.GetDirectoryInfo(contentPath);
         var resolvableFiles = contentDirectory.EnumerateFiles("*", SearchOption.AllDirectories);
         var relativeResolvableFilePaths = resolvableFiles.Select(f => f.FullName.Substring(contentPath.Length + 1));
         foreach (var resolvableFilePath in relativeResolvableFilePaths) {
            if (cancellationToken.IsCancellationRequested) {
               logger.Info($"End resolving modification {modification.RepositoryName}.");
               return false;
            }
            LeagueResolutionTableValue entryValue;
            if (!resolutionTable.TryGetValue(resolvableFilePath, out entryValue)) {
               entryValue = new LeagueResolutionTableValue();
               resolutionTable.Add(resolvableFilePath, entryValue);
            }

            Resolver[] orderedResolvers;
            if (entryValue.Target == LeagueModificationTarget.Client) {
               orderedResolvers = new[] { airResolver, gameResolver };
            } else {
               orderedResolvers = new[] { gameResolver, airResolver };
            }

            foreach (var resolver in orderedResolvers) {
               var resolution = resolver.Resolve(resolvableFilePath, entryValue.ResolvedPath).FirstOrDefault();
               if (resolution == null) {
                  entryValue.ResolvedPath = null;
                  entryValue.Target = LeagueModificationTarget.Invalid;
               } else {
                  entryValue.ResolvedPath = resolution.GetPath();
                  entryValue.Target = resolver == airResolver ? LeagueModificationTarget.Client : LeagueModificationTarget.Game;
                  break;
               }
            }
         }
         resolutionTableComponent.NotifyUpdated();
         logger.Info($"End resolving modification {modification.RepositoryName}.");
         return true;
      }

      public bool CompileModification(Modification modification, CancellationToken cancellationToken) {
         logger.Info($"Begin compiling modification {modification.RepositoryName}.");
         if (cancellationToken.IsCancellationRequested) {
            logger.Info($"End resolving modification {modification.RepositoryName}.");
            return false;
         }
         var resolutionTableComponent = modification.GetComponent<LeagueResolutionTableComponent>();
         var resolutionTable = resolutionTableComponent.Table;
         var contentPath = Path.Combine(modification.RepositoryPath, "content");
         var contentDirectory = fileSystemProxy.GetDirectoryInfo(contentPath);
         var contentFiles = contentDirectory.EnumerateFiles("*", SearchOption.AllDirectories);
         var contentRelativePaths = contentFiles.Select(f => f.FullName.Substring(contentPath.Length + 1));
         var objectsPath = Path.Combine(modification.RepositoryPath, "objects");
         fileSystemProxy.PrepareDirectory(objectsPath);
         File.SetAttributes(objectsPath, File.GetAttributes(objectsPath) | FileAttributes.Hidden);
         foreach (var contentRelativePath in contentRelativePaths) {
            if (cancellationToken.IsCancellationRequested) {
               logger.Info($"End resolving modification {modification.RepositoryName}.");
               return false;
            }
            LeagueResolutionTableValue resolutionTableValue;
            if (!resolutionTable.TryGetValue(contentRelativePath, out resolutionTableValue)) {
               continue;
            }
            if (resolutionTableValue.Target == LeagueModificationTarget.Game) {
               var contentFilePath = Path.Combine(contentPath, contentRelativePath);
               var objectsFilePath = Path.Combine(objectsPath, contentRelativePath);
               if (!File.Exists(objectsFilePath) || File.GetLastWriteTime(objectsFilePath) < File.GetLastWriteTime(contentFilePath)) {
                  fileSystemProxy.PrepareParentDirectory(objectsFilePath);
                  using (var contentStream = fileSystemProxy.OpenFile(contentFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                  using (var objectsStream = fileSystemProxy.OpenFile(objectsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                  using (ZlibStream compressionStream = new ZlibStream(objectsStream.__Stream, CompressionMode.Compress, true)) {
                     contentStream.__Stream.CopyTo(compressionStream);
                  }
               }
            }
         }
         logger.Info($"End resolving modification {modification.RepositoryName}.");
         return true;
      }

      public CommandList LinkAirModifications(IReadOnlyList<Modification> modifications) {
         DefaultCommandList commandList = new DefaultCommandList();
         foreach (var modification in modifications) {
            var resolutionTableComponent = modification.GetComponent<LeagueResolutionTableComponent>();
            var resolutionTable = resolutionTableComponent.Table;
            var contentPath = Path.Combine(modification.RepositoryPath, "content");
            var contentDirectory = fileSystemProxy.GetDirectoryInfo(contentPath);
            var contentFiles = contentDirectory.EnumerateFiles("*", SearchOption.AllDirectories);
            var contentRelativePaths = contentFiles.Select(f => f.FullName.Substring(contentPath.Length + 1));
            var objectsPath = Path.Combine(modification.RepositoryPath, "objects");

            foreach (var contentRelativePath in contentRelativePaths) {
               LeagueResolutionTableValue resolutionTableValue;
               if (!resolutionTable.TryGetValue(contentRelativePath, out resolutionTableValue)) {
                  logger.Warn($"Not linking {contentRelativePath}: resolution table lookup failed");
                  continue;
               }
               if (resolutionTableValue.Target != LeagueModificationTarget.Client) {
                  logger.Warn($"Not linking {contentRelativePath}: does not target air client.");
                  continue;
               }
               string replacedPath = resolutionTableValue.ResolvedPath;
               string replacementPath = Path.Combine(contentPath, contentRelativePath);
               commandList.Add(commandFactory.CreateFileRedirectionCommand(replacedPath, replacementPath));
            }
         }
         return commandList;
      }

      public CommandList LinkGameModifications(IReadOnlyList<Modification> modifications) {
         var archiveLoader = new RiotArchiveLoader(leagueConfiguration.RadsPath);
         Dictionary<uint, List<KeyValuePair<RiotArchive, SectorCollection>>> archivePairsById = new Dictionary<uint, List<KeyValuePair<RiotArchive, SectorCollection>>>();

         var riotSolution = riotSolutionLoader.Load(leagueConfiguration.RadsPath, RiotProjectType.GameClient);
         var gameProject = riotSolution.ProjectsByType[RiotProjectType.GameClient];
         var gameManifest = gameProject.ReleaseManifest;

         foreach (var modification in modifications) {
            var resolutionTableComponent = modification.GetComponent<LeagueResolutionTableComponent>();
            var resolutionTable = resolutionTableComponent.Table;
            var contentPath = Path.Combine(modification.RepositoryPath, "content");
            var contentDirectory = fileSystemProxy.GetDirectoryInfo(contentPath);
            var contentFiles = contentDirectory.EnumerateFiles("*", SearchOption.AllDirectories);
            var contentRelativePaths = contentFiles.Select(f => f.FullName.Substring(contentPath.Length + 1));
            var objectsPath = Path.Combine(modification.RepositoryPath, "objects");

            foreach (var contentRelativePath in contentRelativePaths) {
               LeagueResolutionTableValue resolutionTableValue;
               if (!resolutionTable.TryGetValue(contentRelativePath, out resolutionTableValue)) {
                  logger.Warn($"Not linking {contentRelativePath}: resolution table lookup failed");
                  continue;
               }
               if (resolutionTableValue.Target != LeagueModificationTarget.Game) {
                  logger.Warn($"Not linking {contentRelativePath}: does not target game client.");
                  continue;
               }
               var manifestEntry = gameManifest.Root.GetRelativeOrNull<ReleaseManifestFileEntry>(resolutionTableValue.ResolvedPath);
               if (manifestEntry == null) {
                  logger.Warn($"Not linking {contentRelativePath}: could not find {resolutionTableValue.ResolvedPath} in manifest.");
                  continue;
               }
               List<KeyValuePair<RiotArchive, SectorCollection>> archivePairs;
               if (!archivePairsById.TryGetValue(manifestEntry.ArchiveId, out archivePairs)) {
                  archivePairs = new List<KeyValuePair<RiotArchive, SectorCollection>>();
                  IReadOnlyList<RiotArchive> archives;
                  if (!archiveLoader.TryLoadArchives(manifestEntry.ArchiveId, out archives)) {
                     logger.Error($"Not linking {contentRelativePath}: could not load archives for {manifestEntry.ArchiveId}!");
                     continue;
                  }
                  archives.ForEach(archive => {
                     var datLength = new FileInfo(archive.DatFilePath).Length;
                     SectorCollection sectorCollection = new SectorCollection();
                     sectorCollection.AssignSector(new SectorRange(0, datLength), new FileSector(archive.DatFilePath, 0, datLength));
                     archivePairs.Add(archive.PairValue(sectorCollection));
                  });
                  archivePairsById.Add(manifestEntry.ArchiveId, archivePairs);
               }

               var rafPath = RAFUtil.FormatPathToRAFPath(resolutionTableValue.ResolvedPath);
               RAFFileListEntry rafEntry = null;
               SectorCollection sectors = null;
               foreach (var archivePair in archivePairs) {
                  rafEntry = archivePair.Key.GetDirectoryFile().GetFileList().GetFileEntryOrNull(rafPath);
                  if (rafEntry != null) {
                     sectors = archivePair.Value;
                     break;
                  }
               }
               if (rafEntry == null) {
                  logger.Warn($"Not linking {contentRelativePath}: could not find {resolutionTableValue.ResolvedPath} in {manifestEntry.ArchiveId} archives.");
                  continue;
               }

               var vfmEndOffset = sectors.EnumerateSectorPairs().Last().Key.endExclusive;

               sectors.DeleteRange(rafEntry.FileOffset, rafEntry.FileOffset + rafEntry.FileSize);

               var objectsFilePath = Path.Combine(objectsPath, contentRelativePath);
               var objectsFileLength = new FileInfo(objectsFilePath).Length;

               rafEntry.FileOffset = (uint)vfmEndOffset;
               rafEntry.FileSize = (uint)objectsFileLength;

               sectors.AssignSector(new SectorRange(vfmEndOffset, vfmEndOffset + objectsFileLength), new FileSector(objectsFilePath, 0, objectsFileLength));

               var originalFileLength = new FileInfo(Path.Combine(contentPath, contentRelativePath)).Length;
               manifestEntry.CompressedSize = (uint)objectsFileLength;
               manifestEntry.DecompressedSize = (uint)originalFileLength;

               logger.Warn("Successfully linked " + resolutionTableValue.ResolvedPath  + " in archive " + manifestEntry.ArchiveId + ".");
            }
         }

         var commandList = new DefaultCommandList();
         var versionStringUtilities = new VersionStringUtilities();
         var tempDir = temporaryFileService.AllocateTemporaryDirectory(TimeSpan.FromMinutes(1));
         logger.Info("Allocated temporary directory " + tempDir);
         foreach (var archivePair in archivePairsById) {
            string versionString = versionStringUtilities.GetVersionString(archivePair.Key);

            foreach (var archiveData in archivePair.Value) {
               // Get archive name (e.g. archive_2.raf or archive_12930813.raf)
               string archiveFileName = archiveData.Key.RAFFilePath.With(x => x.Substring(x.LastIndexOfAny(new[] { '/', '\\' }) + 1));

               // Serialize the VFM
               var vfmSerializer = new SectorCollectionSerializer();
               var vfmFileName = versionString + "/" + archiveFileName + ".dat.vfm";
               var vfmPath = temporaryFileService.AllocateTemporaryFile(tempDir, vfmFileName);
               using (var vfmFileStream = File.Open(vfmPath, FileMode.Create, FileAccess.Write, FileShare.None))
               using (var writer = new BinaryWriter(vfmFileStream)) {
                  vfmSerializer.Serialize(archiveData.Value, writer);
                  commandList.Add(commandFactory.CreateFileRemappingCommand(archiveData.Key.DatFilePath, vfmFileStream.Name));
               }
               logger.Info("Wrote VFM " + vfmFileName + " to " + tempDir);

               // Serialize the RAF
               var rafFileName = versionString + "/" + archiveFileName;
               var rafPath = temporaryFileService.AllocateTemporaryFile(tempDir, rafFileName);
               using (var rafFileStream = File.Open(rafPath, FileMode.Create, FileAccess.Write, FileShare.None))
               using (var writer = new BinaryWriter(rafFileStream)) {
                  writer.Write(archiveData.Key.GetDirectoryFile().GetBytes());
                  commandList.Add(commandFactory.CreateFileRedirectionCommand(archiveData.Key.RAFFilePath, rafFileStream.Name));
               }
               logger.Info("Wrote RAF " + rafFileName + " to " + tempDir);
            }
         }

         // Serialize the Release Manifest
         var manifestPath = temporaryFileService.AllocateTemporaryFile(tempDir, "releasemanifest");
         using (var manifestFileStream = File.Open(manifestPath, FileMode.Create, FileAccess.Write, FileShare.None))
         using (var writer = new BinaryWriter(manifestFileStream)) {
            new ReleaseManifestWriter(gameManifest).Save(writer);
            commandList.Add(commandFactory.CreateFileRedirectionCommand(gameManifest.Path, manifestFileStream.Name));
         }
         logger.Info("Wrote release manifest to " + tempDir);
         return commandList;
      }
   }

   [ModificationComponent(ComponentOrigin.Local, "LEAGUE_RESOLUTION")]
   public class LeagueResolutionTableComponent : FileTableComponentBase<LeagueResolutionTableValue> { }

   public class LeagueResolutionTableValue : IPortableObject {
      private const int kVersion = 0;
      private string resolvedPath;
      private LeagueModificationTarget target;

      public string ResolvedPath { get { return resolvedPath; } set { resolvedPath = value; } }
      public LeagueModificationTarget Target { get { return target; } set { target = value; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, kVersion);
         writer.WriteObject(1, resolvedPath);
         writer.WriteU32(2, (uint)target);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadS32(0);
         resolvedPath = (string)reader.ReadObject(1);
         target = (LeagueModificationTarget)reader.ReadU32(2);

         Trace.Assert(version == kVersion, "Unexpected version " + version);
      }
   }

   public class FileTableComponentBase<TValue> : Component {
      private IDictionary<string, TValue> table = new Dictionary<string, TValue>();
      public event PropertyChangedEventHandler PropertyChanged;

      public IDictionary<string, TValue> Table => table;

      public void Serialize(IPofWriter writer) {
         writer.WriteMap(0, table);
      }

      public void Deserialize(IPofReader reader) {
         table = reader.ReadMap<string, TValue>(0);
      }

      public void NotifyUpdated() {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Table)));
      }
   }
}
