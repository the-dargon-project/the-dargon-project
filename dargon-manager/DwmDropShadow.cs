// via http://stackoverflow.com/questions/3372303/dropshadow-for-wpf-borderless-window/3374544#3374544
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DargonManager
{
   internal class DwmDropShadow
   {
      public struct Margins
      {
         public int leftWidth;
         public int rightWidth;
         public int topHeight;
         public int bottomHeight;
      }

      [DllImport("dwmapi.dll", PreserveSig = true)]
      private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

      [DllImport("dwmapi.dll")]
      private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

      /// <summary>
      /// Drops a standard shadow to a WPF Window, even if the window isborderless. Only works with DWM (Vista and Seven).
      /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
      /// as AllowsTransparency involves a huge permormance issue (hardware acceleration is turned off for all the window).
      /// </summary>
      /// <param name="window">Window to which the shadow will be applied</param>
      public static void DropShadowToWindow(Window window)
      {
         Console.WriteLine("DropShadowToWindow");
         if (!DropShadow(window))
         {
            Console.WriteLine("DropShadowToWindow inner");
            window.SourceInitialized += new EventHandler(window_SourceInitialized);
         }
      }

      private static void window_SourceInitialized(object sender, EventArgs e) //fixed typo
      {
         Window window = (Window)sender;

         Console.WriteLine("DropShadowToWindow Source Initialized");
         DropShadow(window);
         Console.WriteLine("DropShadowToWindow Dropped Shadow");

         window.SourceInitialized -= new EventHandler(window_SourceInitialized);
      }

      /// <summary>
      /// The actual method that makes API calls to drop the shadow to the window
      /// </summary>
      /// <param name="window">Window to which the shadow will be applied</param>
      /// <returns>True if the method succeeded, false if not</returns>
      private static bool DropShadow(Window window)
      {
         try
         {
            Console.WriteLine("At DropShadow");
            WindowInteropHelper helper = new WindowInteropHelper(window);

            int val = 2;
            Console.WriteLine("DwmSetWindowAttribute: " + DwmSetWindowAttribute(helper.Handle, 2, ref val, 4));
            var m = new Margins
            {
               bottomHeight = 0,
               leftWidth = 0,
               rightWidth = 0,
               topHeight = 0
            };

            Console.WriteLine("DwmExtendFrameIntoClientArea: " + DwmExtendFrameIntoClientArea(helper.Handle, ref m));
            return true;
            /*
            int val = 2;
            Console.WriteLine("HWND " + helper.Handle);
            int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

            if (ret1 == 0)
            {
               Console.WriteLine("DwmSetWindowAttribute Returned 0");
               Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
               int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);

               Console.WriteLine("DwmExtendFrameIntoClientArea Returned " + ret2);
               return ret2 == 0;
            }
            else
            {
               Console.WriteLine("Warn: DwmSetWindowAttribute returned " + ret1);
               return false;
            }
             */
         }
         catch (Exception ex)
         {
            Console.WriteLine("Warn: DropShadow threw " + ex.ToString());
            // Probably dwmapi.dll not found (incompatible OS)
            return false;
         }
      }

   }
}