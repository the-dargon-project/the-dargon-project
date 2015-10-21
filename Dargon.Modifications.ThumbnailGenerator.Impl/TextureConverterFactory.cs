using System.Windows.Forms;
using SharpDX.Direct3D9;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class TextureConverterFactory {
      public TextureConverter Create() {
         var presentParameters = new PresentParameters {
            BackBufferCount = 2, // 1
            SwapEffect = SwapEffect.Discard,
            Windowed = true,
         };
         var direct3d = new Direct3D();
         var panel = new Panel();
         var device = new Device(direct3d, 0, DeviceType.Hardware, panel.Handle, CreateFlags.SoftwareVertexProcessing, presentParameters);
         return new TextureConverter(direct3d, panel, device);
      }
   }
}