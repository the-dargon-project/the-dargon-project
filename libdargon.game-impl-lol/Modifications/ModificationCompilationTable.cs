using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Patcher;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class ModificationCompilationTable : IDisposable
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const uint MAGIC = 0x524d4446U;

      private readonly string path;
      private readonly Dictionary<string, ModificationCompilationValue> valuesByFileRevisionHash = new Dictionary<string, ModificationCompilationValue>();

      public ModificationCompilationTable(string path)
      {
         this.path = path;

         Load();
      }

      public ModificationCompilationValue this[string internalPath] { get { return valuesByFileRevisionHash[internalPath]; } set { valuesByFileRevisionHash[internalPath] = value; } }

      public bool Contains(string internalPath) { return valuesByFileRevisionHash.ContainsKey(internalPath); }
      public bool Remove(string internalPath) { return valuesByFileRevisionHash.Remove(internalPath); }
      public ModificationCompilationValue GetValueOrNull(string internalPath) { return valuesByFileRevisionHash.GetValueOrDefault(internalPath); }

      private void Load()
      {
         if (!File.Exists(path))
         {
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
                  var fileRevisionHash = reader.ReadHash160();
                  var lastModified = reader.ReadUInt64();
                  var compiledFileHash = reader.ReadHash160();
                  valuesByFileRevisionHash.Add(internalPath, new ModificationCompilationValue(fileRevisionHash, lastModified, compiledFileHash));
               }
            }
         } catch (Exception e) {
            logger.Error("Nonfatal error: Modification Compilation Table likely corrupt.", e);
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
                  writer.Write(kvp.Value.FileRevisionHash);
                  writer.Write(kvp.Value.LastModified);
                  writer.Write(kvp.Value.CompiledFileHash);
               }
            }
            File.WriteAllBytes(path, ms.ToArray());
         }
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose() { Save(); }

      public class ModificationCompilationValue
      {
         private readonly Hash160 fileRevisionHash;
         private readonly ulong lastModified;
         private readonly Hash160 compiledFileHash;

         public ModificationCompilationValue(Hash160 fileRevisionHash, ulong lastModified, Hash160 compiledFileHash)
         {
            this.fileRevisionHash = fileRevisionHash;
            this.lastModified = lastModified;
            this.compiledFileHash = compiledFileHash;
         }

         public Hash160 FileRevisionHash { get { return fileRevisionHash; } }
         public ulong LastModified { get { return lastModified; } }
         public Hash160 CompiledFileHash { get { return compiledFileHash; } }
      }
   }
}
