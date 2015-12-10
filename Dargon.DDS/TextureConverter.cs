using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Rectangle = System.Drawing.Rectangle;

namespace Dargon.DDS {
   public class TextureConverter : IDisposable {
      private readonly Direct3D direct3d;
      private readonly Panel panel;
      private readonly Device device;

      public TextureConverter(Direct3D direct3d, Panel panel, Device device) {
         this.direct3d = direct3d;
         this.panel = panel;
         this.device = device;
      }

      public Bitmap ConvertToBitmap(string filePath) {
         using (var texture = Texture.FromFile(device, filePath)) {
            var surfaceDescription = texture.GetLevelDescription(0);
            var textureWidth = surfaceDescription.Width;
            var textureHeight = surfaceDescription.Height;

            using (var renderTarget = Surface.CreateRenderTarget(device, textureWidth, textureHeight, Format.A8R8G8B8, MultisampleType.None, 0, true)) {
               var oldBackBuffer = device.GetRenderTarget(0);

               device.SetRenderTarget(0, renderTarget);
               using (var sprite = new Sprite(device)) {
                  device.Clear(ClearFlags.Target, Color.Transparent, 0, 1);
                  device.BeginScene();
                  sprite.Begin(SpriteFlags.AlphaBlend);
                  sprite.Draw(texture, new ColorBGRA(Vector4.One));
                  sprite.End();
                  device.EndScene();
               }
               device.SetRenderTarget(0, oldBackBuffer);

               var renderTargetData = renderTarget.LockRectangle(LockFlags.ReadOnly);
               var resultBitmap = new Bitmap(textureWidth, textureHeight, PixelFormat.Format32bppArgb);
               var resultData = resultBitmap.LockBits(new Rectangle(0, 0, textureWidth, textureHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
               for (var y = 0; y < textureHeight; y++) {
                  Utilities.CopyMemory(resultData.Scan0 + y * resultData.Stride, renderTargetData.DataPointer + y * renderTargetData.Pitch, textureWidth * 4);
               }
               resultBitmap.UnlockBits(resultData);
               renderTarget.UnlockRectangle();
               return resultBitmap;
            }
         }
      }

      public void ConvertAndSaveToTexture(string inputImagePath, string outputDdsPath) {
         using (var texture = Texture.FromFile(device, inputImagePath)) {
            BaseTexture.ToFile(texture, outputDdsPath, ImageFileFormat.Dds);
         }
      }

      public void Dispose() {
         device.Dispose();
         panel.Dispose();
         direct3d.Dispose();
      }
   }
}
