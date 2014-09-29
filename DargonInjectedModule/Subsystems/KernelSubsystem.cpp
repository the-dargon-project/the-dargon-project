#include "stdafx.h"
#include <boost/algorithm/string.hpp>
#include <boost/signals2.hpp>
#include "Util.hpp"
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "KernelSubsystem.hpp"
#include "KernelSubsystemTypedefs.hpp"

using namespace Dargon::Subsystems;

// - singleton ------------------------------------------------------------------------------------
KernelSubsystem* KernelSubsystem::s_instance = nullptr;
KernelSubsystem* KernelSubsystem::GetInstance()
{
   if(s_instance == nullptr)
      s_instance = new KernelSubsystem();
   return s_instance;
}

// - instance -------------------------------------------------------------------------------------
KernelSubsystem::KernelSubsystem()
{
}

bool KernelSubsystem::Initialize()
{
   if(Subsystem::Initialize())
   {
      HMODULE hModuleKernel32 = WaitForModuleHandle("Kernel32.dll");
      InstallCreateProcessADetour(hModuleKernel32);
      return true;
   }
}

bool KernelSubsystem::Uninitialize()
{
   if (Subsystem::Uninitialize()) {

   } else {
      return true;
   }
}

// - static ---------------------------------------------------------------------------------------
DIM_IMPL_STATIC_DETOUR(KernelSubsystem, CreateProcessA, FunctionCreateProcessA, "CreateProcessA", MyCreateProcessA);

BOOL WINAPI KernelSubsystem::MyCreateProcessA(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                              LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags,
                                              LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo,
                                              LPPROCESS_INFORMATION lpProcessInformation)
{
   s_logger->Log(LL_ALWAYS, [&](std::ostream& os){ 
         os << "Detour CreateProcessA:" 
            << " lpApplicationName: " << lpApplicationName << " lpCommandLine: " << lpCommandLine
            << " lpProcessAttributes: " << lpProcessAttributes << " lpThreadAttributes: " << lpThreadAttributes
            << " bInheritHandles: " << bInheritHandles << " dwCreationFlags: " << dwCreationFlags
            << " lpEnvironment: " << lpEnvironment << " lpCurrentDirectory: " << lpCurrentDirectory
            << " lpStartupInfo: " << lpStartupInfo << " lpProcessInformation: " << lpProcessInformation << std::endl; 
      }
   );

   if(ShouldSuspendProcess(lpApplicationName))
      dwCreationFlags |= CREATE_SUSPENDED;

   auto result = m_trampCreateProcessA(
      lpApplicationName, lpCommandLine, lpProcessAttributes,
      lpThreadAttributes, bInheritHandles, dwCreationFlags, 
      lpEnvironment, lpCurrentDirectory, lpStartupInfo, 
      lpProcessInformation
   );
   return result;
}

bool KernelSubsystem::ShouldSuspendProcess(const char* path)
{
   auto processName = GetFileName(std::string(path));
   for(auto property : s_bootstrapContext->ArgumentProperties)
   {
      if(boost::iequals(property.first, "launchsuspended"))
      {
         std::cout << "KernelSubsystem::ShouldSuspendProcess have launchsuspended property with value " << property.second << std::endl;

         auto fileNames = split(property.second, ',');

         for (auto fileName : fileNames) {
            std::cout << "KernelSubsystem::ShouldSuspendProcess Compare Process Name " << fileName << " to " << property.second << std::endl;

            if (boost::iequals(processName, fileName))
               return true;
         }
      }
   }
   return false;
}