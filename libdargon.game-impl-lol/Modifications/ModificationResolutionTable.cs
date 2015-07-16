using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Dargon.Patcher;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class ModificationResolutionTable : IDisposable
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const uint MAGIC = 0x524d4446U;

      private readonly string path;
      private readonly Dictionary<string, ResolutionMetadataValue> valuesByInternalPath = new Dictionary<string, ResolutionMetadataValue>(); 

      public ModificationResolutionTable(string path)
      {
         this.path = path;

         Load();
      }

      public ResolutionMetadataValue this[string internalPath] { get { return valuesByInternalPath[internalPath]; } set { valuesByInternalPath[internalPath] = value; } }

      public bool Contains(string internalPath) { return valuesByInternalPath.ContainsKey(internalPath); }
      public bool Remove(string internalPath) { return valuesByInternalPath.Remove(internalPath); }
      public ResolutionMetadataValue GetValueOrNull(string internalPath) { return valuesByInternalPath.GetValueOrDefault(internalPath); }

      private void Load()
      {
         if (!File.Exists(path)) {
            Save();
         }

         try {
            using (var ms = new MemoryStream(File.ReadAllBytes(path)))
            using (var reader = new BinaryReader(ms)) {
               var magic = reader.ReadUInt32();
               if (magic != MAGIC) {
                  throw new InvalidOperationException("RMDF Magic Mismatch - Expected " + MAGIC + " but found " + magic);
               }

               var count = reader.ReadUInt32();
               for (uint i = 0; i < count; i++) {
                  var internalPath = reader.ReadNullTerminatedString();
                  var resolvedPath = reader.ReadNullTerminatedString();
                  if (string.IsNullOrEmpty(resolvedPath))
                     resolvedPath = null;
                  var fileRevision = reader.ReadHash160();
                  var targetType = (ModificationTargetType)reader.ReadUInt32();
                  valuesByInternalPath.Add(internalPath, new ResolutionMetadataValue(resolvedPath, fileRevision, targetType));
               }
            }
         } catch (Exception e) {
            logger.Error("Nonfatal error: Modification Resolution Table likely corrupt.", e);
         }
      }

      private void Save() 
      {
         using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
               writer.Write((uint)MAGIC);
               writer.Write((uint)valuesByInternalPath.Count);

               foreach (var kvp in valuesByInternalPath) {
                  writer.WriteNullTerminatedString(kvp.Key);
                  writer.WriteNullTerminatedString(kvp.Value.ResolvedPath ?? "");
                  writer.Write(kvp.Value.FileRevision);
                  writer.Write((uint)kvp.Value.Target);
               }
            }
            File.WriteAllBytes(path, ms.ToArray());
         }
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose() { Save(); }

      public class ResolutionMetadataValue
      {
         private string resolvedPath;
         private Hash160 fileRevision;
         private ModificationTargetType target;

         public ResolutionMetadataValue(string resolvedPath, Hash160 fileRevision, ModificationTargetType target)
         {
            this.resolvedPath = resolvedPath;
            this.fileRevision = fileRevision;
            this.target = target;
         }

         public string ResolvedPath { get { return resolvedPath; } set { resolvedPath = value; } }
         public Hash160 FileRevision { get { return fileRevision; } set { fileRevision = value; } }
         public ModificationTargetType Target { get { return target; } set { target = value; } }
      }
   }
}