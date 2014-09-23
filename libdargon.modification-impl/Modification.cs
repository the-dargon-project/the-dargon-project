namespace Dargon.Modifications
{
   public class Modification : IModification
   {
      private readonly string rootPath;

      public Modification(string rootPath) {
         this.rootPath = rootPath;
      }

      public void Resolve() { }

      public void Compile() { }
   }
}
