using System.Linq;
using Dargon.Game;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Modifications
{
   public class ModificationMetadata : IModificationMetadata, IPortableObject
   {
      private string name = "";
      private string[] authors = { };
      private string version = "";
      private GameType[] targets = { };
      private string website = "";
      private string toggleUrl = "";
      private string contentPath = "content";

      public string Name { get { return name; } set { name = value; } }
      public string[] Authors { get { return authors; } set { authors = value; } }
      public string Version { get { return version; } set { version = value; } }
      public GameType[] Targets { get { return targets; } set { targets = value; } }
      public string Website { get { return website; } set { website = value; } }
      public string ToggleUrl { get { return toggleUrl; } set { toggleUrl = value; } }
      public string ContentPath { get { return contentPath; } set { contentPath = value; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, name);
         writer.WriteCollection(1, authors);
         writer.WriteString(2, version);
         writer.WriteCollection(3, targets.Select(t => t.Name));
         writer.WriteString(4, website);
         writer.WriteString(5, toggleUrl);
         writer.WriteString(6, contentPath);
      }

      public void Deserialize(IPofReader reader) {
         name = reader.ReadString(0);
         authors = reader.ReadArray<string>(1);
         version = reader.ReadString(2);
         var targetNames = reader.ReadArray<string>(3);
         targets = Util.Generate(targetNames.Length, i => GameType.FromString(targetNames[i]));
         website = reader.ReadString(4);
         toggleUrl = reader.ReadString(5);
         contentPath = reader.ReadString(6);
         
      }
   }
}