using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dargon.Manager.Controllers;
using Dargon.Manager.Models;
using Dargon.Manager.ViewModels;
using DargonManager;
using DargonManager.SampleData;

namespace Dargon.Manager {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private readonly RootController rootController;
      private readonly ModificationImportController modificationImportController;
      private GlowWindow m_glowWindow = null;
      private HwndSource m_windowHandleSource;
      private StatusModel statusModel;

      /// <summary>
      /// Main Window with dummy data
      /// </summary>
      public MainWindow()
      {
         this.InitializeComponent();
         SourceInitialized += ShellWindow_SourceInitialized;

         // Insert code required on object creation below this point.
         List<SampleModification> mods = new List<SampleModification>();
         mods.Add(new SampleModification { Name = "Beauty Queen Ezreal", Author = "Warty", IsEnabled = true, Type = "Actors > Ezreal" });
         mods.Add(new SampleModification { Name = "Naughty Nautilus", Author = "Warty", IsEnabled = true, Type = "Actors > Nautilus" });
         mods.Add(new SampleModification { Name = "Tencent Artwork", Author = "Ququroon", IsEnabled = false, Type = "Actors" });
         mods.Add(new SampleModification { Name = "Sunset Beach Rift", Author = "Yurixy Works", IsEnabled = true, Type = "Maps > Summoner's Rift" });
         
         AllowDrop = true;
         PreviewDragEnter += MainWindow_PreviewDragEnter;
         DragEnter += ContentControl_DragEnter;
         DragLeave += ContentControl_DragLeave;
         DragOver += ContentControl_DragOver;
         Drop += ContentControl_Drop;
      }

      public MainWindow(RootController rootController) {
         this.rootController = rootController;
         this.modificationImportController = rootController.GetModificationImportController();
         this.statusModel = rootController.GetStatusModel();

         InitializeComponent();
         SourceInitialized += ShellWindow_SourceInitialized;
         Loaded += HandleLoad;
      }

      public StatusModel StatusModel { get { return rootController.GetStatusModel(); } }

      public ModificationListingViewModel ModificationListingViewModel { get { return rootController.GetModificationListingViewModel(); } }

      private void HandleLoad(object sender, RoutedEventArgs e) {
         AllowDrop = true;
         DragEnter += HandleDragEnter;
         DragLeave += HandleDragLeave;
         // Drop += (s, e) => {
         //    Console.WriteLine("DROP " + e.Effects);
         //    string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
         //    foreach (var d in data)
         //       Console.WriteLine(d);
         // 
         //    viewmodel.ImportModifications(data);
         // 
         //    m_glowWindow.Model.IsEmbiggened = false;
         // };
      }

      private void HandleDragEnter(object sender, DragEventArgs e) {
         e.Handled = true;

         var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
         if (!modificationImportController.ValidateDropHover(paths)) {
            m_glowWindow.Model.IsEmbiggened = false;
            e.Effects = DragDropEffects.None;
         } else {
            m_glowWindow.Model.IsEmbiggened = true;
            
            if (e.AllowedEffects.HasFlag(DragDropEffects.Copy))
               e.Effects |= DragDropEffects.Copy;
            if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
               e.Effects |= DragDropEffects.Move;
            if (e.AllowedEffects.HasFlag(DragDropEffects.Link))
               e.Effects |= DragDropEffects.Link;
         }
      }

      private void HandleDragLeave(object sender, DragEventArgs e) {
         modificationImportController.HandleDragLeave();
         m_glowWindow.Model.IsEmbiggened = false;
      }

      private void SetupWindowGlow()
      {
         //Console.WriteLine("SWG");
         if (m_glowWindow == null)
         {
            //Console.WriteLine("Create glow window");
            m_glowWindow = new GlowWindow(m_windowHandleSource.Handle, this);
            m_glowWindow.Show();
            this.Owner = m_glowWindow;

            m_glowWindow.MouseMove += HandleResizeBorderMouseMove;
            SizeChanged += (s, e) => m_glowWindow.Update(false);
            LocationChanged += (s, e) => m_glowWindow.Update(false);
            LostFocus += (s, e) => m_glowWindow.Update(false);
            m_glowWindow.Update(false);
         }
      }

