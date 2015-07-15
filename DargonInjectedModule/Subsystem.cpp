#include "stdafx.h"
#include <algorithm>
#include "Subsystem.hpp"
#include "Init/bootstrap_context.hpp"
#include "logger.hpp"
#include "Configuration.hpp"

using namespace dargon;
using namespace dargon::IO::DIM;

std::shared_ptr<dargon::logger> Subsystem::s_logger;
std::shared_ptr<const dargon::Init::bootstrap_context> Subsystem::s_bootstrap_context;
std::shared_ptr<Configuration> Subsystem::s_configuration;

void Subsystem::Initialize(
   std::shared_ptr<const dargon::Init::bootstrap_context> bootstrap_context,
   std::shared_ptr<Configuration> configuration,
   std::shared_ptr<dargon::logger> logger
) {
   s_bootstrap_context = bootstrap_context;
   s_configuration = configuration;
   s_logger = logger;
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
Subsystem::Subsystem() : m_initialized(false) { }

Subsystem::~Subsystem() { }

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