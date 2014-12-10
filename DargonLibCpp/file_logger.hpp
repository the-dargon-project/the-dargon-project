#pragma once

#include <string>
#include <iostream>
#include <fstream>
#include <mutex>
#include "Dargon.hpp"
#include "logger.hpp"
#include "logger_levels.hpp"
#include "noncopyable.hpp"

namespace dargon {
   class file_logger : public logger, dargon::noncopyable
   {
   public:
      static void Initialize(std::string fileName);
      static inline void L(UINT32 file_loggerLevel, LoggingFunction file_logger);
      // System-level logging.  Stuff that only Core Implementors care about.
      static inline void SL(UINT32 file_loggerLevel, LoggingFunction file_logger);
      // System's Network-Level Logging.  For debugging netcode.
      static inline void SNL(UINT32 file_loggerLevel, LoggingFunction file_logger);

   private:
      static file_logger* s_instance;

   private:
      /// <summary>
      /// Initializes a new instance of a file_logger that directs output to the given file path as
      /// well as console/dargon output.
      /// </summary>
      /// <param name="fileName">The path to the file which we are outputting to.</param>
      file_logger(std::string fileName);
      inline void Log(UINT32 file_loggerLevel, LoggingFunction file_logger);

   private:
      unsigned int m_file_loggerFilter;
      int m_indentationCount;
      std::ofstream m_outputStream;
   };
}

#include "file_logger.inl.hpp"

//TODO: The do-while loop allows the caller to place a semicolon after the LogOnce() call.
#define LogOnce(file_loggerLevel, a) \
   do \
   { \
      static std::mutex myMutex; \
      static bool hasRun = false; \
      if(!hasRun) \
      { \
         std::lock_guard<std::mutex> lock(myMutex); \
         if(!hasRun) \
         { \
            dargon::file_logger::GetOutputStream(file_loggerLevel) << a; \
            hasRun = true; \
         } \
      } \
   } while(false)