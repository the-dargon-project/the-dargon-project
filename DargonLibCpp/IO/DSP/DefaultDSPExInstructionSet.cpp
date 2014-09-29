#include "DSPEx.hpp"
#include "ClientImpl/DSPExRITEchoHandler.hpp"
#include "DefaultDSPExInstructionSet.hpp"

using namespace Dargon::IO::DSP;
using namespace Dargon::IO::DSP::ClientImpl;

bool DefaultDSPExInstructionSet::TryConstructRITHandler(UINT32 transactionId, DSPEx opcode, OUT DSPExRITransactionHandler** ppResult) 
{
   switch (opcode)
   {
      case DSP_EX_ECHO: *ppResult = new DSPExRITEchoHandler(transactionId); return true;
   }
   return false;
}