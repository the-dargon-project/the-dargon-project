#include "stdafx.h"
#include "RemappedFileOperationProxyFactory.hpp"
#include "RemappedFileOperationProxy.hpp"

using namespace dargon::Subsystems;

RemappedFileOperationProxyFactory::RemappedFileOperationProxyFactory(
   std::shared_ptr<dargon::IO::IoProxy> io_proxy, 
   std::shared_ptr<dargon::vfm_file> virtual_file_map
) : io_proxy(io_proxy), virtual_file_map(virtual_file_map) {
}

std::shared_ptr<FileOperationProxy> RemappedFileOperationProxyFactory::create() {
   return std::make_shared<RemappedFileOperationProxy>(io_proxy, virtual_file_map);
}
