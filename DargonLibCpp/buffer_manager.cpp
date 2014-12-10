#include "dlc_pch.hpp"
#include "buffer_manager.hpp"

using namespace dargon;

buffer_manager::buffer_manager(UINT32 maxPoolSize, UINT32 minBufferSize)
   : m_maxPoolSize(maxPoolSize),
   m_minBufferSize(minBufferSize) {}

dargon::Blob* buffer_manager::take(UINT32 size) {
   std::unique_lock<std::mutex> lock(m_mutex);
   auto it = m_poolContents.lower_bound(size);
   if (it != m_poolContents.end()) {
      auto returnedBlob = *it;
      m_poolContents.erase(it);
      lock.unlock();
      return returnedBlob.second;
   } else // size is greater than anything in our pool, alloc new blob
   {
      lock.unlock();
      return new Blob(std::max(size, m_minBufferSize));
   }
}

void buffer_manager::give(dargon::Blob* blob) {
   std::unique_lock<std::mutex> lock(m_mutex);
   m_poolContents.insert(PoolMap::value_type(blob->size, blob));
   if (m_poolContents.size() > m_maxPoolSize) {
      // If we go over the pool size cap, unalloc the smallest allocated buffer.
      auto frontElement = m_poolContents.begin();
      auto blob = frontElement->second;
      m_poolContents.erase(frontElement);
      delete blob;
   }
}