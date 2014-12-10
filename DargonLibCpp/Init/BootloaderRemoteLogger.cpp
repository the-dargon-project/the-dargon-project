#include "dlc_pch.hpp"
#include "BootloaderRemoteLogger.hpp"
#include "BootstrapContext.hpp"
#include "../Util/ILogger.hpp"
#include "../IO/DSP/DSPExNodeSession.hpp"
#include "../IO/DSP/ClientImpl/DSPExLITRemoteLogHandler.hpp"
using namespace dargon::Init;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

BootloaderRemoteLogger::BootloaderRemoteLogger(const BootstrapContext* context)
   : m_context(context)
{
}

void BootloaderRemoteLogger::Log(UINT32 loggerLevel, LoggingFunction logger)
{
   m_context->DIMSession->Log(loggerLevel, logger);
}