      protected override void OnSourceInitialized(EventArgs e)
      {
         base.OnSourceInitialized(e);
         m_windowHandleSource = PresentationSource.FromVisual(this) as HwndSource;
         SetupWindowGlow();
      }

      /// <summary>
      /// Event fired when the template of the custom window is applied
      /// </summary>
      public override void OnApplyTemplate()
      {
         Button minimizeButton = GetTemplateChild("minimizeButton") as Button;
         if (minimizeButton == null)
            Console.WriteLine("Couldn't get minimize button!");
         else
            minimizeButton.Click += (s, e) => WindowState = System.Windows.WindowState.Minimized;

         Button maximizeButton = GetTemplateChild("maximizeButton") as Button;
         if (maximizeButton == null)
            Console.WriteLine("Couldn't get maximize button!");
         else
            maximizeButton.Click += (s, e) => {
               if(WindowState == WindowState.Normal)
                  WindowState = WindowState.Maximized;
               else if (WindowState == WindowState.Maximized)
                  WindowState = WindowState.Normal;
            };

         Button closeButton = GetTemplateChild("closeButton") as Button;
         if (closeButton == null)
            Console.WriteLine("Couldn't get close button!");
         else
            closeButton.Click += (s, e) => Close();

         TreatAsWindowDraggable((FrameworkElement)GetTemplateChild("dargonRectangle"));
         TreatAsWindowDraggable((FrameworkElement)GetTemplateChild("moveRectangle"));

         var appNameAndMenuStripPanel = GetTemplateChild("appNameAndMenuStrip") as Panel;
         if (appNameAndMenuStripPanel == null)
            Console.WriteLine("Couldn't get appnameandmenustrip panel!");
         else
         {
            appNameAndMenuStripPanel.MouseDown += (s, e) => {
               var hitTestResult = VisualTreeHelper.HitTest(appNameAndMenuStripPanel, e.GetPosition(appNameAndMenuStripPanel));
               if (hitTestResult == null) // Shouldn't happen..
                  DragMove();
               else if (hitTestResult.VisualHit is StackPanel) // Clicked the stackpanel - empty space
                  DragMove();
               else if (hitTestResult.VisualHit is FrameworkElement && ((FrameworkElement)hitTestResult.VisualHit).Name == "applicationLogo")
                  DragMove();
               else if (hitTestResult.VisualHit is FrameworkElement && ((FrameworkElement)hitTestResult.VisualHit).Name == "applicationLogoRectangle")
                  DragMove();
               else if (hitTestResult.VisualHit is FrameworkElement && ((FrameworkElement)hitTestResult.VisualHit).Name == "appNameAndMenuStripFiller")
                  DragMove();
               else if (hitTestResult.VisualHit is FrameworkElement && ((FrameworkElement)hitTestResult.VisualHit).Name == "menuStripRectangle")
                  DragMove();
            };
         }

         // Resize Border code
         ResizeBorder = GetTemplateChild("resizeBorder") as Border;
         if (ResizeBorder == null)
            Console.WriteLine("Got null resize border!");
         else
         {
            ResizeBorder.MouseMove += HandleResizeBorderMouseMove;
            ResizeBorder.PreviewMouseDown += HandleResizeBorderPreviewMouseDown;
         }

         ResizeBorderInnerGrid = GetTemplateChild("resizeBorderInnerGrid") as Grid;

         // Make the menu strip's empty space draggable
         base.OnApplyTemplate();
      }

      /// <summary>
      /// Assign some events to the given control so that it causes a BeginDrag on the window when
      /// mousedown-ed.
      /// </summary>
      private void TreatAsWindowDraggable(FrameworkElement control)
      {
         if (control == null)
            Console.WriteLine("TreatAsWindowDraggable got null parameter");
         else
            control.MouseDown += (s, e) => DragMove();
      }

