#pragma once
#include <cstdint>
#include <assert.h>

namespace dargon {
   struct CreateFileArgs {
      const wchar_t* path;
      std::uint32_t desiredAccess;
      std::uint32_t shareMode;
      std::uint32_t creationDisposition;
   };

   struct CreateFileEventArgsPre {
      CreateFileArgs* arguments;
   };

   struct CreateFileEventArgsPost {
      CreateFileArgs* arguments;
      void* retval;
   };

   struct FileHookEventPublisher {
      virtual ~FileHookEventPublisher() { }

      virtual void PublishCreateFileEventPre(CreateFileEventArgsPre* x) = 0;
      virtual void PublishCreateFileEventPost(CreateFileEventArgsPost* x) = 0;
   };

   struct NullFileHookEventPublisher : FileHookEventPublisher {
      virtual void PublishCreateFileEventPre(CreateFileEventArgsPre* x) override { }
      virtual void PublishCreateFileEventPost(CreateFileEventArgsPost* x) override { };
   };

   const uint64_t TRINKET_NATIVES_START_CANARY = 0x74656b6e69727464U; // dtrinket
   const uint64_t TRINKET_NATIVES_TAIL_CANARY = 0x54454b4e49525444U; // DTRINKET

   struct TrinketNatives {
      uint64_t startCanary;
      FileHookEventPublisher* fileHookEventPublisher;
      uint64_t tailCanary;

      void Validate() {
         assert(startCanary == TRINKET_NATIVES_START_CANARY);
         assert(tailCanary == TRINKET_NATIVES_TAIL_CANARY);
      }
   };
}