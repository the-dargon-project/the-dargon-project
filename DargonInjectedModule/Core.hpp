#pragma once
#include "stdafx.h"
#include <memory>
#include <vector>
#include <Windows.h>
#include "Init/bootstrap_context.hpp"
#include "IO/DIM/CommandManager.hpp"

namespace dargon { namespace InjectedModule {
   class Core 
   {
   private:
      HMODULE module_handle;
      HANDLE main_thread_handle;
      dargon::IO::DIM::CommandManager* task_manager;

   public:
      // The common entry point reached when entering through DllMain or main.
      // hModule: Our injected DLL's module handle (also its base address...)
      // Sets m_mainThreadHandle and m_moduleHandle
      Core(HMODULE hModule);

   private:
      // Begins the Dargon bootstrap operation.
      static unsigned int WINAPI Bootstrap(void* pThis);

      // Initializes the DIM Core after the bootloader initializes basic necessities such as 
      // file_loggers, Dargon Service Protocol Ex Client, etc.
      void Initialize(std::shared_ptr<const dargon::Init::bootstrap_context> context);

   public:
      // Gets the CommandManager, or null if it doesn't exist
      dargon::IO::DIM::CommandManager* GetTaskManager();
   };
} }