using Dargon.RADS.Archives;
using System;

namespace Dargon.LeagueOfLegends.RADS {
   public interface IRadsArchiveReference : IDisposable
   {
      RiotArchive Value { get; }
   }
}