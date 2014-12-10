#pragma once

#include <map>
#include <memory>
#include <unordered_map>
#include "base.hpp"

namespace dargon { namespace IO { namespace DIM {
   class IDIMTaskHandler;
   struct  DIMTask;
} } }

typedef std::unordered_multimap<dargon::IO::DIM::IDIMTaskHandler*, dargon::IO::DIM::DIMTask*> DIMHandlerToTasksMap;

#define TaskType std::string