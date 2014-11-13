namespace Dargon.Modifications
{
   public interface IBuildConfigurationLoader
   {
      bool TryLoad(string path, out IBuildConfiguration result);
   }
}