#pragma once 

#include "dargon.hpp"
#include "util.hpp"

namespace dargon { namespace IO { namespace DSP {
   /// <summary>
   /// DSP Interactions begin with a DSPExInitialMessage message being sent.  This message will
   /// contain opcode information, allowing the DSP Server or Client to use the appropriate
   /// interaction handler.
   /// 
   /// This class intentionally does not implement DSPExMessage, so that method overloading can
   /// be used.
   /// </summary>
   class DSPExInitialMessage
   {
   public:
      /// <summary>
      /// The transaction ID of our DSPEx Message
      /// </summary>
      const UINT32 TransactionId;

      /// <summary>
      /// The opcode which initiated our DSPEx interaction
      /// </summary>
      const UINT32 Opcode;

      /// <summary>
      /// The data contained within our DSPEx message
      /// </summary>
      const BYTE* DataBuffer;

      /// <summary>
      /// The length of our data in the data buffer
      /// </summary>
      const INT32 DataLength;

      /// <summary>
      /// Creates a new instance of a DSPEx message
      /// </summary>
      /// <param name="transactionId">The transaction ID of our DSPEx message</param>
      /// <param name="opcode">The opcode of our DSPEx message</param>
      /// <param name="data">Some buffer</param>
      /// <param name="offset">
      /// The offset in the parameter buffer for our data
      /// </param>
      /// <param name="length">
      /// The length of our data
      /// </param>
      DSPExInitialMessage(UINT32 transactionId, UINT32 opcode, const BYTE* data, INT32 length)
         : TransactionId(transactionId), Opcode(opcode), DataBuffer(data), DataLength(length){}
   };
} } }