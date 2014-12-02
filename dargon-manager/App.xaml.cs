using System.Windows;
using DargonManager;

namespace Dargon.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       private void Application_Startup(object sender, StartupEventArgs e)
       {
          new DargonManagerApplication().Run();
       }
    }
}
