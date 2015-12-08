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

bool CheckFeatureToggle(std::wstring name) {
   bool result = false;
   WCHAR userHomePath[MAX_PATH];
   if (SUCCEEDED(::SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, 0, userHomePath))) {
      std::wstringstream ss;
      ss << userHomePath << L"/.dargon/configuration/system-state/" << name;

      std::fstream fs(ss.str().c_str(), std::fstream::in);
      if (fs.good()) {
         std::string token;
         fs >> token;
         if (!fs.fail() && dargon::iequals(token, "True")) {
            result = true;
         }
      }
      fs.close();
   }
   return result;
}

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