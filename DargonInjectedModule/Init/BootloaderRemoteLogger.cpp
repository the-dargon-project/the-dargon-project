#include "stdafx.h"
#include <memory>
#include "BootloaderRemoteLogger.hpp"
#include "bootstrap_context.hpp"
#include "logger.hpp"
#include "../IO/DSP/DSPExNodeSession.hpp"
#include "../IO/DSP/ClientImpl/DSPExLITRemoteLogHandler.hpp"
using namespace dargon::Init;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

BootloaderRemoteLogger::BootloaderRemoteLogger(std::shared_ptr<const bootstrap_context> context)
   : m_context(context) {

}

void BootloaderRemoteLogger::Log(UINT32 file_loggerLevel, LoggingFunction file_logger) {
   if (isLoggingEnabled) {
      m_context->dtp_session->Log(file_loggerLevel, file_logger);
   }
}