#pragma once

#include "base.hpp"
#include "noncopyable.hpp"
#include "DIMCommandTypes.hpp"

namespace dargon { namespace IO { namespace DIM {
   // A command sent by DargonD for the Dargon Injected Module.
   // Commands are recieved via GetDIMCommandListHandler, handed to Core, and then dispatched to their
   // executors. Executors are responsible for freeing the memory of the command.
   struct DIMCommand : dargon::noncopyable {
      CommandType type;
      UINT32 length;
      BYTE* data;
   };
} } }