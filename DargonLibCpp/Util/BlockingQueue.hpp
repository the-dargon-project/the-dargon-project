#include <queue>
#include <condition_variable>

namespace Dargon { namespace Util { 
   template <typename T>
   class BlockingQueue
   {
   private:
      typedef std::deque<T> container_type; // Constant time insertion/deletion at front/back.
      typedef std::mutex mutex_type;
      typedef std::lock_guard<mutex_type> lock_type;

      container_type m_queue;
      mutex_type m_mutex;
      std::condition_variable m_condition;

   public:
      /// <summary>
      /// Enqueues the given value into our thread-safe blocking queue.  The programmer becomes 
      /// responsible for the lifetime of the parameter while it is within the queue; before the
      /// queue is disposed, the programmer must empty it and dispose of its contents.
      /// </summary>
      void push(T const& value)
      {
         {
            lock_type lock(m_mutex);
            m_queue.push_back(value);
         }
         m_condition.notify_one();
      }

      /// <summary>
      /// Dequeues a value from our thread-safe blocking queue.  The caller becomes responsible
      /// for the lifetime of the returned value.  This method blocks until an element from the
      /// queue becomes available (as in, we wait while it is empty).
      /// </summary>
      T pop()
      {
         lock_type lock(m_mutex);
         m_condition.wait(lock, [this]{ return !m_queue.empty(); });
         T returnedValue = std::move(m_queue.front()); //via http://stackoverflow.com/questions/2142965/c0x-move-from-container#comment2084416_2143009
         m_queue.pop_front();
         return returnedValue;
      }

      typename container_type::size_type size()
      {
         return m_queue.size(); 
      }
   };
} }