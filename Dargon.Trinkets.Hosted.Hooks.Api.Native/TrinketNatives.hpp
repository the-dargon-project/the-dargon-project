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

   struct ReadFileArgs {
      void* hFile;
      void* lpBuffer;
      std::uint32_t nNumberOfBytesToRead;
      std::uint32_t* lpNumberOfBytesRead;
      std::uint64_t fileOffset;
   };

   struct ReadFileEventArgsPre {
      ReadFileArgs* arguments;
   };

   struct ReadFileEventArgsPost {
      ReadFileArgs* arguments;
      std::uint32_t retval;
   };

   struct CloseHandleArgs {
      void* handle;
   };

   struct CloseHandleEventArgsPre {
      CloseHandleArgs* arguments;
   };

   struct CloseHandleEventArgsPost {
      CloseHandleArgs* arguments;
      std::uint32_t retval;
   };

   struct CreateDeviceEventArgsPost {
      void* retval;
   };

   struct CreateTextureArgs {
      std::uint32_t width;
      std::uint32_t height;
      std::uint32_t levels;
      std::uint32_t usage;
      std::int32_t format;
      std::int32_t pool;
      void** ppTexture;
      void* pSharedHandle;
   };

   struct CreateTextureEventArgsPost {
      CreateTextureArgs* arguments;
      std::uint32_t retval;
   };

   struct SetTextureArgs {
      std::uint32_t stage;
      void* pTexture;
   };

   struct SetTextureEventArgsPost {
      SetTextureArgs* arguments;
      std::uint32_t retval;
   };

   struct FileHookEventPublisher {
      virtual ~FileHookEventPublisher() { }

      virtual void PublishCreateFileEventPre(CreateFileEventArgsPre* x) = 0;
      virtual void PublishCreateFileEventPost(CreateFileEventArgsPost* x) = 0;

      virtual void PublishReadFileEventPre(ReadFileEventArgsPre* x) = 0;
      virtual void PublishReadFileEventPost(ReadFileEventArgsPost* x) = 0;

      virtual void PublishCloseHandleEventPre(CloseHandleEventArgsPre* x) = 0;
      virtual void PublishCloseHandleEventPost(CloseHandleEventArgsPost* x) = 0;
   };

   struct NullFileHookEventPublisher : FileHookEventPublisher {
      virtual void PublishCreateFileEventPre(CreateFileEventArgsPre* x) override { };
      virtual void PublishCreateFileEventPost(CreateFileEventArgsPost* x) override { };

      virtual void PublishReadFileEventPre(ReadFileEventArgsPre* x) override { };
      virtual void PublishReadFileEventPost(ReadFileEventArgsPost* x) override { };

      virtual void PublishCloseHandleEventPre(CloseHandleEventArgsPre* x) override {};
      virtual void PublishCloseHandleEventPost(CloseHandleEventArgsPost* x) override {};
   };

   struct Direct3D9HookEventPublisher {
      virtual ~Direct3D9HookEventPublisher() {}

      virtual void PublishCreateDeviceEventPost(CreateDeviceEventArgsPost* x) = 0;
      
      virtual void PublishCreateTextureEventPost(CreateTextureEventArgsPost* x) = 0;
     
      virtual void PublishSetTextureEventPost(SetTextureEventArgsPost* x) = 0;
   };

   struct NullDirect3D9HookEventPublisher : Direct3D9HookEventPublisher {
      virtual void PublishCreateDeviceEventPost(CreateDeviceEventArgsPost* x) override {};
      
      virtual void PublishCreateTextureEventPost(CreateTextureEventArgsPost* x) override {};
      
      virtual void PublishSetTextureEventPost(SetTextureEventArgsPost* x) override {};
   };

   const uint64_t TRINKET_NATIVES_START_CANARY = 0x74656b6e69727464U; // dtrinket
   const uint64_t TRINKET_NATIVES_TAIL_CANARY = 0x54454b4e49525444U; // DTRINKET

   struct TrinketNatives {
      uint64_t startCanary;
      FileHookEventPublisher* fileHookEventPublisher;
      Direct3D9HookEventPublisher* direct3D9HookEventPublisher;
      uint64_t tailCanary;

      void Validate() {
         assert(startCanary == TRINKET_NATIVES_START_CANARY);
         assert(tailCanary == TRINKET_NATIVES_TAIL_CANARY);
      }
   };
}