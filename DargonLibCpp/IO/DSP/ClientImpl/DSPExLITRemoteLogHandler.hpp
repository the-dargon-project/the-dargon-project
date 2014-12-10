#pragma once 

#include "dlc_pch.hpp"
#include <string>
#include "../../../Dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExLITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP { namespace ClientImpl {
   class DSPExLITRemoteLogHandler : public DSPExLITransactionHandler
   {
      UINT32 m_loggerLevel;
      std::string m_message;

   public:
      DSPExLITRemoteLogHandler(UINT32 transactionId, UINT32 loggerLevel, std::string message);
      void InitializeInteraction(IDSPExSession& session);
      void ProcessMessage(IDSPExSession& session, DSPExMessage& message);
   };
} } } }