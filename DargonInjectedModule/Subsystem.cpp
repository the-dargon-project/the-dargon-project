#include "stdafx.h"
#include "Subsystem.hpp"
#include "Init/bootstrap_context.hpp"
#include "logger.hpp"

using namespace dargon;
using namespace dargon::IO::DIM;

dargon::logger* Subsystem::s_logger;
std::shared_ptr<const dargon::Init::bootstrap_context> Subsystem::s_bootstrap_context;
std::unordered_set<Subsystem*> Subsystem::s_subsystems;
const std::unordered_set<Subsystem*>& Subsystem::Subsystems(Subsystem::s_subsystems);

void Subsystem::Initialize(std::shared_ptr<const dargon::Init::bootstrap_context> bootstrap_context)
{
   s_bootstrap_context = bootstrap_context;
   s_logger = bootstrap_context->logger;
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