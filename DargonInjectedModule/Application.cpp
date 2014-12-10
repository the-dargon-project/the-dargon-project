#include "stdafx.h"
#include <memory>
#include <process.h>
#include <Windows.h>

#include "util.hpp"
#include "Init/Bootloader.hpp"
#include "IO/DIM/CommandManager.hpp"
#include "IO/DSP/DSPExNodeSession.hpp"
#include "file_logger.hpp"

#include "Application.hpp"
#include "Configuration.hpp"
#include "feature_toggles.hpp"
#include "Subsystems/FileSubsystem.hpp"
#include "Subsystems/KernelSubsystem.hpp"

using namespace dargon;
using namespace dargon::Init;
using namespace dargon::IO::DIM;
using namespace dargon::Subsystems;

HMODULE Application::module_handle;
HANDLE Application::main_thread_handle;

void Application::HandleDllEntry(HMODULE hModule) {
   module_handle = hModule;

   // initialize diagnostic dependencies
   file_logger::initialize("C:/DargonLog.log");
   std::cout << "Logger initialized!" << std::endl;

   // suspend host application's main thread while we init
   main_thread_handle = OpenMainThread();
   SuspendThread(main_thread_handle);
   std::cout << "Suspended main thread." << std::endl;

   // begin bootstrapping process in new thread to free injector
   _beginthreadex(nullptr, 0, BootstrappingThreadStart, nullptr, 0, nullptr);
}

uint32_t WINAPI Application::BootstrappingThreadStart(void* throwaway) {
   std::cout << "Bootstrap Thread Started" << std::endl;
   Bootloader::BootstrapInjectedModule(
      &Initialize,
      Application::module_handle
   );
   std::cout << "Bootstrap Thread Exiting" << std::endl;
   return 0;
}

void Application::Initialize(std::shared_ptr<const bootstrap_context> context) {
   // unpack values stored in context
   auto flags = context->argument_flags;
   auto properties = context->argument_properties;
   auto io_proxy = context->io_proxy;
   auto dtp_node = context->dtp_node;
   auto dtp_session = context->dtp_session;
   auto logger = context->logger;

   // load configuration
   auto configuration = Configuration::Parse(flags, properties);
   
   // construct task manager
   auto command_manager = std::make_shared<CommandManager>(dtp_session, configuration);
   
   // initialize subsystem dependencies
   std::cout << "Initializing Subsystems" << std::endl;
   Subsystem::Initialize(context, configuration, logger);
   auto file_subsystem = std::make_shared<FileSubsystem>(command_manager);
   file_subsystem->Initialize();
   auto kernel_subsystem = std::make_shared<KernelSubsystem>();
   kernel_subsystem->Initialize();
   
   // initialize task manager.
   command_manager->Initialize();

   // Suspend count can be >! due to LAUNCH_SUSPENDED override by another instance.
   while (ResumeThread(main_thread_handle) > 0);

   std::cout << "Application::Initialize resuming main thread." << std::endl;

}