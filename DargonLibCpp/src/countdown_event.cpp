#include "dlc_pch.hpp"
#include <mutex>
#include <condition_variable>
#include "dargon.hpp"
#include "countdown_event.hpp"

using namespace dargon;

countdown_event::countdown_event(UINT32 initialValue)
   : m_counter(initialValue)
{
}

void countdown_event::signal() {
   std::unique_lock<std::mutex> lock(m_mutex);
   if (m_counter > 0) {
      m_counter--;
      if (m_counter == 0) {
         m_conditionVariable.notify_all();
      }
   }
}

void countdown_event::wait() const {
   std::unique_lock<std::mutex> lock(m_mutex);
   if (m_counter > 0) {
      m_conditionVariable.wait(lock);
   }
}

bool countdown_event::wait(UINT32 milliseconds) const {
   std::unique_lock<std::mutex> lock(m_mutex);
   if (m_counter > 0) {
      auto waitResult = m_conditionVariable.wait_for(lock, std::chrono::milliseconds(milliseconds));
      if (waitResult == std::cv_status::timeout)
         return false;
      else
         return true;
   }
   return true;
}