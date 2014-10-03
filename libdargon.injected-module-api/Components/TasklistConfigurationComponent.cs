using Dargon.InjectedModule.Tasks;

namespace Dargon.InjectedModule.Components
{
   public class TasklistConfigurationComponent : IConfigurationComponent
   {
      private const string kTasklistFlagName = "--enable-dim-tasklist";
      
      private readonly ITasklist tasklist;

      public TasklistConfigurationComponent(ITasklist tasklist) { this.tasklist = tasklist; }

      public ITasklist Tasklist { get { return tasklist; } }

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) { builder.SetFlag(kTasklistFlagName); }
   }
}
