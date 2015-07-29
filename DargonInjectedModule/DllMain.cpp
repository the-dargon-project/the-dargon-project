#include "stdafx.h"
#include <Shlobj.h>
#include <Windows.h>
#include <sstream>
#include <iomanip>
#include "ThirdParty/guicon.h"
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
//         WCHAR path[MAX_PATH];
//         if (::SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, 0, path) == S_OK) {
//            std::wstringstream ss;
//            ss << path << ".dargon\configuration\system-state\enable-trinket-dim-console";
//            auto hFile = CreateFileW(ss.str().c_str(), GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
//            if (hFile != INVALID_HANDLE_VALUE) {
//               int8_t buffer = 0;
//               DWORD bytesRead = 0;
//               while (bytesRead == 0) {
//                  ReadFile(hFile, &buffer, 1, &bytesRead, nullptr);
//               }
//               if (buffer == '1') {
                  RedirectIOToConsole();
//               }
//            }
//            CloseHandle(hFile);
//         }
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