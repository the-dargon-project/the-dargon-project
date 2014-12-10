#include "stdafx.h"
#include <string>
#include <iostream>
#include <unordered_set>
#include <fcntl.h>
#include <io.h>
#include <stdio.h>
#include <Windows.h>

#include <boost/algorithm/string.hpp>
#include <boost/nowide/iostream.hpp>
#include <boost/nowide/convert.hpp>

#include "Init/Bootloader.hpp"
#include <IO/DIM/DIMTask.hpp>
#include <IO/DIM/IDIMTaskHandler.hpp>
#include <IO/DIM/DIMTaskTypes.hpp>
#include "IO/DSP/DSPExNodeSession.hpp"
#include "Util.hpp"
#include <file_logger.hpp>

#include "Core.hpp"
#include "Subsystem.hpp"
#include "Subsystems/KernelSubsystem.hpp"
#include "Subsystems/FileSubsystem.hpp"

#include "ThirdParty/guicon.h"

using namespace dargon::InjectedModule;
using namespace dargon::Init;
using namespace dargon;
using namespace std::placeholders;
using namespace dargon::Subsystems;
using namespace dargon::IO::DSP::ClientImpl;
using namespace dargon::IO::DIM;

Core::Core(HMODULE hModule) : m_pDIMTaskManager(nullptr)
{
   m_moduleHandle = hModule;
   file_logger::Initialize("C:/DargonLog.log");

   std::cout << "Entered Core::Core, suspending main thread" << std::endl;
   m_mainThreadHandle = OpenMainThread();
   std::cout << " - Thread ID " << m_mainThreadHandle << std::endl;
   SuspendThread(m_mainThreadHandle);

   std::cout << "Creating Dargon Bootstrap thread" << std::endl;
   _beginthreadex(
      nullptr,
      0,
      Bootstrap,
      this,
      0,
      nullptr
   );
}

unsigned int WINAPI Core::Bootstrap(void* pThis)
{
   std::cout << "Entered Core Bootstrap" << std::endl;
   Bootloader::BootstrapInjectedModule(
      std::bind(&Core::Initialize, (Core*)pThis, _1),
      ((Core*)pThis)->m_moduleHandle
   );
   std::cout << "Exit Core Bootstrap" << std::endl;
   return 0;
}

void Core::Initialize(const BootstrapContext* context)
{
   std::cout << "At Core::Initialize with Bootstrap Context" << std::endl
             << " - Argument Flags: " << boost::algorithm::join(context->ArgumentFlags, " ") << std::endl
             << " - Argument Properties: ";
   for(auto kvp : context->ArgumentProperties)
      std::cout << kvp.first << "=" << kvp.second << " ";
   std::cout << std::endl;
   
   bool tasksEnabled = std::find(context->ArgumentFlags.begin(), context->ArgumentFlags.end(), "--enable-dim-tasklist") != context->ArgumentFlags.end();
   
   // Initialize DIM Task List Manager if it's enabled
   if (tasksEnabled)
   {
      std::cout << "DIM Task Lists are enabled - Initialize DIM Task Manager" << std::endl;
      m_pDIMTaskManager = new DIMTaskManager();
   }

   // Initialize Dargon Subsystems
   std::cout << "Initializing Subsystems" << std::endl;
   Subsystem::OnCoreBootstrap(this, context);
   KernelSubsystem::GetInstance()->Initialize();
   FileSubsystem::GetInstance()->Initialize();

   // Initialize mods
   if(tasksEnabled)
   {
      std::cout << "Registering DIM Task Manager Instruction Set" << std::endl;
      context->DIMSession->AddInstructionSet(m_pDIMTaskManager->ReleaseInstructionSet());
      
      std::cout << "Querying Initial DIM Task List" << std::endl;
      auto transactionId = context->DIMSession->TakeLocallyInitializedTransactionId();
      auto handler = m_pDIMTaskManager->ConstructInitialTaskListQueryHandler(transactionId);
      context->DIMSession->RegisterAndInitializeLITransactionHandler(*handler);

      std::cout << "Waiting for initial DIM Task List" << std::endl;
      handler->CompletionLatch.wait();
      
      std::cout << "Processing Initial DIM Task List... " << std::endl;
      auto tasks = handler->ReleaseTasks();
      m_pDIMTaskManager->ProcessTasks(tasks);

      std::cout << "Initial DIM Task List processed." << std::endl;
   }
   
   std::cout << "Core::Initialize resuming main thread " << m_mainThreadHandle << std::endl;

   // The thread's suspend count might be more than 1, so invoke resumethread until the thread 
   // resumes...
   while(ResumeThread(m_mainThreadHandle) > 0);

   std::cout << "Core::Initialize end" << std::endl;
}

DIMTaskManager* Core::GetDIMTaskManager()
{
   return m_pDIMTaskManager;
}