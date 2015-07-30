#pragma once

#include <map>
#include <mutex>
#include "dargon.hpp"

namespace dargon { 
   /// <summary>
   /// Allows users to request buffers of a given size from a pool of preallocated buffers.  These
   /// buffers can later be returned to the pool.  The pool maintains a maxium size, ensuring that
   /// we don't allocate buffers that end up never getting used.
   ///
   /// The buffer manager is optimized to prevent more reallocations; when large buffers are 
   /// requested and returned, the smallest buffers are freed and the largest buffers remain 
   /// allocated.  This means that over time, the cumulative size of the allocated buffers will
   /// increase as more and more buffers of random size are allocated.  However, since buffers are
   /// returned after they are used, the number of high-size buffers allocated by repetitive 
   /// synchronous operations is capped by the number of repetitive synchronous operations.  
   ///
   /// Setting maxPoolSize to something low can decrease performance due to repetitive alloc/free 
   /// calls.  Setting it to a high value can decrease performance due to paging.
   ///
   /// TODO: Cap the cumulative size of the allocated pool and free huge buffers?
   /// </summary>
   class buffer_manager
   {
      typedef std::multimap<UINT32, dargon::blob*> PoolMap;
   public:
      /// <summary>
      /// Initializes a new instance of a Buffer Manager with the given maximum pool size and 
      /// maximum buffer size.
      /// </summary>
      /// <param name="maxPoolSize">
      /// The maximum number of buffers that may be stored in this buffer manager.  If additional
      /// buffers are returned, then the memory associated with those buffers is freed.
      /// </param>
      /// <param name="minBufferSize">
      /// The minimum size (in bytes) of a buffer allocated by this buffer manager.
      /// </param>
      buffer_manager(UINT32 maxPoolSize, UINT32 minBufferSize);

      /// <summary>
      /// Gets a Dargon blob of the given size or larger. The returned Dargon blob will not have its
      /// size value changed (it will be equal to the number of bytes allocated for the blob, which
      /// will be greater than or equal to the size parameter).
      ///
      /// If the requested buffer size is larger than the designated maximum buffer size of this 
      /// pool, then a new buffer will always be allocated. 
      /// </summary>
      dargon::blob* take(UINT32 size = 0);

      /// <summary>
      /// Returns a Dargon Buffer to the pool.
      /// </summary>
      /// <param name="blob">
      /// The Dargon blob to return to the buffer manager.
      /// </param>
      void give(dargon::blob* blob);

   private:
      UINT32 m_maxPoolSize;
      UINT32 m_minBufferSize;
      PoolMap m_poolContents;
      std::mutex m_mutex;
   };
} 
