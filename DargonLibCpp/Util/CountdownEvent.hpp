#pragma once

#include <atomic>
#include <mutex>
#include <condition_variable>
#include <boost/utility.hpp>
#include "../Dargon.hpp"

namespace Dargon { namespace Util {
   /// <summary>
   /// Implementation of a C#/Java-like latch/countdownevent object.  Allows none, one, or many
   /// threads to synchronize themselves.  The object has an initial count N and may be Signal()ed
   /// by any thread.  When object has been signaled N times, all threads waiting on it are 
   /// released.  All operations on the object after it reaches N are no-ops; nothing occurs.
   ///
   /// If you wish to use this object many times, consider using a barrier object.
   /// </summary>
   class CountdownEvent : boost::noncopyable
   {
   public:
      /// <summary>
      /// Initializes a new instance of a Countdown Event/Latch with the given initial counter value.
      /// </summary>
      /// <param name="initialValue">
      /// The initial value set to our counter.
      /// </param>
      CountdownEvent(UINT32 initialValue = 0);

      /// <summary>
      /// Signals the latch object, atomically decrementing its internal counter to a minimum value
      /// of zero.  If the counter reaches zero, then all waiting threads are notified.  If the
      /// counter is initially zero, then the overhead of this call is negligible (it's a simple jmp
      /// operation).
      /// </summary>
      void Signal();

      /// <summary>
      /// Waits indefinitely for the the internal counter of the object to reach zero.  
      /// </summary>
      void Wait();

      /// <summary>
      /// Waits the given number of milliseconds for the internal counter of the object to reach 
      /// zero.  Returns true if the counter reaches zero during/before the call of this method.
      /// </summary>
      bool Wait(UINT32 milliseconds);

   private:
      std::atomic<UINT32> m_counter;
      std::mutex m_mutex;
      std::condition_variable m_conditionVariable;
   };
} }