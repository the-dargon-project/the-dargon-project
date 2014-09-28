namespace Dargon.LeagueOfLegends.Modifications
{
   public interface ITask
   {
      Status Status { get; }
      int SuccessCount { get; }
      int TotalCount { get; }
      ITask NextTask { get; } // HACK

      void WaitForTermination();
   }
}