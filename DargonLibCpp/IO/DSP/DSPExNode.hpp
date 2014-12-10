#pragma once

#include <string>
#include <vector>

#include "../../Util/noncopyable.hpp"
#include "../IoProxy.hpp"
#include "DSPExTypeDefs.hpp"
#include "IDSPExSession.hpp"
#include "IDSPExInstructionSet.hpp"
#include "DSPExNodeSession.hpp"
#include "DSPExRITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP {
   class DSPExNode : dargon::Util::noncopyable {   
   private:
      const DSPExNodeRole m_role;
      const std::string m_pipeName;

      std::vector<IDSPExInstructionSet*> m_instructionSets;
      std::shared_ptr<dargon::IO::IoProxy> ioProxy;

   public:
      DSPExNode(DSPExNodeRole role, std::string defaultPipeName, std::shared_ptr<dargon::IO::IoProxy> ioProxy);
      DSPExNodeSession* Connect(const std::string& pipeName = "Dargon");
      void AddInstructionSet(IDSPExInstructionSet* instructionSet);
      bool TryConstructRITHandler(UINT32 transactionid, DSPEx opcode, OUT DSPExRITransactionHandler** ppResult) const;
   };
} } }