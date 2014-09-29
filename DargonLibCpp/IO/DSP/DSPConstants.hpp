#pragma once 

#include "../../Dargon.hpp"

namespace Dargon { namespace IO { namespace DSP {
   class DSPConstants
   {
   public:
      static const INT32 kMaxMessageSize = 20000;
      static const UINT32 kInvalidNodeId  = 0xFFFFFFFFU;
   };
} } }