#pragma once

#include <string>
#include <vector>
#include <boost/noncopyable.hpp>

#include "DSPExTypeDefs.hpp"
#include "IDSPExSession.hpp"
#include "IDSPExInstructionSet.hpp"
#include "DSPExNodeSession.hpp"
#include "DSPExRITransactionHandler.hpp"

namespace Dargon { namespace IO { namespace DSP {
   class DSPExNode : boost::noncopyable {   
   private:
      const DSPExNodeRole m_role;
      const std::string m_pipeName;

      std::vector<IDSPExInstructionSet*> m_instructionSets;

   public:
      DSPExNode(DSPExNodeRole role, std::string defaultPipeName);
      DSPExNodeSession* Connect(std::string pipeName = "Dargon");
      void AddInstructionSet(IDSPExInstructionSet* instructionSet);
      bool TryConstructRITHandler(UINT32 transactionid, DSPEx opcode, OUT DSPExRITransactionHandler** ppResult) const;
   };
} } }