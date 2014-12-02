using Dargon.PortableObjects;

namespace Dargon.Modifications
{
   public class BuildConfiguration : IBuildConfiguration, IPortableObject
   {
      private bool perTargetBuild = true;
      public bool PerTargetBuild { get { return perTargetBuild; } set { perTargetBuild = value; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteBoolean(0, perTargetBuild);
      }

      public void Deserialize(IPofReader reader) {
         perTargetBuild = reader.ReadBoolean(0);
      }
   }
}