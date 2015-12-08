#pragma once
#include <stdafx.h>
#include <vcclr.h>
#include <memory>
#include <TrinketNatives.hpp>

using namespace System;
using namespace Dargon::Trinkets::Hosted::Hooks;

namespace Dargon {
   namespace Trinkets {
      namespace Hosted {
         namespace Hooks {
            public class Direct3D9HookEventPublisherNativeToBusProxy : dargon::Direct3D9HookEventPublisher {
               gcroot<Direct3D9HookEventBus^> hookEventBus;

            public:
               Direct3D9HookEventPublisherNativeToBusProxy(Direct3D9HookEventBus^ hookEventBus);
               virtual ~Direct3D9HookEventPublisherNativeToBusProxy() {}

               virtual void PublishCreateDeviceEventPost(dargon::CreateDeviceEventArgsPost* x) override;
               
               virtual void PublishCreateTextureEventPost(dargon::CreateTextureEventArgsPost* x) override;
               
               virtual void PublishSetTextureEventPost(dargon::SetTextureEventArgsPost* x) override;
            };

            public ref class Direct3D9HookEventPublisherNativeToBusProxyFactory {
            public:
               Direct3D9HookEventPublisherNativeToBusProxy* Create(Direct3D9HookEventBus^ hookEventBus) {
                  return new Direct3D9HookEventPublisherNativeToBusProxy(hookEventBus);
               }
            };
         }
      }
   }
}
