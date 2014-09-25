namespace Dargon.LeagueOfLegends.Modifications
{
   public interface IResolutionTask
   {
      ResolutionStatus Status { get; }
      int FilesResolved { get; }
      int FileCount { get; }
      IResolutionTask NextTask { get; } // HACK

      void Cancel();
      void WaitForTermination();
   }
}