#pragma once

#include <vector>
#include <boost/noncopyable.hpp>
#include "Base.hpp"

namespace Dargon { namespace Subsystems {
   // todo: self-balancing tree would be ideal here...
   struct FileOverrideNode : boost::noncopyable {
      // length of the data chunk represented
      UINT32 length;

      // start of the data in the resultant file
      UINT32 resultStart;

      // true: read from original file, else read from replacement file
      bool sourceIsOriginalFile;

      // start of the data in the source (replacement or original) file
      UINT32 sourceStart;
      
      // node which comes earlier than us in file index
      FileOverrideNode* m_left;
      
      // node which comes later than us in file index
      FileOverrideNode* m_right;

   public:
      FileOverrideNode(
            UINT32 length, 
            UINT32 resultStart, 
            UINT32 sourceStart, 
            bool sourceIsOriginal)
         : length(length), 
           resultStart(resultStart), 
           sourceIsOriginalFile(sourceIsOriginal),
           sourceStart(sourceStart)
      {
      }
   };

   // maps out the structure (replacements, original, location) of a tree
   // the assumption is made that the file's replacements all lie within the same file (ie dpf).
   // if doing a full file swap, set fileOverrideTree == nullptr.
   // if traversal leads to a null pointer, an override is not defined. perform normal file read.
   class FileOverrideTree : boost::noncopyable {
      FileOverrideNode* m_root;

   public:
      FileOverrideTree() 
         : m_root(nullptr)
      {
      }

      // adds an override.  Simpler than C# impl in that overlapping/cutting isn't permitted.
      void AddOverride(UINT32 length, UINT32 resultStart, UINT32 sourceStart, bool sourceIsOriginal)
      {
         auto node = new FileOverrideNode(length, resultStart, sourceStart, sourceIsOriginal);
         m_root = AddOverrideHelper(node, m_root);
      }

      // Gets the override node for the given offset.
      FileOverrideNode* GetOverrideNode(UINT32 resultOffset)
      {
         return GetOverrideNodeHelper(resultOffset, m_root);
      }

   private:      
      // node: the node we're inserting
      // current: the current node we're traversing at, possibly null.
      // returns: the node to replace current with.
      FileOverrideNode* AddOverrideHelper(FileOverrideNode* node, FileOverrideNode* current)
      {
         // handle current = nullptr - this means add node at that point.
         if (current == nullptr)
            return node;
         
         // otherwise, node is before or after current.
         auto nodeEnd = node->resultStart + node->length - 1; // inclusive
         if (nodeEnd < current->resultStart)
            current->m_left = AddOverrideHelper(node, current->m_left);
         else 
            current->m_right = AddOverrideHelper(node, current -> m_right);
      }

      FileOverrideNode* GetOverrideNodeHelper(UINT32 resultOffset, FileOverrideNode* current)
      {
         // run past terminal node = there is no override
         if (current == nullptr)
            return nullptr;

         // check within = done
         auto currentEnd = current->resultStart + current->length; // exclusive
         if (current->resultStart <= resultOffset && 
             resultOffset < currentEnd)
             return current;
         else
         {
            // if current is before the offset, traverse right, else traverse left.
            if (currentEnd <= resultOffset)
               return GetOverrideNodeHelper(resultOffset, current->m_right);
            else
               return GetOverrideNodeHelper(resultOffset, current->m_left);
         }
      }
   };
} }