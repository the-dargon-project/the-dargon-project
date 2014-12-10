#include "stdafx.h"
#include <string>
#include <iostream>
#include <unordered_set>
#include <fcntl.h>
#include <io.h>
#include <stdio.h>
#include <process.h>
#include <Windows.h>

#include "Init/Bootloader.hpp"
#include <IO/DIM/DIMTask.hpp>
#include <IO/DIM/IDIMTaskHandler.hpp>
#include <IO/DIM/DIMTaskTypes.hpp>
#include "IO/DSP/DSPExNodeSession.hpp"
#include "util.hpp"
#include <logger.hpp>

#include "Core.hpp"
#include "Subsystem.hpp"
#include "Subsystems/KernelSubsystem.hpp"
#include "Subsystems/FileSubsystem.hpp"
#include "feature_toggles.hpp"

#include "ThirdParty/guicon.h"

using namespace dargon::InjectedModule;
using namespace dargon::Init;
using namespace dargon;
using namespace std::placeholders;
using namespace dargon::Subsystems;
using namespace dargon::IO::DSP::ClientImpl;
using namespace dargon::IO::DIM;

Core::Core(HMODULE hModule) : module_handle(hModule), task_manager(nullptr) {

   std::cout << "Entered Core::Core, suspending main thread" << std::endl;
   main_thread_handle = OpenMainThread();
   std::cout << " - Thread ID " << main_thread_handle << std::endl;
   SuspendThread(main_thread_handle);

   std::cout << "Creating Dargon Bootstrap thread" << std::endl;
   _beginthreadex(nullptr, 0, Bootstrap, this, 0, nullptr);
}

unsigned int WINAPI Core::Bootstrap(void* pThis) {
   std::cout << "Entered Core Bootstrap" << std::endl;
   Bootloader::BootstrapInjectedModule(
      std::bind(&Core::Initialize, (Core*)pThis, _1),
      ((Core*)pThis)->module_handle
   );
   std::cout << "Exit Core Bootstrap" << std::endl;
   return 0;
}

void Core::Initialize(std::shared_ptr<const bootstrap_context> context)
{
   std::cout << "At Core::Initialize with Bootstrap Context" << std::endl
             << " - Argument Flags: " << dargon::join(context->argument_flags, " ") << std::endl
             << " - Argument Properties: ";
   for (auto kvp : context->argument_properties) {
      std::cout << kvp.first << "=" << kvp.second << " ";
   }
   std::cout << std::endl;
   
   // load feature toggles:
   feature_toggles toggles;
   toggles.tasklist_enabled = dargon::contains(context->argument_flags, "--enable-dim-tasklist");
   
   // Initialize DIM Task List Manager if it's enabled
   // if (toggles.tasklist_enabled)
   // {
   //    std::cout << "DIM Task Lists are enabled - Initialize DIM Task Manager" << std::endl;
   //    task_manager = new CommandManager();
   // }

   // Initialize Dargon Subsystems
   std::cout << "Initializing Subsystems" << std::endl;
   Subsystem::OnCoreBootstrap(this, context);
   KernelSubsystem::GetInstance()->Initialize();
   FileSubsystem::GetInstance()->Initialize();

   // Initialize mods
   if(toggles.tasklist_enabled)
   {
      std::cout << "Registering DIM Task Manager Instruction Set" << std::endl;
      context->dtp_session->AddInstructionSet(task_manager->ReleaseInstructionSet());
      
      std::cout << "Querying Initial DIM Task List" << std::endl;
      auto transactionId = context->dtp_session->TakeLocallyInitializedTransactionId();
      auto handler = task_manager->ConstructInitialTaskListQueryHandler(transactionId);
      context->dtp_session->RegisterAndInitializeLITransactionHandler(*handler);

      std::cout << "Waiting for initial DIM Task List" << std::endl;
      handler->CompletionLatch.wait();
      
      std::cout << "Processing Initial DIM Task List... " << std::endl;
      auto tasks = handler->ReleaseTasks();
      task_manager->ProcessTasks(tasks);

      std::cout << "Initial DIM Task List processed." << std::endl;
   }
   
   std::cout << "Core::Initialize resuming main thread " << main_thread_handle << std::endl;

   // The thread's suspend count might be more than 1, so invoke resumethread until the thread 
   // resumes...
   while(ResumeThread(main_thread_handle) > 0);

   std::cout << "Core::Initialize end" << std::endl;
}

CommandManager* Core::GetTaskManager()
{
   return task_manager;
}