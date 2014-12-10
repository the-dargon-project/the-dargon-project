#pragma once

#include <windef.h>
#include <memory>
#include "file_logger.hpp"
#include "noncopyable.hpp"
#include "IO/IoProxy.hpp"

namespace dargon { namespace IO { namespace DSP {
   class DSPExNode;
   class DSPExNodeSession;
} } }

namespace dargon { namespace Init {
   struct bootstrap_context {
      HMODULE module_handle;
      dargon::logger* logger;
      dargon::IO::DSP::DSPExNode* dtp_node;
      dargon::IO::DSP::DSPExNodeSession* dtp_session;
      std::shared_ptr<dargon::IO::IoProxy> io_proxy;
      std::vector<std::string> argument_flags;
      std::vector<std::pair<std::string, std::string>> argument_properties;
   };
}}