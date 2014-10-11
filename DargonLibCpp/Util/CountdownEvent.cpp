#include "../dlc_pch.hpp"
#include <mutex>
#include <condition_variable>
#include "../Dargon.hpp"
#include "CountdownEvent.hpp"
using namespace Dargon::Util;
CountdownEvent::CountdownEvent(UINT32 initialValue)
   : m_counter(initialValue)
{
}
void CountdownEvent::Signal()
{
   std::unique_lock<std::mutex> lock(m_mutex);
   if(m_counter > 0)
   {
      m_counter--;
      if(m_counter == 0)
      {
         m_conditionVariable.notify_all();
      }
   }
}
void CountdownEvent::Wait() const
{
   std::unique_lock<std::mutex> lock(m_mutex);
   if(m_counter > 0)
   {
      m_conditionVariable.wait(lock);
   }
}
bool CountdownEvent::Wait(UINT32 milliseconds) const
{
   std::unique_lock<std::mutex> lock(m_mutex);
   if (m_counter > 0)
   {
      auto waitResult = m_conditionVariable.wait_for(lock, std::chrono::milliseconds(milliseconds));
      if(waitResult == std::cv_status::timeout)
         return false;
      else
         return true;
   }
   return true;
}