      //-------------------------------------------------------------------------------------------
      // Resizing by Mouse code
      //-------------------------------------------------------------------------------------------
      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

      private Border ResizeBorder { get; set; }
      private Grid ResizeBorderInnerGrid { get; set; }

      private void HandleResizeBorderMouseMove(object sender, MouseEventArgs e)
      {
         if (WindowState == WindowState.Maximized) 
            return;

         var relativePositionToInnerGrid = e.GetPosition(ResizeBorderInnerGrid);
         int xDirection = GetResizeXDirection(relativePositionToInnerGrid);
         int yDirection = GetResizeYDirection(relativePositionToInnerGrid);

         var directionMap = new Cursor[,] {
            {Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW},
            {Cursors.SizeWE,   Cursors.Arrow,  Cursors.SizeWE  },
            {Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE}
         };

         Cursor = directionMap[yDirection + 1, xDirection + 1];
      }

      private void HandleResizeBorderPreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         if (WindowState == WindowState.Maximized)
            return;

         var relativePositionToInnerGrid = e.GetPosition(ResizeBorderInnerGrid);
         int xDirection = GetResizeXDirection(relativePositionToInnerGrid);
         int yDirection = GetResizeYDirection(relativePositionToInnerGrid);

         var offsetMap = new[,] {
            {4, 3, 5},
            {1, 0, 2},
            {7, 6, 8} // Defined by winapi
         };
         var offset = offsetMap[yDirection + 1, xDirection + 1];
         if (offset != 0) // 0 - clicked in interior
            SendMessage(m_windowHandleSource.Handle, 0x112, (IntPtr)(61440 + offsetMap[yDirection + 1, xDirection + 1]), IntPtr.Zero);
      }

      private int GetResizeXDirection(Point relativePositionToInnerGrid)
      {
         int xDirection = 0;
         if (relativePositionToInnerGrid.X < 0)
            xDirection = -1;
         else if (relativePositionToInnerGrid.X >= ResizeBorderInnerGrid.ActualWidth)
            xDirection = 1;
         return xDirection;
      }

      private int GetResizeYDirection(Point relativePositionToInnerGrid)
      {
         int yDirection = 0;
         if (relativePositionToInnerGrid.Y < 0)
            yDirection = -1;
         else if (relativePositionToInnerGrid.Y >= ResizeBorderInnerGrid.ActualHeight)
            yDirection = 1;
         return yDirection;
      }
      //

      void MainWindow_PreviewDragEnter(object sender, DragEventArgs e)
      {
         Console.WriteLine("!" + e.AllowedEffects);
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            e.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
         }
      }


      private void ContentControl_DragEnter(object sender, DragEventArgs e)
      {
         Console.WriteLine("!" + e.AllowedEffects);
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            e.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
         }
      }

      private void ContentControl_DragLeave(object sender, DragEventArgs e)
      {

      }

      private void ContentControl_DragOver(object sender, DragEventArgs e)
      {
         Console.WriteLine("!" + e.AllowedEffects);
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
         {
            e.Effects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
         }
      }

      private void ContentControl_Drop(object sender, DragEventArgs e)
      {
         if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey)) //Control key
         {
            //Drop things separately
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
               //ViewModel.Dargon.LeagueOfLegends.Modifications.ImportFiles(new[] { file });
            }
         }
         else
         {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            //files.ToList().ForEach(System.Console.WriteLine);
            //Dargon.LeagueOfLegends.Modifications.ImportFiles(files);
         }
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

         // below tries to make glowiness ok, but doesn't work
         int val = 2;
         DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

         var m = new Margins { bottomHeight = -1, leftWidth = -1, rightWidth = -1, topHeight = -1 };
         DwmExtendFrameIntoClientArea(helper.Handle, ref m);
      }
   }
}