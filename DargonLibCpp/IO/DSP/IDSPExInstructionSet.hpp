#pragma once

#include "DSPEx.hpp"
#include "DSPExRITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP {
   class IDSPExInstructionSet
   {
   public:
      virtual ~IDSPExInstructionSet() {}
      virtual bool TryConstructRITHandler(UINT32 transactionId, DSPEx opcode, OUT DSPExRITransactionHandler** ppResult) = 0;
   };
} } }