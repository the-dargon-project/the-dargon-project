#pragma once 

#include <unordered_map>
#include <mutex>
#include <iostream>
#include <thread>
#include <deque>
#include "../../Init/BootstrapContext.hpp"
#include "../../Util.hpp"
#include "../../noncopyable.hpp"
#include "../IPCObject.hpp"
#include "../IoProxy.hpp"
#include "IDSPExFrameTransmitter.hpp"
#include "DSPExLITransactionHandler.hpp"
#include "DSPEXRITransactionHandler.hpp"
#include "IDSPExSession.hpp"
#include "DSPExNode.hpp"
#include "IDSPExInstructionSet.hpp"

#include "IO/DIM/DIMTask.hpp"
#include "IO/DIM/IDIMTaskHandler.hpp"

// DSPExHandler Implementations
#include "ClientImpl/DSPExLITEchoHandler.hpp"
#include "ClientImpl/DSPExRITEchoHandler.hpp"
#include "ClientImpl/DSPExLITBootstrapGetArgsHandler.hpp"
#include "ClientImpl/DSPExRITDIMRunTasksHandler.hpp"

// Frame Processor
#include "DSPExFrameProcessor.hpp"

namespace dargon { namespace IO { namespace DSP {
   class DSPExFrameProcessor;

   class DSPExNodeSession : public IDSPExSession, dargon::noncopyable
   {
      typedef DSPExRITransactionHandler*(DSPExRITransactionHandlerFactory)(UINT32 transactionId);
      typedef std::unordered_map<BYTE, DSPExRITransactionHandlerFactory*> FactoryMap;
      typedef std::unordered_map<UINT32, DSPExLITransactionHandler*> LITransactionMap;
      typedef std::unordered_map<UINT32, DSPExRITransactionHandler*> RITransactionMap;

      typedef std::recursive_mutex MutexType;
      typedef std::unique_lock<MutexType> LockType;

      typedef std::uint32_t TransactionIdType;

      /// <summary>
      /// Whether or not debug logging is enabled.
      /// </summary>
      static bool kDebugEnabled;
      
      // The number of threads started to handle DSPEx frames.
      static int kFrameProcessorCount; 
      static int kFrameProcessorLimit; 

   public:
      /// <summary>
      /// Whether or not the DSP Ex Client connection is shutting down/has shut down.
      /// </summary>
      const bool &Terminated;

      /// <summary>
      /// Initializes a new instance of a DSPEx Client.  Unlike the C# implementation, the C++
      /// implementation constructor does not connect to a server; rather, you must invoke the
      /// Connect() method after constructing the client.  
      /// </summary>
      DSPExNodeSession(DSPExNode* node, std::shared_ptr<dargon::IO::IoProxy> ioProxy);
      
      /// <summary>
      /// Attempts to connect to the server at the given hostname and port.
      /// Not implemented!
      /// </summary>
      bool Connect(std::string host, UINT32 port);

      /// <summary>
      /// Attempts to connect locally with the given pipename.  If the named pipe connection fails,
      /// Dargon falls back to a TCP Socket connection.
      /// </summary>
      bool ConnectLocal(const std::string& pipeName = "Dargon");
      
   private:
      static unsigned int WINAPI StaticFrameReceivingThreadStart(void* pThis);

      /// <summary>
      /// Entry point for a frame-receiving thread.
      /// </summary>
      void FrameReceivingThreadStart();

      void AddFrameProcessor();
      
   public:
      UINT32 TakeLocallyInitializedTransactionId();

      /// <summary>
      /// Registers a locally initialized transaction handler so that future messages can be 
      /// routed to it.  This method respects the Transaction Handler's TransactionID property; 
      /// that value is not mutated by this method.
      /// </summary>
      /// <param name="th">
      /// The transaction handler which we are registering.
      /// </param>
      void RegisterAndInitializeLITransactionHandler(DSPExLITransactionHandler& th);
   
      /// <summary>
      /// Deregisters the given locally initialized transaction handler, freeing its transaction id.
      /// This method is called assuming that the transaction has reached a state where both DSP
      /// endpoints are aware of the transaction ending.  If such is not a case, this method call
      /// will result in a memory leak which will last on the other endpoint until the DSP 
      /// connection is closed.
      ///
      /// This method will free the memory of the transaction handler via delete (thus also calling its
      /// destructor).
      /// </summary>
      /// <param name="th"></param>
      void DeregisterLITransactionHandler(DSPExLITransactionHandler& th);

