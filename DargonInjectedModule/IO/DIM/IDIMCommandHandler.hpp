#pragma once

#include <vector>
#include "noncopyable.hpp"

#include "DIMCommand.hpp"
#include "DIMCommandTypes.hpp"

namespace dargon { namespace IO { namespace DIM {
   class IDIMCommandHandler : dargon::noncopyable {
   private:

   public:
      virtual ~IDIMCommandHandler() { };
      virtual void Initialize() = 0;
      virtual bool IsCommandTypeSupported(CommandType& type) = 0;
      virtual void ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) = 0;
   };
} } }