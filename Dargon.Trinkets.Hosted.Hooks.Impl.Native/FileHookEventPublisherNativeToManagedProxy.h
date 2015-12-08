#pragma once
#include <stdafx.h>
#include <memory>
#include <TrinketNatives.hpp>

using namespace System;
using namespace Dargon::Trinkets::Hosted::Hooks;

namespace Dargon {
   namespace Trinkets {
      namespace Hosted {
         namespace Hooks {
            public class FileHookEventPublisherNativeToBusProxy : dargon::FileHookEventPublisher {
               gcroot<FileSystemHookEventBus^> fileSystemHookEventBus;

            public:
               FileHookEventPublisherNativeToBusProxy(FileSystemHookEventBus^ fileSystemHookEventBus);
               virtual ~FileHookEventPublisherNativeToBusProxy() {}

               virtual void PublishCreateFileEventPre(dargon::CreateFileEventArgsPre* x) override;
               virtual void PublishCreateFileEventPost(dargon::CreateFileEventArgsPost* x) override;

               virtual void PublishReadFileEventPre(dargon::ReadFileEventArgsPre* x) override;
               virtual void PublishReadFileEventPost(dargon::ReadFileEventArgsPost* x) override;

               virtual void PublishCloseHandleEventPre(dargon::CloseHandleEventArgsPre* x) override;
               virtual void PublishCloseHandleEventPost(dargon::CloseHandleEventArgsPost* x) override;
            };

            public ref class FileHookEventPublisherNativeToBusProxyFactory {
            public:
               FileHookEventPublisherNativeToBusProxy* Create(FileSystemHookEventBus^ fileSystemHookEventBus) {
                  return new FileHookEventPublisherNativeToBusProxy(fileSystemHookEventBus);
               }
            };
         }
      }
   }
}
