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
