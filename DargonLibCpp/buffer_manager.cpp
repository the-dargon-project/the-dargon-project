#include "dlc_pch.hpp"
#include <algorithm>
#include <mutex>
#include "buffer_manager.hpp"
#include "base.hpp"

using namespace dargon;

buffer_manager::buffer_manager(UINT32 maxPoolSize, UINT32 minBufferSize)
   : m_maxPoolSize(maxPoolSize),
   m_minBufferSize(minBufferSize) {}

dargon::blob* buffer_manager::take(UINT32 size) {
   std::unique_lock<std::mutex> lock(m_mutex);
   auto it = m_poolContents.lower_bound(size);
   if (it != m_poolContents.end()) {
      auto returned_blob = it->second;
      m_poolContents.erase(it);
      lock.unlock();
      return returned_blob;
   } else {
      // size is greater than anything in our pool, alloc new blob
      lock.unlock();
      auto blobSize = std::max(size, m_minBufferSize);
      return new blob(blobSize);
   }
}

void buffer_manager::give(dargon::blob* blob) {
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