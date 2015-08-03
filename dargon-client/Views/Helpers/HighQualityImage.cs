using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dargon.Client.Views.Helpers {
   public class HighQualityImage : Image {
      protected override void OnRender(DrawingContext dc) {
         this.VisualBitmapScalingMode = BitmapScalingMode.Fant;
         base.OnRender(dc);
      }
   }
}
