// This is the main DLL file.

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
      (FileMode)nativeParameters->creationDisposition
   );
   auto eventArgs = gcnew CreateFilePreEventArgs(arguments);
   fileSystemHookEventBus->RaiseCreateFilePre(eventArgs);
}

void FileHookEventPublisherNativeToBusProxy::PublishCreateFileEventPost(dargon::CreateFileEventArgsPost* x) {

}
