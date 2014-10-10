// This file was ported from DargonLib C# - see comments over there.

#include "../dlc_pch.hpp"
#include <list>
#include "../Dargon.hpp"
#include "UniqueIdentificationSet.hpp"
using namespace Dargon::Util;

UniqueIdentificationSet::UniqueIdentificationSet(bool filled)
{
   if(filled)
      m_segments.push_front(Segment(0, UINT32_MAX));
}
UniqueIdentificationSet::UniqueIdentificationSet(UINT32 low, UINT32 high)
{
   m_segments.push_front(Segment(low, high));
}
UINT32 UniqueIdentificationSet::TakeUniqueID()
{
   // TODO: This method assumes that we haven't maxed out the set values
   std::lock_guard<std::mutex> lock(m_mutex);
   auto it = m_segments.begin();
   assert(it != m_segments.end()); // end is after last element
   //std::cout << " !! " << m_segments.size() << " // " << m_segments.begin()._Ptr << " : " << m_segments.end()._Ptr << std::endl;
   //std::cout << "!/! " << (m_segments.begin() != m_segments.end()) << std::endl;
   if (it->low != it->high) {
      return it->low++;
   } else {
      UINT32 returnedValue = it->low;
      m_segments.erase(it);
      return it->low;
   }
}
bool UniqueIdentificationSet::TakeUniqueID(UINT32 uid)
{
   {
      std::lock_guard<std::mutex> lock(m_mutex);
      auto it = m_segments.begin();
      bool done = false;
      while (it != m_segments.end() && !done)
      {
         if (it->low == it->high)
         {
            if (uid == it->low) //And thusly uid equals segment.high
            {
               m_segments.erase(it);
               done = true;
            }
         }
         else
         {
            if (uid == it->low)
            {
               it->low++;
               done = true;
            }
            else if (uid == it->high)
            {
               it->high--;
               done = true;
            }
            else if (it->low < uid && uid < it->high)
            {
               //Segment newSegment = Segment() { low = uid + 1, high = segment.high };
               UINT32 high = it->high;
               it->high = uid - 1;
               it++; // Move it forwards so we can insert behind it
               m_segments.insert(it, Segment(uid + 1, high));
               done = true;
            }
         }
         if(!done) //Prevents undefined it increment if at end of list
            it++;
      }
      if(!done)
         return false;
      else
         return true;
         //throw new std::exception("The Unique ID Set was unable to take the given Unique ID.  Check for UID Leaks");
   }
}
bool UniqueIdentificationSet::GiveUniqueID(UINT32 value)
{
   {
      std::lock_guard<std::mutex> lock(m_mutex);
      //Segment segment;
      //LinkedListNode<Segment> neighborNode;
      SegmentList::iterator* lastSegment = nullptr;
      auto it = m_segments.begin();
      if (m_segments.size() == 0) //We have an empty set
      {
         m_segments.push_back(Segment(value, value));
         return true;
      }
      else
      {
         bool done = false;
         while (it != m_segments.end() && !done)
         {
            if (it->low == value + 1 && value != UINT32_MAX)
            {
               it->low = value;
               if (lastSegment != nullptr && (*lastSegment)->high == it->low + 1)
               {
                  it->low = (*lastSegment)->low;
                  m_segments.erase(*lastSegment);
               }
               done = true;
            }
            else if (it->high == value - 1 && value != 0) //UINT32_MIN
            {
               it->high = value;
               SegmentList::iterator temp = std::next(it);
               auto nextNeighbor = &temp;
               if(temp == m_segments.end())
                  nextNeighbor = nullptr;

               if (nextNeighbor != nullptr && temp->low - 1 == it->high)
               {
                  it->high = temp->high;
                  m_segments.erase(temp);
               }
               done = true;
            }
            else if (it->low > value)
            {
               m_segments.insert(it, Segment(value, value)); //Equivalent to insert before
               done = true;
            }
            else if (it->high < value && std::next(it) == m_segments.end()) //it is end of list
            {
               //Segment newSegment = new Segment() { low = value, high = value };
               //m_segments.AddAfter(it, newSegment);
               m_segments.push_back(Segment(value, value));
               done = true;
            }
            else if (it->low == value || it->high == value)
            {
               //throw new std::exception("Attempted to return UID to UID Set, but value already existed in set!");
               return false;
            }
            else
            {
               it++; // it = it.Next
               done = false;
            }
         }
         if (!done)
            return false; //throw new std::exception("Unable to return UID to Unique ID Set, check for duplicate returns");
         else 
            return true;
      }
   }
}