      /// <summary>
      /// Returns a pointer to the DSPExLIT of the given transaction id, or nullptr.
      /// </summary>
      DSPExLITransactionHandler* FindLITransactionHandler(UINT32 transactionId);
   
      /// <summary>
      /// Creates a remotely initialized transaction handler for the given opcode
      /// </summary>
      /// <param name="transactionId">
      /// Unique identifier associated with the transaction.
      /// </param>
      /// <param name="opcode">
      /// The opcode associated with the given transaction
      /// </param>
      /// <returns>
      /// The transaction handler, or null if such a transaction handler doesn't exist
      /// </returns>
      DSPExRITransactionHandler* CreateAndRegisterRITransactionHandler(UINT32 transactionId, INT32 opcode);
   
      /// <summary>
      /// Deregisters the remotely initialized transaction's handler, freeing its transaction id.
      /// This method is called assuming that the transaction has reached a state where both DSP
      /// endpoints are aware of the transaction ending.  If such is not a case, this method call
      /// will result in a memory leak which will last on the other endpoint until the DSP 
      /// connection is closed.
      /// </summary>
      /// <param name="handler"></param>
      void DeregisterRITransactionHandler(DSPExRITransactionHandler* handler);

      /// <summary>
      /// Returns a pointer to the DSPExLIT of the given transaction id, or nullptr.
      /// </summary>
      DSPExRITransactionHandler* FindRITransactionHandler(UINT32 transactionId);
   
      /// <summary>
      /// Sends a DSPEX Initial message to the remote endpoint.  Before this method is called, 
      /// DSPEx's RegisterTransaction() method must be called to modify the associated transaction's
      /// transactionId field.
      /// </summary>
      /// <param name="message">
      /// The initial message which we are sending.
      /// </param>
      void SendMessage(DSPExInitialMessage& message);
   
      /// <summary>
      /// Sends a DSPEx message.  This method must be called after DSPExInitialMessage is sent once.
      /// If this is the last DSPEx message to be sent, DeregisterTransaction() must be called by
      /// the associated transaction to free its transactionId.
      /// </summary>
      /// <param name="message">
      /// The message which we are sending.
      /// </param>
      void SendMessage(DSPExMessage& message);

      void AddInstructionSet(IDSPExInstructionSet* instructionSet);
      
      /// <summary>
      /// Sends an echo message to the remote endpoint.  Then, blocks until the remote endpoint
      /// sends an appropriate response.  The buffer remains owned by the caller.
      /// </summary>
      /// <param name="buffer">The data buffer to echo</param>
      /// <param name="length">Length of the data buffer</param>
      /// <returns></returns>
      bool Echo(BYTE* buffer, UINT32 length);

      // @seealso: logger
      void Log(UINT32 file_loggerLevel, LoggingFunction& file_logger);

      /// <summary>
      /// Fills the given BootstrapContext structure's Flags and Properties fields with data
      /// recieved from the Daemon.
      /// </summary>
      void GetBootstrapArguments(dargon::Init::BootstrapContext* context);

      /// <summary>
      /// Registers a task handler for Dargon Injected Module task list entries.
      /// </summary>
      void RegisterDIMTaskHandler(dargon::IO::DIM::IDIMTaskHandler* handler);

      /// <summary>
      /// Processes the given task list
      /// </summary>
      //void ProcessTaskList(const std::vector<DIMTask*> & tasks);

   private:      
      /// <summary>
      /// Prints a dump of the given message to console
      /// </summary>
      /// <param name="message"></param>
      void DumpToConsole(DSPExMessage& message);
      
      /// <summary>
      /// Prints a dump of the given message to console
      /// </summary>
      /// <param name="message"></param>
      void DumpToConsole(DSPExInitialMessage& message);

      /// <summary>
      /// Prints a dump of the given buffer to console
      /// </summary>
      /// <param name="message"></param>
      void DumpBufferToOutputStream(std::ostream& os, const BYTE* buffer, UINT32 length);

