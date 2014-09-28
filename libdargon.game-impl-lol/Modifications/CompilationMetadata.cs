using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Patcher;
using ItzWarty;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class CompilationMetadata : IDisposable
   {
      private const uint MAGIC = 0x524d4446U;

      private readonly string path;
      private readonly Dictionary<Hash160, CompilationMetadataValue> valuesByFileRevisionHash = new Dictionary<Hash160, CompilationMetadataValue>();

      public CompilationMetadata(string path)
      {
         this.path = path;

         Load();
      }

      public CompilationMetadataValue this[Hash160 fileRevisionHash] { get { return valuesByFileRevisionHash[fileRevisionHash]; } set { valuesByFileRevisionHash[fileRevisionHash] = value; } }

      public bool Contains(Hash160 fileRevisionHash) { return valuesByFileRevisionHash.ContainsKey(fileRevisionHash); }
      public bool Remove(Hash160 fileRevisionHash) { return valuesByFileRevisionHash.Remove(fileRevisionHash); }
      public CompilationMetadataValue GetValueOrNull(Hash160 fileRevisionHash) { return valuesByFileRevisionHash.GetValueOrDefault(fileRevisionHash); }

      private void Load()
      {
         if (!File.Exists(path))
         {
            Save();
         }

         using (var ms = new MemoryStream(File.ReadAllBytes(path)))
         using (var reader = new BinaryReader(ms))
         {
            var magic = reader.ReadUInt32();
            if (magic != MAGIC)
            {
               throw new InvalidOperationException("RMDF Magic Mismatch - Expected " + MAGIC + " but found " + magic);
            }

            var count = reader.ReadUInt32();
            for (uint i = 0; i < count; i++)
            {
               var fileRevisionHash = reader.ReadHash160();
               var lastModified = reader.ReadUInt64();
               var compiledFileHash = reader.ReadHash160();
               valuesByFileRevisionHash.Add(internalPath, new CompilationMetadataValue(resolvedpath, fileRevision, targetType));
            }
         }
      }

      private void Save()
      {
         using (var ms = new MemoryStream())
         {
            using (var writer = new BinaryWriter(ms))
            {
               writer.Write((uint)MAGIC);
               writer.Write((uint)valuesByFileRevisionHash.Count);

               foreach (var kvp in valuesByFileRevisionHash)
               {
                  writer.WriteNullTerminatedString(kvp.Key);
                  writer.WriteNullTerminatedString(kvp.Value.ResolvedPath);
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

      public class CompilationMetadataValue
      {
         private readonly ulong lastModified;
         private readonly Hash160 compiledFileHash;

         public CompilationMetadataValue(ulong lastModified, Hash160 compiledFileHash)
         {
            this.lastModified = lastModified;
            this.compiledFileHash = compiledFileHash;
         }

         public ulong LastModified { get { return lastModified; } }
         public Hash160 CompiledFileHash { get { return compiledFileHash; } }
      }
   }
}
