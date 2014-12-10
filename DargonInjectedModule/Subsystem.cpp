#include "stdafx.h"
#include "Subsystem.hpp"
#include "Init/BootstrapContext.hpp"
#include "Util/ILogger.hpp"

using namespace dargon;
using namespace dargon::InjectedModule;
using namespace dargon::IO::DIM;

dargon::Util::ILogger* Subsystem::s_logger;
const dargon::Init::BootstrapContext* Subsystem::s_bootstrapContext;
Core* Subsystem::s_core;
std::unordered_set<Subsystem*> Subsystem::s_subsystems;
const std::unordered_set<Subsystem*>& Subsystem::Subsystems(Subsystem::s_subsystems);

void Subsystem::OnCoreBootstrap(Core* core, const dargon::Init::BootstrapContext* bootstrapContext)
{
   s_core = core;
   s_bootstrapContext = bootstrapContext;
   s_logger = bootstrapContext->Logger;
}

DWORD* Subsystem::GetVTablePointer(void* pObject)
{
   return ((DWORD**)pObject)[0];
}

HMODULE Subsystem::WaitForModuleHandle(const char* moduleName)
{
   std::cout << "Waiting for " << moduleName << " module load" << std::endl;
   HMODULE hModule;
   while((hModule = GetModuleHandleA(moduleName)) == NULL) Sleep(10);
   std::cout << "The " << moduleName << " module loaded.  hModule: " << hModule << std::endl;
   return hModule;
}

// - Instance Methods -----------------------------------------------------------------------------
Subsystem::Subsystem()
   : m_initialized(false)
{
   s_subsystems.insert(this);
}

Subsystem::~Subsystem()
{
   s_subsystems.erase(this);
}

void Subsystem::AddTaskHandler(IDIMTaskHandler* handler)
{
   m_taskHandlers.insert(handler);
}

bool Subsystem::Initialize()
{
   m_initialized = true;
   return true;
}

bool Subsystem::Uninitialize()
{
   m_initialized = false;
   return true;
}

bool Subsystem::IsInitialized()
{
   return m_initialized;
}