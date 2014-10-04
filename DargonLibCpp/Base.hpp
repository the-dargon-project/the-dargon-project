#pragma once

#include <boost/integer.hpp>
#include <boost/utility.hpp>
// An IN paramter is read by the method, but not mutated by the method.
#define IN

// An OUT paramter is not read by the method, but is mutated by the method.
#define OUT

// An IN_OUT paramter is both read and mutated by the method
#define IN_OUT

// An OPTIONAL paramter is not required.  If unspecified, set it to the default vlaue (0/NULL).
#define OPTIONAL

// Basic Type Definitions
typedef boost::uint8_t  UINT8;
typedef boost::uint16_t UINT16;
typedef boost::uint32_t UINT32;
typedef boost::uint64_t UINT64;

typedef boost::int8_t  INT8;
typedef boost::int16_t INT16;
typedef boost::int32_t INT32;
typedef boost::int64_t INT64;

typedef INT8   SBYTE;
typedef UINT8  BYTE;

typedef INT16  SHORT;
typedef UINT16 USHORT;

namespace Dargon {

   class Blob : private boost::noncopyable
   {
   public:
      // The size of our blob's data block
      UINT32 const size;

      // Pointer to the data block of our blob.  Owned and disposed by the blob.
      UINT8* const data;
      
      /// <summary>
      /// Initializes a new instance of a binary large object of the given size.
      /// </summary>
      /// <param name="newSize">
      /// The size of our new blob's data block.
      /// </param>
      Blob(UINT32 newSize);
      
      /// <summary>
      /// Initializes a new instance of a binary large object of the given size.
      /// </summary>
      /// <param name="newSize">
      /// The size of our new blob's data block.
      /// </param>
      /// <param name="newData">
      /// Our blob's new data block, which becomes owned by the Blob object.  When the blob object
      /// is destructed, then the data block's memory is freed.
      /// </param>
      Blob(UINT32 newSize, UINT8* newData);

      /// <summary>
      /// Frees the data block associated with this blob.
      /// </summary>
      ~Blob();
   };
}