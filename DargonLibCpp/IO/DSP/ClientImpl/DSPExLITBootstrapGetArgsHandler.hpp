#pragma once 

#include <string>
#include <memory>

#include "../../../dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExLITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP { namespace ClientImpl {
   class DSPExLITBootstrapGetArgsHandler : public DSPExLITransactionHandler
   {
   public:
      DSPExLITBootstrapGetArgsHandler(UINT32 transactionId);
      void InitializeInteraction(IDSPExSession& session);
      void ProcessMessage(IDSPExSession& session, DSPExMessage& message);

      // m_flags and m_properties are moved by DSPExClient
      std::vector<std::string> m_flags;
      std::vector<std::pair<std::string, std::string>> m_properties;
   };
} } } }