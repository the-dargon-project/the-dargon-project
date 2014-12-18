using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using Dargon.IO.RADS.Manifest;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Patcher;
using Dargon.VirtualFileMapping;
using ItzWarty;
using LibGit2Sharp;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueGameModificationLinkerServiceImpl : LeagueGameModificationLinkerService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly TemporaryFileService temporaryFileService;
      private readonly RadsService radsService;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;

      public LeagueGameModificationLinkerServiceImpl(TemporaryFileService temporaryFileService, RadsService radsService, LeagueModificationRepositoryService leagueModificationRepositoryService)
      {
         this.temporaryFileService = temporaryFileService;
         this.radsService = radsService;
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
      }

      public void LinkModificationObjects() 
      {
         var manifest = radsService.GetReleaseManifestUnsafe(RiotProjectType.GameClient);
         var archiveDataById = new Dictionary<uint, List<ArchiveData>>();
         foreach (var modification in leagueModificationRepositoryService.EnumerateModifications()) {
            var modificationMetadata = modification.Metadata;
            var contentPath = modificationMetadata.ContentPath.Trim('/', '\\');

            logger.Info("Linking files of modification {0}".F(modificationMetadata.Name));
            logger.Info("  Content Path: {0}".F(contentPath));

            var gitRepository = new Repository(modification.RepositoryPath);
            var dpmRepository = new LocalRepository(modification.RepositoryPath);
            using (dpmRepository.TakeLock()) {
               var compilationMetadataFilePath = dpmRepository.GetMetadataFilePath(LeagueModificationObjectCompilerServiceImpl.COMPILATION_METADATA_FILE_NAME);
               var resolutionMetadataFilePath = dpmRepository.GetMetadataFilePath(LeagueModificationResolutionServiceImpl.RESOLUTION_METADATA_FILE_NAME);
               using (var compilationMetadata = new ModificationCompilationTable(compilationMetadataFilePath))
               using (var resolutionMetadata = new ModificationResolutionTable(resolutionMetadataFilePath)) {
                  foreach (var file in gitRepository.Index) {
                     if (!file.Path.StartsWith(contentPath, StringComparison.OrdinalIgnoreCase)) {
                        logger.Info("Ignoring \"" + file.Path + "\" as it is not in the content directory.");
                        continue;
                     }

                     string internalPath = file.Path;
                     Hash160 fileHash = Hash160.Parse(file.Id.Sha);
                     var resolutionEntry = resolutionMetadata.GetValueOrNull(internalPath);
                     var compilationEntry = compilationMetadata.GetValueOrNull(internalPath);

                     if (resolutionEntry == null || compilationEntry == null) {
                        logger.Info("Not linking " + internalPath + " as it is either not resolved or not compiled.");
                        continue;
                     }

                     if (resolutionEntry.ResolvedPath == null || !resolutionEntry.Target.HasFlag(ModificationTargetType.Game)) {
                        logger.Info("Not linking " + internalPath + " as it is unresolved or does not target the game.");
                        continue;
                     }

                     if (compilationEntry.CompiledFileHash.Equals(Hash160.Zero)) {
                        logger.Info("Not linking " + internalPath + " as its compiled file hash is unset.");
                        continue;
                     }

                     var sourcePath = dpmRepository.GetAbsolutePath(internalPath);
                     var objectPath = dpmRepository.GetObjectPath(compilationEntry.CompiledFileHash);
                     var sourceLength = new FileInfo(sourcePath).Length;
                     var objectLength = new FileInfo(objectPath).Length;

                     // Ensure the Manifest entry still exists
                     var manifestEntry = manifest.Root.GetRelativeOrNull<ReleaseManifestFileEntry>(resolutionEntry.ResolvedPath);
                     if (manifestEntry == null) {
                        logger.Warn("Manifest Entry for " + resolutionEntry.ResolvedPath + " not found!?");
                        continue;
                     }

                     // Get RAF Archive Data for the given archive id
                     var archiveDatas = archiveDataById.GetValueOrDefault(manifestEntry.ArchiveId);
                     if (archiveDatas == null) {
                        logger.Warn("Loading archive {0}.".F(manifestEntry.ArchiveId));
                        archiveDataById.Add(manifestEntry.ArchiveId, archiveDatas = LoadArchiveDatas(manifestEntry));
                     }

                     // Find the archive which contains our raf entry
                     ArchiveData archiveData = null;
                     RAFFileListEntry rafEntry = null;
                     foreach (var candidate in archiveDatas) {
                        rafEntry = candidate.Archive.GetDirectoryFile().GetFileList().GetFileEntryOrNull(RAFUtil.FormatPathToRAFPath(resolutionEntry.ResolvedPath));
                        if (rafEntry != null) {
                           archiveData = candidate;
                           break;
                        }
                     }
                     if (rafEntry == null) {
                        logger.Warn("RAF Entry for " + resolutionEntry.ResolvedPath + " in archive " + manifestEntry.ArchiveId + " not found!?");
                        continue;
                     }

                     // Update Release Manifest:
                     manifestEntry.CompressedSize = (uint)objectLength;
                     manifestEntry.DecompressedSize = (uint)sourceLength;
                     
                     // Update RAF Data File's Virtual File Map
                     var dataStartInclusive = archiveData.NextOffset;
                     var dataEndExclusive = dataStartInclusive + objectLength;
                     archiveData.Sectors.AssignSector(new SectorRange(dataStartInclusive, dataEndExclusive), new FileSector(objectPath, 0, objectLength));
                     archiveData.NextOffset = dataEndExclusive;
                     
                     // Update RAF File
                     rafEntry.FileOffset = (uint)dataStartInclusive;
                     rafEntry.FileSize = (uint)objectLength;
                     logger.Warn("Successfully linked RAF Entry for " + resolutionEntry.ResolvedPath + " in archive " + manifestEntry.ArchiveId + ".");
                  }
               }
            }
         }

         var versionStringUtilities = new VersionStringUtilities();
         var tempDir = temporaryFileService.AllocateTemporaryDirectory(DateTime.Now + TimeSpan.FromHours(24));
         logger.Info("Allocated temporary directory " + tempDir);
         foreach (var archiveDataKvp in archiveDataById) {
            string versionString = versionStringUtilities.GetVersionString(archiveDataKvp.Key);

            foreach (var archiveData in archiveDataKvp.Value) {
               // Get archive name (e.g. archive_2.raf or archive_12930813.raf)
               string archiveFileName = archiveData.Archive.RAFFilePath.With(x => x.Substring(x.LastIndexOfAny(new[] { '/', '\\' }) + 1));

               // Serialize the VFM
               var vfmSerializer = new SectorCollectionSerializer();
               var vfmFileName = versionString + "/" + archiveFileName + ".dat.vfm";
               using (var vfmFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, vfmFileName))
               using (var writer = new BinaryWriter(vfmFileStream)) {
                  vfmSerializer.Serialize(archiveData.Sectors, writer);
               }
               logger.Info("Wrote VFM " + vfmFileName + " to " + tempDir);

               // Serialize the RAF
               var rafFileName = versionString + "/" + archiveFileName;
               using (var rafFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, rafFileName))
               using (var writer = new BinaryWriter(rafFileStream)) {
                  writer.Write(archiveData.Archive.GetDirectoryFile().GetBytes());
               }
               logger.Info("Wrote RAF " + rafFileName + " to " + tempDir);
            }
         }

         // Serialize the Release Manifest
         using (var manifestFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, "releasemanifest")) 
         using (var writer = new BinaryWriter(manifestFileStream)) {
            new ReleaseManifestWriter(manifest).Save(writer);  
         }
         logger.Info("Wrote release manifest to " + tempDir);
      }

      private List<ArchiveData> LoadArchiveDatas(ReleaseManifestFileEntry manifestEntry) {
         var archives = radsService.GetArchivesUnsafe(manifestEntry.ArchiveId);
         var archiveDatas = new List<ArchiveData>();
         foreach (var archive in archives) {
            var datLength = new FileInfo(archive.DatFilePath).Length;
            var sectors = new SectorCollection();
            sectors.AssignSector(new SectorRange(0, datLength), new FileSector(archive.DatFilePath, 0, datLength));
            var archiveData = new ArchiveData(archive, sectors, datLength);
            archiveDatas.Add(archiveData);
         }
         return archiveDatas;
      }

      private class ArchiveData
      {
         private readonly RiotArchive archive;
         private readonly SectorCollection sectors;
         private long nextOffset;

         public ArchiveData(RiotArchive archive, SectorCollection sectors, long nextOffset)
         {
            this.archive = archive;
            this.sectors = sectors;
            this.nextOffset = nextOffset;
         }

         public RiotArchive Archive { get { return archive; } }
         public SectorCollection Sectors { get { return sectors; } }
         public long NextOffset { get { return nextOffset; } set { nextOffset = value; } }
      }
   }
}
