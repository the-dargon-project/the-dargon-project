#pragma once

#include <windef.h>
#include <memory>
#include "../Util/Logger.hpp"
#include "../Util/noncopyable.hpp"
#include "../IO/IoProxy.hpp"

namespace dargon { namespace IO { namespace DSP {
   class DSPExNode;
   class DSPExNodeSession;
} } }

namespace dargon { namespace Init {
   struct BootstrapContext 
   {
   public:
      HMODULE ApplicationModuleHandle;
      dargon::Util::ILogger* Logger;
      dargon::IO::DSP::DSPExNode* DSPExNode;
      dargon::IO::DSP::DSPExNodeSession* DIMSession;

      std::vector<std::string> ArgumentFlags;
      std::vector<std::pair<std::string, std::string>> ArgumentProperties;
      std::shared_ptr<dargon::IO::IoProxy> IoProxy;
   };
} }