using System;
using System.IO;

namespace Dargon.Patcher
{
   public static class Extensions
   {
      public static Hash160 ReadHash160(this BinaryReader reader) { return new Hash160(reader.ReadBytes(Hash160.Size)); }
      public static void Write(this BinaryWriter writer, Hash160 hash) { writer.Write(hash.GetBytes()); }

      public static IndexEntry ReadIndexEntry(this BinaryReader reader) { return new IndexEntry(reader.ReadUInt64(), reader.ReadHash160(), (IndexEntryFlags)reader.ReadUInt32()); }
      public static void Write(this BinaryWriter writer, IndexEntry entry) { writer.Write(entry.LastModified); writer.Write(entry.RevisionHash); writer.Write((UInt32)entry.Flags); }
   }
}