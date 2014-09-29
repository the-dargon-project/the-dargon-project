#pragma once

#include <windef.h>
#include <functional>
#include <boost/utility.hpp>
#include "BootstrapContext.hpp"

namespace Dargon { namespace Init {
   typedef std::function<void(const BootstrapContext*)> FunctionInitialize;

   class Bootloader : boost::noncopyable
   {
   public:
      static void BootstrapInjectedModule(const FunctionInitialize& init, HMODULE moduleHandle);
   };
} } //Namespaces