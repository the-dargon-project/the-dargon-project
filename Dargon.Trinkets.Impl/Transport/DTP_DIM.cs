namespace Dargon.Trinkets.Transport
{
   public enum DTP_DIM : byte
   {
      USER_RESERVED_BEGIN                       = 0x00,
      
      C2S_GET_BOOTSTRAP_ARGS                    = 0x01,
      C2S_GET_INITIAL_COMMAND_LIST              = 0x02,
      C2S_REMOTE_LOG                            = 0x03,

      USER_RESERVED_END                         = 0x7F,
   }
}
