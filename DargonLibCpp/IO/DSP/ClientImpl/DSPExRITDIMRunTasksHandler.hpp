#pragma once

#include "../../../dlc_pch.hpp"
#include "../../../Dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExRITransactionHandler.hpp"

namespace Dargon { namespace IO { namespace DSP { namespace ClientImpl {
   class DSPExRITDIMRunTasksHandler : public DSPExRITransactionHandler
   {
   public:
      DSPExRITDIMRunTasksHandler(UINT32 transactionId);
      void ProcessInitialMessage(IDSPExSession& session, DSPExInitialMessage& message);
      void ProcessMessage(IDSPExSession& session, DSPExMessage& message);
   };
} } } }