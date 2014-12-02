using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DargonManager
{
   /// <summary>
   /// Interaction logic for DummyShadowedWindow.xaml
   /// </summary>
   public partial class DummyShadowedWindow : Window
   {
      public DummyShadowedWindow()
      {
         InitializeComponent();
         SourceInitialized += HandleSourceInitialized;
      }

      //-------------------------------------------------------------------------------------------
      // Drop Shadow
      //-------------------------------------------------------------------------------------------
      private void HandleSourceInitialized(object sender, EventArgs e)
      {
         //m_windowHandleSource = (HwndSource)PresentationSource.FromVisual(this);
         ShellWindow_SourceInitialized(sender, e);
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct Margins
      {
         public int leftWidth;
         public int rightWidth;
         public int topHeight;
         public int bottomHeight;
      }

      [DllImport("dwmapi.dll", PreserveSig = true)]
      public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

      [DllImport("dwmapi.dll")]
      public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

      void ShellWindow_SourceInitialized(object sender, EventArgs e)
      {
         var helper = new WindowInteropHelper(this);
         //var helper = m_windowHandleSource;

         int val = 2;
         DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

         var m = new Margins { bottomHeight = -1, leftWidth = -1, rightWidth = -1, topHeight = -1 };
         DwmExtendFrameIntoClientArea(helper.Handle, ref m);
      }
   }
}
