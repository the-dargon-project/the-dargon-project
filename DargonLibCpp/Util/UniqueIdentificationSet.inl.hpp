#pragma once

#include "../dlc_pch.hpp"
#include "UniqueIdentificationSet.hpp"

inline std::ostream& Dargon::Util::operator<<(std::ostream& os, const Dargon::Util::UniqueIdentificationSet & uidSet) 
{
   os << "[UniqueIdentificationSet {";
   {
      boost::mutex& mutex = const_cast<boost::mutex&>(uidSet.m_mutex);
      boost::lock_guard<boost::mutex> lock(mutex );
      auto it = uidSet.m_segments.begin(); 
      while(it != uidSet.m_segments.end())
      {
         auto current = *(it++); //Get it value as A, then increment it to B, then dereference A
   
         os << "[" << current.low << ", " << current.high << "]";
         if(it != uidSet.m_segments.end())
            os << ", ";
      }
   }
   return os << "}]";
}