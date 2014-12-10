#pragma once

#include <windef.h>
#include <functional>
#include "BootstrapContext.hpp"

namespace dargon { namespace Init {
   typedef std::function<void(const BootstrapContext*)> FunctionInitialize;

   class Bootloader : dargon::Util::noncopyable {
   public:
      static void BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle);
   };
} } 