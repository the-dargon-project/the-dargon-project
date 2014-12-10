#pragma once

#include "dlc_pch.hpp"
#include <memory>
#include "logger.hpp"
#include "noncopyable.hpp"
#include "IO/DSP/DSPExNode.hpp"

namespace dargon { namespace Init {
   class BootloaderRemoteLogger : dargon::noncopyable, public dargon::logger
   {
   private:
      std::shared_ptr<const bootstrap_context> m_context;

   public:
      BootloaderRemoteLogger(std::shared_ptr<const bootstrap_context> context);
      void Log(UINT32 file_loggerLevel, LoggingFunction file_logger);
   };
} }