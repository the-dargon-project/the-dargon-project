#include "stdafx.h"
#include <Windows.h>
#include "ThirdParty/guicon.h"
#include <iomanip>
#include "Application.hpp"
using namespace std;
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
         RedirectIOToConsole();
         Application::HandleDllEntry(hModule);
      }
   }
   return true; //We're all winners; false would denote a failed initialization.
}