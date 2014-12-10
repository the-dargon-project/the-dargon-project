#include "../dlc_pch.hpp"
#include <sstream>
#include "../io.hpp"
#include "../IO/IoProxy.hpp"
#include "Bootloader.hpp"
#include "BootstrapContext.hpp"
#include "BootloaderRemoteLogger.hpp"
using namespace dargon::Init;
using namespace dargon::IO;
using namespace dargon::IO::DSP;

void Bootloader::BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle)
{
   // Create context object and fill it with parameter data
   BootstrapContext* context = new BootstrapContext();
   std::cout << "Bootloader::BootstrapInjectedModule passed BootstrapContext ctor" << std::endl;
   context->ApplicationModuleHandle = moduleHandle;
   
   // Create IoProxy so that hooks don't pick up DIM invocations.
   std::cout << "Initializing I/O Proxy" << std::endl;
   auto ioProxy = std::make_shared<IoProxy>();
   ioProxy->Initialize();
   context->IoProxy = ioProxy;

   // Connect to DSPEx server and get bootstrap arguments
   std::stringstream ss;
   ss << "DargonInjectedModule_" << GetProcessId(GetCurrentProcess());
   DSPExNode* dspExNode = new DSPExNode(DSPExNodeRole::Client, ss.str(), ioProxy);
   std::cout << "DSPExNode constructed for named pipe " << ss.str() << std::endl;
   context->DSPExNode = dspExNode;
   
   DSPExNodeSession* session = dspExNode->Connect(ss.str());   
   context->DIMSession = session;
   std::cout << "Bootloader::BootstrapInjectedModule DSPExClient::ConnectLocal passed" << std::endl;
   context->DIMSession->GetBootstrapArguments(context);
   std::cout << "Bootloader::BootstrapInjectedModule GetBootstrapArguments passed" << std::endl;

   // Create a DSPEx Remote file_logger
   context->logger = new BootloaderRemoteLogger(context);
   std::cout << "Bootloader::BootstrapInjectedModule file_logger Constructed" << std::endl;

   // Call application's init handler
   init(context);
}