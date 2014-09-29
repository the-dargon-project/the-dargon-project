#pragma once

#include <map>
#include <unordered_map>
#include "../../Base.hpp"
namespace Dargon { namespace IO { namespace DIM {
   class IDIMTaskHandler;
   struct  DIMTask;
} } }

typedef std::unordered_multimap<Dargon::IO::DIM::IDIMTaskHandler*, Dargon::IO::DIM::DIMTask*> DIMHandlerToTasksMap;

#define TaskType UINT32
#define TT_FILESWAP ((TaskType)0x00000001U)