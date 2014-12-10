#pragma once

#include <windef.h>
#include <functional>
#include <memory>
#include "bootstrap_context.hpp"

namespace dargon { namespace Init {
   typedef std::function<void(std::shared_ptr<const bootstrap_context>)> FunctionInitialize;

   class Bootloader : dargon::noncopyable {
   public:
      static void BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle);
   };
} } 