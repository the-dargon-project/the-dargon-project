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
#include "Commands/FileRedirectionCommandHandler.hpp"
#include "Subsystems/FileSubsystem.hpp"
#include "Subsystems/KernelSubsystem.hpp"
#include "vfm/vfm_reader.hpp"

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

   // initialize libvfm dependencies
   auto sector_factory = std::make_shared<vfm_sector_factory>();
   std::cout << "vfm constructing reader" << std::endl;
   auto vfm_reader = std::make_shared<dargon::vfm_reader>(sector_factory);
   std::cout << "vfm constructing fs" << std::endl;
   auto vfm_fs = std::make_shared<std::fstream>();
   vfm_fs->open("C:\\Users\\ItzWarty\\.dargon\\temp\\de87afe80b6e2b49a190818a1a4151ce\\0.0.0.235\\Archive_3.raf.dat.vfm", std::fstream::in | std::fstream::binary);
   binary_reader vfm_fs_reader(vfm_fs);
   std::cout << "vfm load" << std::endl;
   auto vfm = vfm_reader->load(vfm_fs_reader);
   std::cout << "vfm begin" << std::endl;
   for (auto it = vfm->sectors_begin(); it != vfm->sectors_end(); it++) {
      std::cout << ": " << (*it)->to_string() << std::endl;
   }
   std::cout << "vfm end" << std::endl;

   // load configuration
   auto configuration = Configuration::Parse(flags, properties);
   
   // construct command manager
   auto command_manager = std::make_shared<CommandManager>(dtp_session, configuration);
   
   // initialize subsystem dependencies
   std::cout << "Initializing Subsystems" << std::endl;
   Subsystem::Initialize(context, configuration, logger);
   auto file_subsystem = std::make_shared<FileSubsystem>();
   file_subsystem->Initialize();
   auto kernel_subsystem = std::make_shared<KernelSubsystem>();
   kernel_subsystem->Initialize();

   // initialize command handlers
   auto file_swap_command_handler = std::make_shared<FileRedirectionCommandHandler>(command_manager, file_subsystem);
   file_swap_command_handler->Initialize();
   
   // initialize command manager
   command_manager->Initialize();

   // Suspend count can be >1 due to LAUNCH_SUSPENDED override by another instance.
   std::cout << "Application::Initialize resuming main thread." << std::endl;
   while (ResumeThread(main_thread_handle) > 0);
}
