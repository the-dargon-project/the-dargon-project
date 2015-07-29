#include "stdafx.h"
#include <Shlobj.h>
#include <Shlwapi.h>
#include <Windows.h>
#include <fstream>
#include <sstream>
#include <iomanip>
#include "ThirdParty/guicon.h"
#include "Application.hpp"
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
         WCHAR userHomePath[MAX_PATH];
         if (SUCCEEDED(::SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, 0, userHomePath))) {
            WCHAR enableTrinketDimConsoleToggleAbsolutePath[MAX_PATH];
            auto enableTrinketDimConsoleToggleRelativePath = L".dargon/configuration/system-state/enable-trinket-dim-console";
            ::PathCombine(enableTrinketDimConsoleToggleAbsolutePath, userHomePath, enableTrinketDimConsoleToggleRelativePath);
            std::fstream fs(enableTrinketDimConsoleToggleAbsolutePath, std::fstream::in);
            if (fs.good()) {
               std::string token;
               fs >> token;
               if (!fs.fail() && dargon::iequals(token, "True")) {
                  RedirectIOToConsole();
                  dargon::logger::isLoggingEnabled = true;
               }
            }
            fs.close();
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