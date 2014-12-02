using System.Windows.Forms.Integration;
using DargonManager;

namespace Dargon.Manager
{
   public class DargonManagerApplication
   {
      /// <summary>
      /// Runs the main entry point of a Dargon application
      /// </summary>
      /// <param name="dargon"></param>
      public void Run()
      {
         var mainWindow = new MainWindow(new DummyViewModel());
         ElementHost.EnableModelessKeyboardInterop(mainWindow); // Makes it so that we can type in textboxes...
         mainWindow.Show();
      }

      public class DummyViewModel : DMViewModelBase {
         public override void ImportModifications(string[] drop) { }
      }
   }
}
