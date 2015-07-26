using ItzWarty;
using NLog;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dargon.Modifications {
   public class Modification2 : INotifyPropertyChanged {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string repositoryName;
      private readonly string repositoryPath;
      private string friendlyName;
      private string[] authors;
      private bool isEnabled;
      public event PropertyChangedEventHandler PropertyChanged;

      public Modification2(string repositoryName, string repositoryPath) {
         this.repositoryName = repositoryName;
         this.repositoryPath = repositoryPath;
      }

      public string RepositoryName => repositoryName;
      public string RepositoryPath => repositoryPath;
      public string FriendlyName { get { return friendlyName; } set { friendlyName = value; OnPropertyChanged(); } }
      public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; OnPropertyChanged(); } }
      public string[] Authors { get { return authors; } set { authors = value; OnPropertyChanged(); } }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
