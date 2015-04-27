using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.RADS;

namespace Dargon.LeagueOfLegends.RADS
{
   public interface IRadsProjectReference : IDisposable
   {
      RiotProject Value { get; }  
   }
}
