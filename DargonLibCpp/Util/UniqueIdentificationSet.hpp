#pragma once

#include <list>
#include <boost/utility.hpp>
#include <mutex>
#include "../Dargon.hpp"

namespace Dargon { namespace Util {
   // See ItzWartyLib c# implementation for comments.
   class UniqueIdentificationSet : boost::noncopyable
   {
   private:
      /// <summary>
      /// A segment in our UID Set
      /// <seealso cref="UniqueIdentificationSet"/>
      /// </summary>
      typedef struct _Segment {
      public:
         /// <summary>
         /// The low end of our segment's range
         /// </summary>
         UINT32 low;

         /// <summary>
         /// The high end of our segment's range
         /// </summary>
         UINT32 high;

         _Segment(UINT32 low, UINT32 high) { this->low = low; this->high = high; }
      } Segment;
      
      typedef std::list<Segment> SegmentList;
      
      /// <summary>
      /// Our linkedlist of segments
      /// </summary>
      SegmentList m_segments;
      
      /// <summary>
      /// We use this lock object to ensure that only one thread modifies the m_segments list at a time.
      /// </summary>
      std::mutex m_mutex;

   public:
      /// <summary>
      /// Initializes a new instance of a Unique Identification Set as either filled or empty
      /// </summary>
      /// <param name="filled">
      /// If set to true, the set is initially full.  Otherwise, the set is initially empty.
      /// </param>
      UniqueIdentificationSet(bool filled);
            
      /// <summary>
      /// Initializes a new instance of a Unique Identification Set with the given initial range of
      /// available values
      /// </summary>
      /// <param name="low">The low bound of the set</param>
      /// <param name="high">The high bound of the set</param>
      UniqueIdentificationSet(UINT32 low, UINT32 high);

      /// <summary>
      /// Takes a unique identifier from the Unique Identification set.
      ///     foreach segment
      ///        if(segment.low != segment.high)
      ///          return segment.low--;
      /// </summary>
      /// <returns>A unique identifier</returns>
      UINT32 TakeUniqueID();

      /// <summary>
      /// Takes a unique identifier from the Unique Identification set.
      /// If the UID does not exist in the set, an exception will be thrown.
      /// </summary>
      /// <param name="uid">The UID which we are taking from the set</param>
      /// <returns>A unique identifier</returns>
      bool TakeUniqueID(UINT32 uid);
      
      /// <summary>
      /// Returns a unique identifier to the Unique Identification Set.
      ///     foreach segment
      ///        if(segment.low == value + 1) //This ensures we don't face overflow issues
      ///          segment.low = value;
      ///        else if(segment.high = value - 1)
      ///        {
      ///          segment.high = value;
      ///          if(nextSegment.low = segment.high)
      ///          {
      ///            segment.high = nextSegment.high;
      ///            RemoveSegment(nextSegment).
      ///          }
      ///        }
      ///        else if(segment.low > value) // Ie: Inserting 3 before [5, INT32_MAX]
      ///        {
      ///          segment.prepend([value, value]);
      ///        }
      /// </summary>
      /// <param name="value">The UID which we are returning to the set.</param>
      bool GiveUniqueID(UINT32 value);
   
   private:
      friend inline std::ostream& operator<<(std::ostream& os, const Dargon::Util::UniqueIdentificationSet & self);
   };
} }

#include "UniqueIdentificationSet.inl.hpp"