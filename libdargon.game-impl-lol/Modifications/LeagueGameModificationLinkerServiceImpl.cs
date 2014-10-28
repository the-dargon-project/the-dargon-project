using System;
using System.IO;
using Dargon.Daemon;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Patcher;
using Dargon.VirtualFileMapping;
using ItzWarty;
using ItzWarty.Collections;
using NLog;
using System.Collections.Generic;

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
         var archiveDataById = new Dictionary<uint, ArchiveData>();
         foreach (var modification in leagueModificationRepositoryService.EnumerateModifications()) {
            var repository = new LocalRepository(modification.RootPath);
            using (repository.TakeLock()) {
               var compilationMetadataFilePath = repository.GetMetadataFilePath(LeagueModificationObjectCompilerServiceImpl.COMPILATION_METADATA_FILE_NAME);
               var resolutionMetadataFilePath = repository.GetMetadataFilePath(LeagueModificationResolutionServiceImpl.RESOLUTION_METADATA_FILE_NAME);
               using (var compilationMetadata = new ModificationCompilationTable(compilationMetadataFilePath))
               using (var resolutionMetadata = new ModificationResolutionTable(resolutionMetadataFilePath)) {
                  foreach (var indexEntry in repository.EnumerateIndexEntries()) {
                     if (indexEntry.Value.Flags.HasFlag(IndexEntryFlags.Directory)) {
                        continue;
                     }

                     string internalPath = indexEntry.Key;
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

                     var sourcePath = repository.GetAbsolutePath(indexEntry.Key);
                     var objectPath = repository.GetObjectPath(compilationEntry.CompiledFileHash);
                     var sourceLength = new FileInfo(sourcePath).Length;
                     var objectLength = new FileInfo(objectPath).Length;

                     // Ensure the Manifest entry still exists
                     var manifestEntry = manifest.Root.GetRelativeOrNull<ReleaseManifestFileEntry>(resolutionEntry.ResolvedPath);
                     if (manifestEntry == null) {
                        logger.Warn("Manifest Entry for " + resolutionEntry.ResolvedPath + " not found!?");
                        continue;
                     }

                     // Get RAF Archive Data for the given archive id
                     var archiveData = archiveDataById.GetValueOrDefault(manifestEntry.ArchiveId);
                     if (archiveData == null) {
                        var archive = radsService.GetArchiveUnsafe(manifestEntry.ArchiveId);
                        var datLength = new FileInfo(archive.DatFilePath).Length;
                        var sectors = new SectorCollection();
                        sectors.AssignSector(new SectorRange(0, datLength), new FileSector(archive.DatFilePath, 0, datLength));
                        archiveData = new ArchiveData(archive, sectors, datLength);
                        archiveDataById.Add(manifestEntry.ArchiveId, archiveData);
                     }
                     
                     // Ensure the RAF Entry exists (release manifest can lie about what's in RAFs)
                     var rafEntry = archiveData.Archive.GetDirectoryFile().GetFileList().GetFileEntryOrNull(RAFUtil.FormatPathToRAFPath(resolutionEntry.ResolvedPath));
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
                  }
               }
            }
         }

         var versionStringUtilities = new VersionStringUtilities();
         var tempDir = temporaryFileService.AllocateTemporaryDirectory(DateTime.Now + TimeSpan.FromHours(24));
         foreach (var archiveDataKvp in archiveDataById) {
            string versionString = versionStringUtilities.GetVersionString(archiveDataKvp.Key);

            // Serialize the VFM
            var vfmSerializer = new SectorCollectionSerializer();
            var vfmFileName = versionString + ".raf.dat.vfm";
            using (var vfmFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, vfmFileName))
            using (var writer = new BinaryWriter(vfmFileStream)) {
               vfmSerializer.Serialize(archiveDataKvp.Value.Sectors, writer);
            }
            logger.Info("Wrote VFM " + vfmFileName + " to " + tempDir);

            // Serialize the RAF
            var rafFileName = versionString + ".raf";
            using (var rafFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, rafFileName)) 
            using (var writer = new BinaryWriter(rafFileStream)) {
               writer.Write(archiveDataKvp.Value.Archive.GetDirectoryFile().GetBytes());
            }
            logger.Info("Wrote RAF " + rafFileName + " to " + tempDir);
         }

         // Serialize the Release Manifest
         using (var manifestFileStream = temporaryFileService.AllocateTemporaryFile(tempDir, "releasemanifest")) 
         using (var writer = new BinaryWriter(manifestFileStream)) {
            new ReleaseManifestWriter(manifest).Save(writer);  
         }
         logger.Info("Wrote release manifest to " + tempDir);
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
