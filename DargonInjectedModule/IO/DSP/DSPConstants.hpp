#pragma once 

#include "dargon.hpp"

namespace dargon { namespace IO { namespace DSP {
   class DSPConstants
   {
   public:
      static const INT32 kMaxMessageSize = 20000;
      static const UINT32 kInvalidNodeId  = 0xFFFFFFFFU;
   };
} } }