#pragma once

#include <limits>
#include <list>
#include <type_traits>
#include <mutex>
#include <stdexcept>
#include "Dargon.hpp"
#include "noncopyable.hpp"

namespace dargon { 
   // #define TValue UINT32

   template<typename TValue, typename = typename std::enable_if<std::is_arithmetic<TValue>::value, TValue>::type>
   class unique_id_set : dargon::noncopyable
   {
      class _Node {
      public:
         TValue low;
         TValue high;

         _Node* next;
         _Node* prev;

         _Node(TValue low, TValue high) : _Node(low, high, nullptr, nullptr) { }
         _Node(TValue low, TValue high, _Node* next, _Node* prev) : low(low), high(high), next(next), prev(prev) { }

         inline bool Contains(TValue value) { return low <= value && value <= high; }
      };

      typedef std::numeric_limits<TValue> limits;
      typedef std::mutex TMutex;
      typedef std::lock_guard<TMutex> TLock;

      _Node* front;
      std::mutex mutex;

   public:
      unique_id_set(bool filled) : unique_id_set(filled ? limits::min() : 0, filled ? limits::min() : 0) {}
      unique_id_set(TValue low, TValue high) : front(new _Node(low, high)) { }

      TValue take() {
         TLock lock(mutex);
         if (front == nullptr) {
            throw std::runtime_error("Attempted to take Unique ID from empty Unique ID set.");
         } else {
            auto result = front->low++;
            if (front->low > front->high) {
               auto oldFront = front;
               auto newFront = front->next;
               _Unlink(oldFront, newFront);
               delete oldFront;
               front = newFront;
            }
            return result;
         }
      }

      bool take(TValue value) {
         if (front == nullptr) {
            return false;
         } else {
            if (front->low == value) {
               take(); // Takes frontmost value
               return true;
            } else {
               for (auto current = front; current != nullptr; current = current->next) {
                  if (current->Contains(value)) {
                     if (current->low == current->high) {
                        auto oldPrev = current->prev;
                        auto oldNext = current->next;
                        _Link(oldPrev, oldNext);
                        delete current;
                        return true;
                     } else if (current->low == value) {
                        current->low++;
                        return true;
                     } else if (current->high == value) {
                        current->high--;
                        return true;
                     } else {
                        auto oldPrev = current->prev;
                        auto oldNext = current->next;
                        auto newNode = new _Node(current->low, value - 1);
                        current->low = value + 1;
                        _Link(oldPrev, newNode);
                        _Link(newNode, current);
                        _Link(current, oldNext);
                        return true;
                     }
                  }
               }
               return false;
            }
         }
      }

      bool give(TValue value) {
         TLock lock(mutex);
         if (front == nullptr) {
            front = new _Node(value, value);
            return true;
         } else if (front->low != limits::min() && value < front->low - 1) {
            auto newNode = new _Node(value, value);
            _Link(newNode, front);
            front = newNode;
            return true;
         } else if (value == limits::max()) {
            auto current = front;
            while (current->next != nullptr) {
               current = current->next;
            }
            if (current->high + 1 == value) {
               current->high = value;
               return true;
            } else {
               auto newNode = new _Node(value, value);
               _Link(current, newNode);
               return true;
            }
         } else {
            for (auto current = front; current != nullptr; current = current->next) {
               if (current->low == value + 1) {
                  current->low = value;
                  return true;
               } else if (current->high != limits::max() && current->high + 1 == value) {
                  current->high = value;

                  if (current->next != nullptr) {
                     if (current->high + 1 == current->next->low) {
                        auto oldNext = current->next;
                        auto nextNext = oldNext->next;
                        current->high = current->next->high;
                        _Link(current, nextNext);
                        delete oldNext;
                        return true;
                     }
                  }
                  return true;
               } else if (current->next == nullptr) {
                  auto newNode = new _Node(value, value);
                  _Link(current, newNode);
                  return true;
               }
            }
         }
      }
   
   private:
      template<typename TValue_, typename = typename std::enable_if<std::is_arithmetic<TValue_>::value, TValue_>::type>
      friend inline std::ostream& operator<<(std::ostream& os, const dargon::unique_id_set<TValue_>& self) {
         return os;
      }
      /*
      

inline std::ostream& dargon::operator<<(std::ostream& os, const dargon::unique_id_set& uidSet) 
{
   os << "[unique_id_set {";
   {
      std::mutex& mutex = const_cast<std::mutex&>(uidSet.mutex);
      std::lock_guard<std::mutex> lock(mutex);

      for (auto current = uidSet.front; current != nullptr; current = current->next) {
         os << "[" << current->low << ", " << current->high << "]";
         if (current->next != nullptr) {
            os << ", ";
         }
      }
   }
   return os << "}]";
}
*/

      void inline _Unlink(_Node* left, _Node* right) {
         if (left) {
            left->next = nullptr;
         }
         if (right) {
            right->prev = nullptr;
         }
      }
      void inline _Link(_Node* left, _Node* right) {
         if (left) {
            left->next = right;
         }
         if (right) {
            right->prev = left;
         }
      }
   };
}
