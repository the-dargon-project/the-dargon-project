using System;
using Dargon.IO.RADS.Archives;

namespace Dargon.LeagueOfLegends.RADS
{
   public interface IRadsArchiveReference : IDisposable
   {
      RiotArchive Value { get; }
   }
}