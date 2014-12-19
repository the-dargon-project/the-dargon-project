#pragma once

#include "base.hpp"

namespace dargon { namespace IO {
   enum class FileAccess : uint32_t {
      Read              = (0x80000000L),
      Write             = (0x40000000L),
      ReadWrite         = Read | Write
   };
   
   enum class FileShare : uint32_t {
      Read              = (0x00000001L),
      Write             = (0x00000002L),
      ReadWrite         = Read | Write,
      None              = 0
   };
   
   enum class CreationDisposition : uint32_t {
      CreateAlways      = 2,
      CreateNew         = 1,
      OpenAlways        = 4,
      OpenExisting      = 3,
      TruncateExisting  = 5
   };
} }