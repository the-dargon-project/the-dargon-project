#include "../DSP/DSPEx.hpp"
#include "../DSP/DSPExInitialMessage.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"

using namespace Dargon::IO::DSP;
using namespace Dargon::IO::DIM;

DSPExLITDIMQueryInitialTaskListHandler::DSPExLITDIMQueryInitialTaskListHandler(UINT32 transactionId)
   : DSPExLITransactionHandler(transactionId)
{
}

void DSPExLITDIMQueryInitialTaskListHandler::InitializeInteraction(Dargon::IO::DSP::IDSPExSession& session)
{
   session.SendMessage(
      DSPExInitialMessage(
         TransactionId,
         DSP_EX_C2S_DIM_READY_FOR_TASKS,
         nullptr,
         0
      )
   );
   std::cout << "Sent DSP_EX_C2S_DIM_READY_FOR_TASKS Message!" << std::endl;
}