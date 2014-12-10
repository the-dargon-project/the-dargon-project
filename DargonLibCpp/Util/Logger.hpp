#pragma once

#include <string>
#include <iostream>
#include <fstream>
#include <mutex>
#include <boost/iostreams/tee.hpp>
#include <boost/iostreams/stream.hpp>
#include "../Dargon.hpp"
#include "ILogger.hpp"
#include "LoggerLevels.hpp"
#include "noncopyable.hpp"

namespace Dargon { namespace Util { 
   class Logger : public ILogger, Dargon::Util::noncopyable
   {
   public:
      static void Initialize(std::string fileName);
      static inline void L(UINT32 loggerLevel, LoggingFunction logger);
      // System-level logging.  Stuff that only Core Implementors care about.
      static inline void SL(UINT32 loggerLevel, LoggingFunction logger);
      // System's Network-Level Logging.  For debugging netcode.
      static inline void SNL(UINT32 loggerLevel, LoggingFunction logger);

   private:
      static Logger* s_instance;

   private:
      /// <summary>
      /// Initializes a new instance of a Logger that directs output to the given file path as
      /// well as console/dargon output.
      /// </summary>
      /// <param name="fileName">The path to the file which we are outputting to.</param>
      Logger(std::string fileName);
      inline void Log(UINT32 loggerLevel, LoggingFunction logger);

   private:
      unsigned int m_loggerFilter;
      int m_indentationCount;
      std::ofstream m_outputStream;
   };
} }

#include "Logger.inl.hpp"

//TODO: The do-while loop allows the caller to place a semicolon after the LogOnce() call.
#define LogOnce(LoggerLevel, a) \
   do \
   { \
      static std::mutex myMutex; \
      static bool hasRun = false; \
      if(!hasRun) \
      { \
         std::lock_guard<std::mutex> lock(myMutex); \
         if(!hasRun) \
         { \
            Dargon::Util::Logger::GetOutputStream(LoggerLevel) << a; \
            hasRun = true; \
         } \
      } \
   } while(false)