#pragma once

namespace dargon { namespace Subsystems {
   class FileOperationProxyFactory {
   public:
#ifdef WIN32
      virtual std::shared_ptr<FileOperationProxy> create() = 0;
#endif
   };
} }