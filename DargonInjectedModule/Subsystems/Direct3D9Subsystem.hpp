#pragma once

#include "../stdafx.h"

#include <TrinketNatives.hpp>

#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "Direct3D9SubsystemTypedefs.hpp"

namespace dargon {
   namespace Subsystems {
      class Direct3D9Subsystem : public dargon::Subsystem {
      private:
         static Direct3D9HookEventPublisher* direct3D9HookEventPublisher;

      public:
         Direct3D9Subsystem(dargon::Direct3D9HookEventPublisher* direct3D9HookEventPublisher);
         bool Initialize() override;
         bool Uninitialize() override;

         // - static ---------------------------------------------------------------------------------
         DIM_DECL_STATIC_DETOUR(Direct3D9Subsystem, Direct3DCreate9, FunctionDirect3DCreate9, "Direct3DCreate9", MyDirect3DCreate9);

         static IDirect3D9* WINAPI MyDirect3DCreate9(UINT SDKVersion);
      };
   }
}
