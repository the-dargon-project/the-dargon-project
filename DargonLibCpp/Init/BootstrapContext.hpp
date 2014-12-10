#pragma once

#include <windef.h>
#include <memory>
#include "../Util/Logger.hpp"
#include "../Util/noncopyable.hpp"
#include "../IO/IoProxy.hpp"

namespace Dargon { namespace IO { namespace DSP {
   class DSPExNode;
   class DSPExNodeSession;
} } }

namespace Dargon { namespace Init {
   struct BootstrapContext 
   {
   public:
      HMODULE ApplicationModuleHandle;
      Dargon::Util::ILogger* Logger;
      Dargon::IO::DSP::DSPExNode* DSPExNode;
      Dargon::IO::DSP::DSPExNodeSession* DIMSession;

      std::vector<std::string> ArgumentFlags;
      std::vector<std::pair<std::string, std::string>> ArgumentProperties;
      std::shared_ptr<Dargon::IO::IoProxy> IoProxy;
   };
} }