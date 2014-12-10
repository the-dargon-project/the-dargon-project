#include "dlc_pch.hpp"
#include "BootloaderRemoteLogger.hpp"
#include "BootstrapContext.hpp"
#include "../logger.hpp"
#include "../IO/DSP/DSPExNodeSession.hpp"
#include "../IO/DSP/ClientImpl/DSPExLITRemoteLogHandler.hpp"
using namespace dargon::Init;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

BootloaderRemoteLogger::BootloaderRemoteLogger(const BootstrapContext* context)
   : m_context(context)
{
}

void BootloaderRemoteLogger::Log(UINT32 file_loggerLevel, LoggingFunction file_logger)
{
   m_context->DIMSession->Log(file_loggerLevel, file_logger);
}