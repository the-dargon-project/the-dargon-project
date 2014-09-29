#include <iostream>

#include "DSPExNode.hpp"
#include "DSPExNodeSession.hpp"
using namespace Dargon::IO::DSP;

DSPExNode::DSPExNode(DSPExNodeRole role, std::string defaultPipeName) : m_role(role), m_pipeName(defaultPipeName)
{
   if (m_role == DSPExNodeRole::Server)
   {
      std::cout << "Warning: DSPExNodeRole::Server not yet implemented!" << std::endl;
   }
}

DSPExNodeSession* DSPExNode::Connect(std::string pipeName)
{
   auto session = new DSPExNodeSession(this);
   while(!session->ConnectLocal(pipeName))
      std::cout << "Failed to connect to local DSPEx Node " << pipeName << std::endl;
   return session;
}

void DSPExNode::AddInstructionSet(IDSPExInstructionSet* instructionSet)
{
   m_instructionSets.push_back(instructionSet);
}

bool DSPExNode::TryConstructRITHandler(UINT32 transactionId, DSPEx opcode, OUT DSPExRITransactionHandler** ppResult) const
{
   bool done = false;
   for (auto instructionSet : m_instructionSets)
   {
      if (instructionSet->TryConstructRITHandler(transactionId, opcode, ppResult))
         done = true;
   }
   return done;
}