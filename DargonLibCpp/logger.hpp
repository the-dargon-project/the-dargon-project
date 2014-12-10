#pragma once

#include <ostream>
#include <functional>
#include "dargon.hpp"
#include "logger_levels.hpp"

// Verbose file_logger Level - for things that happen every single frame
#define LL_VERBOSE  0x00000001UL

// Info file_logger Level - for things that are guaranteed to happen,
// but which occur repeatedly (ex: texture loads).
#define LL_INFO     0x00000002UL

// Notice file_logger Level - for things that are guaranteed to happen,
// but which occur once.  (ex: Connect to Dargon Manager, Init D3D)
#define LL_NOTICE   0x00000004UL

// Warning file_logger Level - for things that are a bit strange,
// but not potentially fatal.  Ex: Unable to find champion portrait
// texture.  We can recover from warnings, usually.
#define LL_WARN     0x00000008UL

// Error file_logger Level - for things that are going to be fatal to Dargon
// Ex: Dargon is injected to wrong process, Dargon can't read config files, etc.
#define LL_ERROR    0x00000010UL

// Always file_logger level - useful for when you're writing new features.
#define LL_ALWAYS   0x01000000UL

typedef std::function<void(std::ostream&)> LoggingFunction;

namespace dargon { 
   class logger
   {
   public:
      virtual void Log(UINT32 file_loggerLevel, LoggingFunction loggingFunction) = 0;
   };
}
