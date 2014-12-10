#pragma once 

#include "dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExLITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP { namespace ClientImpl {
   class DSPExLITEchoHandler : public DSPExLITransactionHandler
   {
   public:
      bool const& ResponseDataMatched;
      DSPExLITEchoHandler(UINT32 transactionId, BYTE* data, UINT32 dataLength);
      void InitializeInteraction(IDSPExSession& session);
      void ProcessMessage(IDSPExSession& session, DSPExMessage& message);

   private:
      bool m_responseDataMatched;
      BYTE* m_data;
      UINT32 m_dataLength;
   };
} } } }