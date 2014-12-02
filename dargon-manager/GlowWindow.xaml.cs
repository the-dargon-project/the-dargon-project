using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using DargonManager.Annotations;

namespace DargonManager
{
   public class GlowWindowModel : INotifyPropertyChanged
   {
      private bool m_isEmbiggened = false;
      public bool IsEmbiggened { get { return m_isEmbiggened; } set { m_isEmbiggened = value; OnPropertyChanged(); } }

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }

   /// <summary>
   /// Interaction logic for GlowWindow.xaml
   /// </summary>
   public partial class GlowWindow : Window
   {
      private readonly IntPtr m_parentHandle;
      private readonly Window m_parentWindow;
      private HwndSource m_windowHandleSource;

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

      public GlowWindowModel Model { get { return m_model; } }
      private readonly GlowWindowModel m_model = new GlowWindowModel();

      public GlowWindow(IntPtr parentHandle, Window parentWindow)
      {
         m_parentHandle = parentHandle;
         m_parentWindow = parentWindow;
         InitializeComponent();
         MouseMove += GlowWindow_MouseMove;
         PreviewMouseDown += GlowWindow_PreviewMouseDown;
         this.DataContext = m_model;
         m_model.PropertyChanged += (s, e) => Update(false);
      }

      public void Update(bool isActivatedEvent = false)
      {
         //Console.WriteLine(IsFocused + " " + IsActive + " " + m_glowWindow.IsActive + " " + WindowState);
         if (m_parentWindow.WindowState == WindowState.Maximized || 
             (!m_parentWindow.IsActive && !this.IsActive && !m_model.IsEmbiggened))
         {
            Visibility = Visibility.Hidden;
         }
         else
         {
            int margin = m_model.IsEmbiggened ? 25 : 25;

            Left = m_parentWindow.Left - margin;
            Top = m_parentWindow.Top - margin;
            Width = m_parentWindow.Width + margin * 2;
            Height = m_parentWindow.Height + margin * 2;
            Visibility = Visibility.Visible;

            if (isActivatedEvent)
               Activate();
         }
      }

      private int GetResizeXDirection(Point relativePositionToInnerGrid)
      {
         int margin = 25;
         int xDirection = 0;
         if (relativePositionToInnerGrid.X < margin)
            xDirection = -1;
         else if (relativePositionToInnerGrid.X >= this.ActualWidth - margin)
            xDirection = 1;
         return xDirection;
      }

      private int GetResizeYDirection(Point relativePositionToInnerGrid)
      {
         int margin = 25;
         int yDirection = 0;
         if (relativePositionToInnerGrid.Y < margin)
            yDirection = -1;
         else if (relativePositionToInnerGrid.Y >= this.ActualHeight - margin)
            yDirection = 1;
         return yDirection;
      }

      void GlowWindow_MouseMove(object sender, MouseEventArgs e)
      {
         var relativePositionToInnerGrid = e.GetPosition(this);
         int xDirection = GetResizeXDirection(relativePositionToInnerGrid);
         int yDirection = GetResizeYDirection(relativePositionToInnerGrid);

         var directionMap = new Cursor[,] {
            {Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW},
            {Cursors.SizeWE,   Cursors.Arrow,  Cursors.SizeWE  },
            {Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE}
         };

         Cursor = directionMap[yDirection + 1, xDirection + 1];
      }

      void GlowWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         var relativePositionToInnerGrid = e.GetPosition(this);
         int xDirection = GetResizeXDirection(relativePositionToInnerGrid);
         int yDirection = GetResizeYDirection(relativePositionToInnerGrid);

         var offsetMap = new[,] {
            {4, 3, 5},
            {1, 0, 2},
            {7, 6, 8} // Defined by winapi
         };
         var offset = offsetMap[yDirection + 1, xDirection + 1];
         if (offset != 0) // 0 - clicked in interior
            SendMessage(m_parentHandle, 0x112, (IntPtr)(61440 + offsetMap[yDirection + 1, xDirection + 1]), IntPtr.Zero);
         //m_windowHandleSource.Handle
      }
      protected override void OnSourceInitialized(EventArgs e)
      {
 	       base.OnSourceInitialized(e);
         m_windowHandleSource = PresentationSource.FromVisual(this) as HwndSource;
         //source.AddHook(WndProc);
      }
   }
}
