#include "stdafx.h"
#include <sstream>
#include "io.hpp"
#include "IO/IoProxy.hpp"
#include "Bootloader.hpp"
#include "bootstrap_context.hpp"
#include "BootloaderRemoteLogger.hpp"
using namespace dargon::Init;
using namespace dargon::IO;
using namespace dargon::IO::DSP;

void Bootloader::BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle)
{
   // Create context object and fill it with parameter data
   auto context = std::make_shared<bootstrap_context>();
   std::cout << "Bootloader::BootstrapInjectedModule passed bootstrap_context ctor" << std::endl;
   context->module_handle = moduleHandle;
   
   // Create IoProxy so that hooks don't pick up DIM invocations.
   std::cout << "Initializing I/O Proxy" << std::endl;
   context->io_proxy = std::make_shared<IoProxy>();;
   context->io_proxy->Initialize();

   // Connect to DSPEx server and get bootstrap arguments
   std::string nodeName = "DargonInjectedModule_" + std::to_string(GetProcessId(GetCurrentProcess()));
   context->dtp_node = std::shared_ptr<DSPExNode>(new DSPExNode(DSPExNodeRole::Client, nodeName, context->io_proxy));
   std::cout << "DSPExNode constructed for named pipe " << nodeName << std::endl;
   
   context->dtp_session = std::shared_ptr<DSPExNodeSession>(context->dtp_node->Connect(nodeName));
   std::cout << "Bootloader::BootstrapInjectedModule DSPExClient::ConnectLocal passed" << std::endl;
   context->dtp_session->GetBootstrapArguments(context);
   std::cout << "Bootloader::BootstrapInjectedModule GetBootstrapArguments passed" << std::endl;

   // Create a DSPEx Remote file_logger
   context->logger = std::make_shared<BootloaderRemoteLogger>(context);
   std::cout << "Bootloader::BootstrapInjectedModule file_logger Constructed" << std::endl;

   // Call application's init handler
   init(context);
}