#pragma once

#include "../../Base.hpp"
#include "DIMTaskTypes.hpp"

namespace Dargon { namespace IO { namespace DIM {
   // A task sent by DargonD for the Dargon Injected Module.
   // Tasks are recieved via GetDIMTaskListHandler, handed to Core, and then dispatched to their
   // executors. Executors are responsible for freeing the memory of the task.
   struct DIMTask : boost::noncopyable {
      TaskType type;
      UINT32 length;
      BYTE data[0];
   };
} } }