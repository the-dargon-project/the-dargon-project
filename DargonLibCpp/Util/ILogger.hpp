#pragma once

#include <ostream>
#include <functional>
#include "../Dargon.hpp"
#include "LoggerLevels.hpp"

typedef std::function<void(std::ostream&)> DoLog;

namespace Dargon { namespace Util {
   class ILogger
   {
   public:
      virtual void Log(UINT32 loggerLevel, DoLog logger) = 0;
   };
} }