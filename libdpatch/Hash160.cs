using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using ItzWarty;
using Extensions = ItzWarty.Extensions;

namespace Dargon.Patcher
{
   [StructLayout(LayoutKind.Explicit, Size = 20, Pack = 1)]
   public unsafe struct Hash160 : IComparable<Hash160>, IEquatable<Hash160>, IComparable, IFormattable
   {
      private static readonly Hash160 max = new Hash160(Util.Generate<byte>(Size, i => 0xFF));

      public static Hash160 Zero { get { return default(Hash160); } }
      public static Hash160 Max { get { return max; } }

      [FieldOffset(0)] private readonly ulong n0;
      [FieldOffset(8)] private readonly ulong n1;
      [FieldOffset(16)] private readonly uint n2;

      public Hash160(byte[] bytes) 
      { 
         n0 = BitConverter.ToUInt64(bytes, 0);
         n1 = BitConverter.ToUInt64(bytes, 8);
         n2 = BitConverter.ToUInt32(bytes, 16);
      }

      public static int Size { get { return 20; } }

      public bool Equals(Hash160 other) { return n0 == other.n0 && n1 == other.n1 && n2 == other.n2; }
      public override bool Equals(object obj) { return base.Equals(obj); }
      
      public int CompareTo(Hash160 other)
      {
         if (n0 != other.n0) {
            return n0.CompareTo(other.n0);
         } else if (n1 != other.n1) {
            return n1.CompareTo(other.n1);
         } else {
            return n2.CompareTo(other.n2);
         }
      }
      int IComparable.CompareTo(object obj) { return CompareTo((Hash160)obj); }

      public static bool operator ==(Hash160 a, Hash160 b) { return a.Equals(b); }
      public static bool operator !=(Hash160 a, Hash160 b) { return !a.Equals(b); }

      /// <summary>
      /// Formats the value of the current instance using the specified format.
      /// </summary>
      /// <returns>
      /// The value of the current instance in the specified format.
      /// Only supports "x" and "X".
      /// </returns>
      /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation. </param><param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
      public string ToString(string format, IFormatProvider formatProvider = null)
      {
         if (!string.IsNullOrEmpty(format)) {
            var mode = format[0];
            switch (mode)
            {
               case 'x':
               case 'X':
                  return GetBytes().Select(b => b.ToString("x2")).Join("");
            }
         }

         throw new NotImplementedException();
      }


      public override int GetHashCode() { 
         var hash = 13;
         hash = hash * 17 + n0.GetHashCode();
         hash = hash * 17 + n1.GetHashCode();
         hash = hash * 17 * n2.GetHashCode();
         return hash;
      }

      public byte[] GetBytes()
      {
         var result = new byte[20];
         Array.Copy(BitConverter.GetBytes(n0), 0, result, 0, 8);
         Array.Copy(BitConverter.GetBytes(n1), 0, result, 8, 8);
         Array.Copy(BitConverter.GetBytes(n2), 0, result, 16, 4);
         return result;
      }

      public static Hash160 Parse(string s)
      {
         var lut = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
         var bytes = new byte[20];
         for (var i = 0; i < bytes.Length; i++) {
            var upper = (byte)Array.IndexOf(lut, Char.ToLower(s[i * 2]));
            var lower = (byte)Array.IndexOf(lut, Char.ToLower(s[i * 2 + 1]));
            bytes[i] = (byte)(upper * 16 + lower);
         }
         return new Hash160(bytes);
      }
   }
}
