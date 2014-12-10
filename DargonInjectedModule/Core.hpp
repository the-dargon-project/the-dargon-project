#pragma once
#include "stdafx.h"
#include <memory>
#include <vector>
#include <Windows.h>
#include "Init/BootstrapContext.hpp"
#include "IO/DIM/DIMTaskManager.hpp"

namespace dargon { namespace InjectedModule {
   class Core 
   {
   private:
      HMODULE m_moduleHandle;
      HANDLE m_mainThreadHandle;
      dargon::IO::DIM::DIMTaskManager* m_pDIMTaskManager;

   public:
      // The common entry point reached when entering through DllMain or main.
      // hModule: Our injected DLL's module handle (also its base address...)
      // Sets m_mainThreadHandle and m_moduleHandle
      Core(HMODULE hModule);

   private:
      // Begins the Dargon bootstrap operation.
      static unsigned int WINAPI Bootstrap(void* pThis);

      // Initializes the DIM Core after the bootloader initializes basic necessities such as 
      // loggers, Dargon Service Protocol Ex Client, etc.
      void Initialize(const dargon::Init::BootstrapContext* context);

   public:
      // Gets the DIMTaskManager, or null if it doesn't exist
      dargon::IO::DIM::DIMTaskManager* GetDIMTaskManager();
   };
} }