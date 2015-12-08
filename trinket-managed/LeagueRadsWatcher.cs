using Dargon.RADS;
using Dargon.RADS.Archives;
using Dargon.RADS.Manifest;
using Dargon.Trinkets.Hosted.Hooks;
using ItzWarty.Collections;
using System;
using System.Diagnostics;
using System.IO;
using SCG = System.Collections.Generic;

namespace Dargon.Trinkets.Hosted {
   public delegate void LeagueRadsEntryRead(LeagueRadsWatcher sender, RafEntry entry);

   public class LeagueRadsWatcher {
      private readonly SCG.Dictionary<TrinketFileIdentifier, RiotArchive> rafDatIdentifierToArchive = new SCG.Dictionary<TrinketFileIdentifier, RiotArchive>();
      private readonly ConcurrentDictionary<IntPtr, RiotArchive> rafArchivesByDatHandle = new ConcurrentDictionary<IntPtr, RiotArchive>();
      private readonly TrinketIoUtilities trinketIoUtilities;
      private readonly FileSystemHookEventBus fileSystemHookEventBus;
      private readonly LeagueTrinketConfiguration leagueTrinketConfiguration;
      private readonly RiotProject gameProject;

      public event LeagueRadsEntryRead RadsEntryRead;

      public LeagueRadsWatcher(
         TrinketIoUtilities trinketIoUtilities, 
         FileSystemHookEventBus fileSystemHookEventBus,
         LeagueTrinketConfiguration leagueTrinketConfiguration, 
         RiotProject gameProject
         ) {
         this.trinketIoUtilities = trinketIoUtilities;
         this.fileSystemHookEventBus = fileSystemHookEventBus;
         this.leagueTrinketConfiguration = leagueTrinketConfiguration;
         this.gameProject = gameProject;
      }

      public void Initialize() {
         var riotArchiveCollectionLoader = RiotArchiveCollectionLoader.FromRadsPath(leagueTrinketConfiguration.RadsPath);
         var requiredArchiveIds = gameProject.GetRequiredArchiveIds();
         foreach (var archiveId in requiredArchiveIds) {
            if (archiveId == 0) continue;

            var archiveCollection = riotArchiveCollectionLoader.LoadArchives(archiveId);
            foreach (var archive in archiveCollection) {
               var datFileIdentifier = trinketIoUtilities.GetFileIdentifier(archive.Path + ".dat");
               rafDatIdentifierToArchive.Add(datFileIdentifier, archive);
            }
         }

         fileSystemHookEventBus.CreateFilePost += HandleCreateFilePost;
         fileSystemHookEventBus.ReadFilePre += HandleReadFilePre;
         fileSystemHookEventBus.CloseHandlePre += HandleCloseHandlePre;
      }

      private void HandleCreateFilePost(FileSystemHookEventBus sender, CreateFilePostEventArgs e) {
         var identifier = trinketIoUtilities.GetFileIdentifier(e.ReturnValue);
         RiotArchive archive;
         if (rafDatIdentifierToArchive.TryGetValue(identifier, out archive)) {
            rafArchivesByDatHandle.AddOrUpdate(
               e.ReturnValue,
               add => archive,
               (update, existing) => {
                  Debugger.Break();
                  return existing;
               });
         }
      }

      private void HandleReadFilePre(FileSystemHookEventBus sender, ReadFilePreEventArgs e) {
         RiotArchive archive;
         if (rafArchivesByDatHandle.TryGetValue(e.Arguments.Handle, out archive)) {
            var offset = e.Arguments.FileOffset;
            var entry = archive.GetEntryOfOffset((uint)offset);
            if (entry != null) {
               RadsEntryRead?.Invoke(this, entry);
            }
         }
      }

      private void HandleCloseHandlePre(FileSystemHookEventBus sender, CloseHandlePreEventArgs e) {
         RiotArchive archive;
         rafArchivesByDatHandle.TryRemove(e.Arguments.Handle, out archive);
      }
   }

   public static class LeagueRadsUtilities {
      public static IReadOnlySet<uint> GetRequiredArchiveIds(this RiotProject project) {
         var releaseManifest = project.ReleaseManifest;
         var requiredArchiveIds = new ItzWarty.Collections.HashSet<uint>();
         foreach (var file in releaseManifest.Files) {
            var entryDescriptor = file.GetComponentOrNull<ReleaseManifestFileEntryDescriptor>();
            requiredArchiveIds.Add(entryDescriptor.ArchiveId);
         }
         return requiredArchiveIds;
      }

      public static RafEntry GetEntryOfOffset(this RiotArchive archive, uint offset) {
         foreach (var entry in archive.Entries) {
            if (entry.DataOffset <= offset) {
               var entryEndExclusive = entry.DataOffset + entry.DataLength;
               if (offset < entryEndExclusive) {
                  return entry;
               }
            }
         }
         return null;
      }
   }

   public struct TrinketFileIdentifier {
      public readonly ulong fileIndex;
      public readonly uint volumeSerialNumber;

      public TrinketFileIdentifier(ulong fileIndex, uint volumeSerialNumber) {
         this.fileIndex = fileIndex;
         this.volumeSerialNumber = volumeSerialNumber;
      }

      public override bool Equals(object obj) {
         if (obj is TrinketFileIdentifier) {
            var other = (TrinketFileIdentifier)obj;
            return fileIndex == other.fileIndex &&
                   volumeSerialNumber == other.volumeSerialNumber;
         } else {
            return false;
         }
      }

      public override int GetHashCode() {
         var hash = 13;
         hash += fileIndex.GetHashCode() * 17;
         hash += volumeSerialNumber.GetHashCode() * 17;
         return hash;
      }
   }

   public class TrinketIoUtilities {
      public TrinketFileIdentifier GetFileIdentifier(IntPtr fileHandle) {
         WinAPI.BY_HANDLE_FILE_INFORMATION fileInfo;
         WinAPI.GetFileInformationByHandle(fileHandle, out fileInfo);

         var fileIndex = BuildUInt64(fileInfo.FileIndexHigh, fileInfo.FileIndexLow);
         return new TrinketFileIdentifier(fileIndex, fileInfo.VolumeSerialNumber);
      }

      public TrinketFileIdentifier GetFileIdentifier(string filePath) {
         using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
            return GetFileIdentifier(fs.Handle);
         }
      }

      private ulong BuildUInt64(uint high, uint low) => ((ulong)high << 32) | low;
   }
}
