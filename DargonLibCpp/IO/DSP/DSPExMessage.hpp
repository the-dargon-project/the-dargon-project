#pragma once 

#include "../../Dargon.hpp"

namespace Dargon { namespace IO { namespace DSP {
   /// <summary>
   /// After the initial DSPExInitialMessage message is sent, DSPExMessages are sent.  These
   /// Messages just contain transaction id and FileTree.
   /// </summary>
   class DSPExMessage
   {
   public:
      /// <summary>
      /// The transaction ID of our DSPEx Message
      /// </summary>
      const UINT32 TransactionId;
   
      /// <summary>
      /// The payload of the DSPEx message. The data here is owned by the DSPExClient, and the 
      /// pointer is only valid while the TransactionHandler's processmessage() method is invoked.
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
      /// <param name="data">Some buffer</param>
      /// <param name="length">
      /// The length of our data
      /// </param>
      DSPExMessage(UINT32 transactionId, const BYTE* data, INT32 length)
         : TransactionId(transactionId), DataBuffer(data), DataLength(length) { }
   };
} } }