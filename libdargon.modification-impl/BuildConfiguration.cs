namespace Dargon.Modifications
{
   public class BuildConfiguration : IBuildConfiguration
   {
      private bool perTargetBuild = true;
      public bool PerTargetBuild { get { return perTargetBuild; } set { perTargetBuild = value; } }
   }
}