      // - Private Fields -------------------------------------------------------------------------
      /// <summary>
      /// The DSPExNode that owns this DSPExSesssion. 
      /// </summary>
      DSPExNode const * const m_pNode;

      std::shared_ptr<dargon::IO::IoProxy> ioProxy;

      /// <summary>
      /// See: Terminated
      /// </summary>
      bool m_terminated;
      
      /// <summary>
      /// The locally initialized transaction lock ensures that the locally initializedtransaction
      /// list is only mutated or accessed by one thread at a time.
      /// </summary>
      MutexType m_locallyInitializedTransactionMutex;
      
      /// <summary>
      /// The remotely initialized transaction lock ensures that the remotely initializedtransaction
      /// list is only mutated or accessed by one thread at a time.
      /// </summary>
      MutexType m_remotelyInitializedTransactionMutex;
      
      /// <summary>
      /// This provides an initially filled Unique Identification Set for interactions IDing.
      /// We take UIDs from this set to label our locally initiated interactions.  Initially,
      /// the set is filled. to contain all possible values.  
      /// 
      /// As this is a server-side implementation, when we take values from this set we
      /// set their HIGH bit; The set is initially filled to the range [0x00000000, 0x7FFFFFFF]
      /// low: 0b00000000 00000000 00000000 00000000 high: 0b01111111 11111111 11111111 11111111
      /// </summary>
      dargon::unique_id_set<TransactionIdType> m_locallyInitializedUIDSet;
      
      /// <summary>
      /// This provides an initially filled Unique Identification Set for interactions IDing.
      /// We give UIDs from this set to label remotely initiated interactions.  Initially,
      /// the set is empty.  Technically, the valid range of the set is [0x80000000, 0xFFFFFFFF]
      /// </summary>
      dargon::unique_id_set<TransactionIdType> m_remotelyInitializedUIDSet;
      
      /// <summary>
      /// Pairs locally initialized transactions with their associated transaction handlers.  
      /// </summary>
      std::unordered_map<UINT32, DSPExLITransactionHandler*> m_locallyInitializedTransactions;
      
      /// <summary>
      /// Pairs remotely initialized transactions with their associated transaction handlers.  
      /// </summary>
      std::unordered_map<UINT32, DSPExRITransactionHandler*> m_remotelyInitializedTransactions;
      
      /// <summary>
      /// Object responsible for handling interprocess communication
      /// </summary>
      IPCObject m_ipc;

      ///// <summary>
      ///// Thread responsible for receiving DSPEx frames and assigning them for processing by
      ///// DSPEx frame processors.
      ///// </summary>
      //std::thread m_frameReceivingThread;
      HANDLE m_frameReceivingThreadHandle;

      /// <summary>
      /// Frame processors waiting to be assigned a DSPEx frame to process.
      /// </summary>
      std::deque<DSPExFrameProcessor*> m_idleFrameProcessors;
      
      /// <summary>
      /// Frame processors currently processing a DSPEx frame.
      /// </summary>
      std::vector<DSPExFrameProcessor*> m_busyFrameProcessors;

      /// <summary>
      /// Mutex that controls adding and removing from our frame processor queues
      /// Recursive because we sometimes need to spin up instances.
      /// </summary>
      std::recursive_mutex m_processorMutex;

      /// <summary>
      /// The write mutex ensures that only one message is being written to our network stream
      /// at a given time.  
      /// </summary>
      std::mutex m_writeMutex;

      /// <summary>
      /// The read mutex ensures that only one DSPEx frame processing thread is reading from the
      /// network stream at a given time.
      /// </summary>
      std::mutex m_readMutex;

      /// <summary>
      /// The output buffer pool provides a pool of buffers.  We use this for message transmitting
      /// so that we don't spend a lot of time allocating and deallocating blocks of memory.
      /// </summary>
      dargon::buffer_manager m_frameBufferPool;

      /// Instruction sets for this DSPExNodeSession specifically.
      std::vector<IDSPExInstructionSet*> m_instructionSets;
      std::mutex m_instructionSetMutex;
      
      FactoryMap kDSPExOpcodeHandlers;

      friend dargon::IO::DSP::DSPExFrameProcessor;
   };
} } }
