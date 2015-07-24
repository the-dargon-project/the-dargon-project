using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dargon.Manager {
   // http://social.technet.microsoft.com/wiki/contents/articles/23287.trick-to-use-a-resourcedictionary-only-when-in-design-mode.aspx
   public class DesignTimeResourceDictionary : ResourceDictionary {
      /// <summary>
      /// Local field storing info about designtime source.
      /// </summary>
      private string designTimeSource;

      /// <summary>
      /// Gets or sets the design time source.
      /// </summary>
      /// <value>
      /// The design time source.
      /// </value>
      public string DesignTimeSource {
         get {
            return this.designTimeSource;
         }

         set {
            this.designTimeSource = value;
            //            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue) {
                           base.Source = new Uri(designTimeSource);
            //            }
         }
      }

//      /// <summary>
//      /// Gets or sets the uniform resource identifier (URI) to load resources from.
//      /// </summary>
//      /// <returns>The source location of an external resource dictionary. </returns>
//      public new Uri Source {
//         get {
//            throw new Exception("Use DesignTimeSource instead Source!");
//         }
//
//         set {
//            throw new Exception("Use DesignTimeSource instead Source!");
//         }
//      }
   }
}
