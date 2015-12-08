#include "stdafx.h"
#include <Shlobj.h>
#include <Shlwapi.h>
#include <Windows.h>
#include <fstream>
#include <sstream>
#include <iomanip>
#include "ThirdParty/guicon.h"
#include "Application.hpp"
#include "SystemState.hpp"
#include "util.hpp"
using namespace dargon;

/// <summary>
/// Entry point of our application, the component of Dargon that is executed in the
/// memory of League of Legends.exe.  From here, we simply pass control to
/// the injected module's core class.
/// </summary>
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
   switch (ul_reason_for_call)
   {
      case DLL_PROCESS_ATTACH:
      {
         DisableThreadLibraryCalls(hModule);
         if (CheckFeatureToggle(L"enable-trinket-dim-console")) {
            RedirectIOToConsole();
            dargon::logger::isLoggingEnabled = true;
         }

         auto targetProcessModuleHandle = GetModuleHandleW(L"d3dx9_39.dll");
         std::cout << targetProcessModuleHandle << " and " << GetProcAddress(targetProcessModuleHandle, "D3DXCreateTextureFromResourceA") << std::endl;
         std::cout << targetProcessModuleHandle << " and " << GetProcAddress(targetProcessModuleHandle, "D3DXCreateTextureFromResourceW") << std::endl;
         std::cout << targetProcessModuleHandle << " and " << GetProcAddress(targetProcessModuleHandle, "D3DXCreateTextureFromResourceExA") << std::endl;
         std::cout << targetProcessModuleHandle << " and " << GetProcAddress(targetProcessModuleHandle, "D3DXCreateTextureFromResourceExW") << std::endl;

         if (CheckFeatureToggle(L"enable-trinket-pause-after-injection")) {
            std::cout << "Pausing after injection for 5 seconds..." << std::endl;
            Sleep(5000);
         }
         Application::HandleDllEntry(hModule);
      }
      case DLL_PROCESS_DETACH:
      {
         std::cout << "ENTERED DLL PROCESS DETACH!" << std::endl;
         Application::HandleDllUnload();
      }
   }
   return true; //We're all winners; false would denote a failed initialization.
}