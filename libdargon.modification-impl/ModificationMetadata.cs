using Dargon.Game;

namespace Dargon.Modifications
{
   internal class ModificationMetadata : IModificationMetadata
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
   }
}