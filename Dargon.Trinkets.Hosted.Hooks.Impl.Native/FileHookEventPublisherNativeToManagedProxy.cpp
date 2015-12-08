#include "stdafx.h"
#include "FileHookEventPublisherNativeToManagedProxy.h"

using namespace System::IO;
using namespace Dargon::Trinkets::Hosted::Hooks;

FileHookEventPublisherNativeToBusProxy::FileHookEventPublisherNativeToBusProxy(
   FileSystemHookEventBus^ fileSystemHookEventBus
) : fileSystemHookEventBus(fileSystemHookEventBus) {
}

void FileHookEventPublisherNativeToBusProxy::PublishCreateFileEventPre(dargon::CreateFileEventArgsPre* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew CreateFileArguments(
      gcnew System::String(nativeParameters->path),
      (FileAccess)nativeParameters->desiredAccess,
      (FileShare)nativeParameters->shareMode,
      (FileMode)nativeParameters->creationDisposition);
   auto eventArgs = gcnew CreateFilePreEventArgs(arguments);
   fileSystemHookEventBus->RaiseCreateFilePre(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishCreateFileEventPost(dargon::CreateFileEventArgsPost* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew CreateFileArguments(
      gcnew System::String(nativeParameters->path),
      (FileAccess)nativeParameters->desiredAccess,
      (FileShare)nativeParameters->shareMode,
      (FileMode)nativeParameters->creationDisposition);
   IntPtr returnValue(nativeArgs->retval);
   auto eventArgs = gcnew CreateFilePostEventArgs(arguments, returnValue);
   fileSystemHookEventBus->RaiseCreateFilePost(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishReadFileEventPre(dargon::ReadFileEventArgsPre* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew ReadFileArguments(
      IntPtr(nativeParameters->hFile),
      (std::uint8_t*)nativeParameters->lpBuffer,
      nativeParameters->nNumberOfBytesToRead,
      nativeParameters->lpNumberOfBytesRead,
      nativeParameters->fileOffset);
   auto eventArgs = gcnew ReadFilePreEventArgs(arguments);
   fileSystemHookEventBus->RaiseReadFilePre(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishReadFileEventPost(dargon::ReadFileEventArgsPost* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew ReadFileArguments(
      IntPtr(nativeParameters->hFile),
      (std::uint8_t*)nativeParameters->lpBuffer,
      nativeParameters->nNumberOfBytesToRead,
      nativeParameters->lpNumberOfBytesRead,
      nativeParameters->fileOffset);
   auto eventArgs = gcnew ReadFilePostEventArgs(arguments, nativeArgs->retval);
   fileSystemHookEventBus->RaiseReadFilePost(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishCloseHandleEventPre(dargon::CloseHandleEventArgsPre* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew CloseHandleArguments(
      IntPtr(nativeParameters->handle));
   auto eventArgs = gcnew CloseHandlePreEventArgs(arguments);
   fileSystemHookEventBus->RaiseCloseHandlePre(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishCloseHandleEventPost(dargon::CloseHandleEventArgsPost* nativeArgs) {
   auto nativeParameters = nativeArgs->arguments;
   auto arguments = gcnew CloseHandleArguments(
      IntPtr(nativeParameters->handle));
   auto eventArgs = gcnew CloseHandlePostEventArgs(arguments, nativeArgs->retval);
   fileSystemHookEventBus->RaiseCloseHandlePost(eventArgs);
}
