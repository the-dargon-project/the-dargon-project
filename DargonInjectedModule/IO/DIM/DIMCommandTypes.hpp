#pragma once

#include <map>
#include <memory>
#include <unordered_map>
#include "base.hpp"

namespace dargon { namespace IO { namespace DIM {
   class IDIMCommandHandler;
   struct DIMCommand;
} } }

typedef std::unordered_multimap<dargon::IO::DIM::IDIMCommandHandler*, dargon::IO::DIM::DIMCommand*> DIMHandlerToCommandsMap;

#define CommandType std::string