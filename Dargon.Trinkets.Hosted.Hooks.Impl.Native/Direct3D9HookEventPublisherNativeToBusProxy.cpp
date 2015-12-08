#include "stdafx.h"
#include "Direct3D9HookEventPublisherNativeToBusProxy.h"

using namespace System::IO;
using namespace Dargon::Trinkets::Hosted::Hooks;

Direct3D9HookEventPublisherNativeToBusProxy::Direct3D9HookEventPublisherNativeToBusProxy(
   Direct3D9HookEventBus^ hookEventBus
) : hookEventBus(hookEventBus) {
}

void Direct3D9HookEventPublisherNativeToBusProxy::PublishCreateDeviceEventPost(dargon::CreateDeviceEventArgsPost* nativeArgs) {
   auto eventArgs = gcnew CreateDevicePostEventArgs(IntPtr(nativeArgs->retval));
   hookEventBus->RaiseCreateDevicePost(eventArgs);
}

void Direct3D9HookEventPublisherNativeToBusProxy::PublishCreateTextureEventPost(dargon::CreateTextureEventArgsPost* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew CreateTextureArguments(
      nativeParameters->width,
      nativeParameters->height,
      nativeParameters->levels,
      nativeParameters->usage,
      nativeParameters->format,
      nativeParameters->pool,
      IntPtr(nativeParameters->ppTexture),
      IntPtr(nativeParameters->pSharedHandle));
   auto eventArgs = gcnew CreateTexturePostEventArgs(arguments, nativeArgs->retval);
   hookEventBus->RaiseCreateTexturePost(eventArgs);
}

void Direct3D9HookEventPublisherNativeToBusProxy::PublishSetTextureEventPost(dargon::SetTextureEventArgsPost* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew SetTextureArguments(
      nativeParameters->stage,
      IntPtr(nativeParameters->pTexture));
   auto eventArgs = gcnew SetTexturePostEventArgs(arguments, nativeArgs->retval);
   hookEventBus->RaiseSetTexturePost(eventArgs);
}
