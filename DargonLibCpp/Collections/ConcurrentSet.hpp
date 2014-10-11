#pragma once

#include "ConcurrentDictionary.hpp"

namespace Dargon { namespace Collections {
   template <typename TKey,
             class KeyHash = std::hash<TKey>,
             class KeyEqualityComparer = std::equal_to<TKey>,
             class PairAllocator = std::allocator<std::pair<const TKey, bool>>>
   class ConcurrentSet_iterator : public std::iterator<std::forward_iterator_tag, const TKey>
   {
      typedef ConcurrentSet_iterator<TKey, KeyHash, KeyEqualityComparer, PairAllocator> my_t;
      typedef ConcurrentDictionary_iterator<TKey, bool, KeyHash, KeyEqualityComparer, PairAllocator> dict_iterator_t;
      typedef std::pair<const TKey, bool> pair_t;

   private:
      dict_iterator_t it;

   public:
      ConcurrentSet_iterator() { }
      explicit ConcurrentSet_iterator(dict_iterator_t it) : it(it) { }
      ConcurrentSet_iterator(const my_t& iterator) : it(iterator.it) { }

      my_t operator++() { ++it; return *this; }
      my_t operator++(int) { my_t copy(*this); ++it; return copy; }

      const TKey& operator* () { return it->first; }
      const TKey* operator-> () { return &it->first; }

      bool operator==(const my_t& other) { return it == other.it; }
      bool operator!=(const my_t& other) { return it != other.it; }
   };

   template <typename TKey,
             class KeyHash = std::hash<TKey>,
             class KeyEqualityComparer = std::equal_to<TKey>,
             class PairAllocator = std::allocator<std::pair<const TKey, bool>>>
   class ConcurrentSet
   {
      typedef ConcurrentSet<TKey, KeyHash, KeyEqualityComparer, PairAllocator> my_t;
      typedef ConcurrentDictionary<TKey, bool, KeyHash, KeyEqualityComparer, PairAllocator> dict_t;
      typedef ConcurrentSet_iterator<TKey, KeyHash, KeyEqualityComparer, PairAllocator> iterator;
      typedef std::pair<TKey, bool> pair_t;

      dict_t dict;

   public:
      inline bool insert(const TKey key) { return dict.insert(key, true); }

      inline bool remove(const TKey key) { return dict.remove(key); }
      inline bool erase(const TKey key) { return remove(key); }

      inline bool contains(const TKey key) const { return dict.contains(key); }

      inline size_t size() const { return dict.size(); }

      iterator begin() const { return iterator(dict.begin()); }
      iterator end() const { return iterator(dict.end()); }
   };

   template <typename TKey,
             class KeyHash = std::hash<TKey>,
             class KeyEqualityComparer = std::equal_to<TKey>,
             class PairAllocator = std::allocator<std::pair<const TKey, bool>>>
   using concurrent_set = ConcurrentSet<TKey, KeyHash, KeyEqualityComparer, PairAllocator>;
} }
