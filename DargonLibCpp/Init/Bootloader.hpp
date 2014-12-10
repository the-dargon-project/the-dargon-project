#pragma once

#include <windef.h>
#include <functional>
#include "BootstrapContext.hpp"

namespace Dargon { namespace Init {
   typedef std::function<void(const BootstrapContext*)> FunctionInitialize;

   class Bootloader : Dargon::Util::noncopyable {
   public:
      static void BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle);
   };
} } 