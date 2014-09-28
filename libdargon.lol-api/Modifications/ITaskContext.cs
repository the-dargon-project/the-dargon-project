namespace Dargon.LeagueOfLegends.Modifications
{
   public interface ITaskContext<out TTask>
   {
      TTask Task { get; }
   }
}