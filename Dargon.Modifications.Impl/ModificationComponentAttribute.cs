using System;

namespace Dargon.Modifications {
   public class ModificationComponentAttribute : Attribute {
      private readonly ComponentOrigin origin;
      private readonly string fileName;

      public ModificationComponentAttribute(ComponentOrigin origin, string fileName) {
         this.origin = origin;
         this.fileName = fileName;
      }

      public ComponentOrigin Origin => origin;
      public string FileName => fileName;
   }
}