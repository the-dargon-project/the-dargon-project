#pragma once

#include "dlc_pch.hpp"
#include "../logger.hpp"
#include "../noncopyable.hpp"
#include "../IO/DSP/DSPExNode.hpp"

namespace dargon { namespace Init {
   class BootloaderRemoteLogger : dargon::noncopyable, public dargon::logger
   {
   private:
      const BootstrapContext* m_context;

   public:
      BootloaderRemoteLogger(const BootstrapContext* context);
      void Log(UINT32 file_loggerLevel, LoggingFunction file_logger);
   };
} }