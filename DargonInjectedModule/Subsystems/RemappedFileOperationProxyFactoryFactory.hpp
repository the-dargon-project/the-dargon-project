#pragma once
#include "stdafx.h"
#include "IO/IoProxy.hpp"
#include "vfm/vfm_reader.hpp"
#include "RemappedFileOperationProxyFactory.hpp"

namespace dargon {
   namespace Subsystems {
      class RemappedFileOperationProxyFactoryFactory {
         std::shared_ptr<dargon::IO::IoProxy> io_proxy;
         std::shared_ptr<dargon::vfm_reader> virtual_file_map_reader;

      public:
         RemappedFileOperationProxyFactoryFactory(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::shared_ptr<dargon::vfm_reader> virtual_file_map_reader);
         std::shared_ptr<RemappedFileOperationProxyFactory> create(std::string path);
      };
   }
}