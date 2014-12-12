#pragma once

#include "stdafx.h"
#include <memory>
#include "IO/IoProxy.hpp"
#include "vfm/vfm_file.hpp"
#include "FileOperationProxy.hpp"
#include "FileOperationProxyFactory.hpp"

namespace dargon {
   namespace Subsystems {
      class RemappedFileOperationProxyFactory : public FileOperationProxyFactory, dargon::noncopyable {
         std::shared_ptr<dargon::IO::IoProxy> io_proxy;
         std::shared_ptr<dargon::vfm_file> virtual_file_map;

      public:
         RemappedFileOperationProxyFactory(std::shared_ptr<dargon::IO::IoProxy> io_proxy, std::shared_ptr<dargon::vfm_file> virtual_file_map);
         std::shared_ptr<FileOperationProxy> create() override;
      };
   }
}