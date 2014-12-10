#pragma once

namespace dargon { namespace util {
   struct noncopyable {
      noncopyable() = default;
      noncopyable(const noncopyable&) = delete;
      noncopyable& operator=(const noncopyable&) = delete;
   };
} }
