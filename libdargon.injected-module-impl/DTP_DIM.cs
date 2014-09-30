using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.InjectedModule
{
   public enum DTP_DIM
   {
      USER_RESERVED_BEGIN                       = 0x00,
      
      C2S_GET_BOOTSTRAP_ARGS                    = 0x01,
      C2S_GET_INITIAL_TASKLIST                  = 0x02,
      C2S_REMOTE_LOG                            = 0x03,

      USER_RESERVED_END                         = 0x7F,
   }
}
