#include "dlc_pch.hpp"
#include <IO/DIM/IDIMCommandHandler.hpp>

namespace dargon { namespace Subsystems {
   class FileOverrideTaskHandler : public dargon::IO::DIM::IDIMCommandHandler
   {
   public:
      bool IsCommandTypeSupported(CommandType& type) override;
      void ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) override;
   };
} }