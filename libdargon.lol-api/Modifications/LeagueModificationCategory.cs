using Dargon.PortableObjects;
using ItzWarty;
using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Modifications {
   public sealed class LeagueModificationCategory : IPortableObject
   {
      private static readonly Dictionary<string, LeagueModificationCategory> categorizationsByName = new Dictionary<string, LeagueModificationCategory>();
      private string name;
      private uint value;

      public static readonly LeagueModificationCategory Unknown            = new LeagueModificationCategory("",               0x00);
      public static readonly LeagueModificationCategory Map                = new LeagueModificationCategory("map",            0x01);
      public static readonly LeagueModificationCategory Champion           = new LeagueModificationCategory("champion",       0x02);
      public static readonly LeagueModificationCategory Ward               = new LeagueModificationCategory("ward",           0x04);
      public static readonly LeagueModificationCategory UserInterface      = new LeagueModificationCategory("user-interface", 0x08);
      public static readonly LeagueModificationCategory Other              = new LeagueModificationCategory("other",          0x10);
      public static readonly LeagueModificationCategory All                = new LeagueModificationCategory("all",            0xFF);

      public LeagueModificationCategory() { }

      public LeagueModificationCategory(string name, uint value) {
         this.name = name;
         this.value = value;
         categorizationsByName.Add(name, this);
      }

      public string Name => name;
      public uint Value => value;
      public override string ToString() => name;

      public static LeagueModificationCategory FromString(string s) => categorizationsByName.GetValueOrDefault(s) ?? Unknown;

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, name);
         writer.WriteU32(1, value);
      }

      public void Deserialize(IPofReader reader) {
         name = reader.ReadString(0);
         value = reader.ReadU32(1);
      }
   }
}
