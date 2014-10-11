#pragma once

#include "dlc_pch.hpp"
#include <boost/noncopyable.hpp>
#include "../Util/ILogger.hpp"
#include "../IO/DSP/DSPExNode.hpp"

namespace Dargon { namespace Init {
   class BootloaderRemoteLogger : boost::noncopyable, public Dargon::Util::ILogger
   {
   private:
      const BootstrapContext* m_context;

   public:
      BootloaderRemoteLogger(const BootstrapContext* context);
      void Log(UINT32 loggerLevel, LoggingFunction logger);
   };
} }