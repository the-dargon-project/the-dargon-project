#include "../stdafx.h"
#include <iostream>
#include "Direct3D9Subsystem.hpp"

using namespace dargon;
using namespace dargon::Subsystems;

// - instance -------------------------------------------------------------------------------------
Direct3D9Subsystem::Direct3D9Subsystem(
   dargon::Direct3D9HookEventPublisher* direct3D9HookEventPublisher
) : Subsystem() {
   Direct3D9Subsystem::direct3D9HookEventPublisher = direct3D9HookEventPublisher;
}

bool Direct3D9Subsystem::Initialize() {
   if (Subsystem::Initialize()) {
      HMODULE hModuleD3d9 = GetModuleHandleA("d3d9.dll");
      if (hModuleD3d9) {
         InstallDirect3DCreate9Detour(hModuleD3d9);
      }
      return true;
   } else {
      return false;
   }
}

bool Direct3D9Subsystem::Uninitialize() {
   if (Subsystem::Uninitialize()) {
      UninstallDirect3DCreate9Detour();
      return true;
   } else {
      return false;
   }
}

// - static ---------------------------------------------------------------------------------------
dargon::Direct3D9HookEventPublisher* Direct3D9Subsystem::direct3D9HookEventPublisher;

DIM_IMPL_STATIC_DETOUR(Direct3D9Subsystem, Direct3DCreate9, FunctionDirect3DCreate9, "Direct3DCreate9", MyDirect3DCreate9);

// - proxies ---------------------------------------------------------------------------------------
class Direct3D9Proxy;
class Direct3DDevice9Proxy;

class Direct3DDevice9Proxy : public IDirect3DDevice9 {
   Direct3D9HookEventPublisher* hookEventPublisher;
   IDirect3D9* direct3d;
   IDirect3DDevice9* target;

public:
   Direct3DDevice9Proxy(
      Direct3D9HookEventPublisher* hookEventPublisher,
      IDirect3D9* direct3d,
      IDirect3DDevice9* target
   ) : hookEventPublisher(hookEventPublisher), direct3d(direct3d), target(target) {
      std::wcout << "At constructor of Direct3DDevice9Proxy" << std::endl;
   }

   HRESULT __stdcall QueryInterface(const IID& riid, void** ppvObj) override {
      return target->QueryInterface(riid, ppvObj);
   }
   ULONG __stdcall AddRef() override {
      return target->AddRef();
   }
   ULONG __stdcall Release() override {
      return target->Release();
   }
   HRESULT __stdcall TestCooperativeLevel() override {
      return target->TestCooperativeLevel();
   }
   UINT __stdcall GetAvailableTextureMem() override {
      return target->GetAvailableTextureMem();
   }
   HRESULT __stdcall EvictManagedResources() override {
      return target->EvictManagedResources();
   }
   HRESULT __stdcall GetDirect3D(IDirect3D9** ppD3D9) override {
      direct3d->AddRef();
      *ppD3D9 = direct3d;
      return D3D_OK;
   }
   HRESULT __stdcall GetDeviceCaps(D3DCAPS9* pCaps) override {
      return target->GetDeviceCaps(pCaps);
   }
   HRESULT __stdcall GetDisplayMode(UINT iSwapChain, D3DDISPLAYMODE* pMode) override {
      return target->GetDisplayMode(iSwapChain, pMode);
   }
   HRESULT __stdcall GetCreationParameters(D3DDEVICE_CREATION_PARAMETERS* pParameters) override {
      return target->GetCreationParameters(pParameters);
   }
   HRESULT __stdcall SetCursorProperties(UINT XHotSpot, UINT YHotSpot, IDirect3DSurface9* pCursorBitmap) override {
      return target->SetCursorProperties(XHotSpot, YHotSpot, pCursorBitmap);
   }
   void __stdcall SetCursorPosition(int X, int Y, DWORD Flags) override {
      return target->SetCursorPosition(X, Y, Flags);
   }
   BOOL __stdcall ShowCursor(BOOL bShow) override {
      return target->ShowCursor(bShow);
   }
   HRESULT __stdcall CreateAdditionalSwapChain(D3DPRESENT_PARAMETERS* pPresentationParameters, IDirect3DSwapChain9** pSwapChain) override {
      return target->CreateAdditionalSwapChain(pPresentationParameters, pSwapChain);
   }
   HRESULT __stdcall GetSwapChain(UINT iSwapChain, IDirect3DSwapChain9** pSwapChain) override {
      return target->GetSwapChain(iSwapChain, pSwapChain);
   }
   UINT __stdcall GetNumberOfSwapChains() override {
      return target->GetNumberOfSwapChains();
   }
   HRESULT __stdcall Reset(D3DPRESENT_PARAMETERS* pPresentationParameters) override {
      return target->Reset(pPresentationParameters);
   }
   HRESULT __stdcall Present(const RECT* pSourceRect, const RECT* pDestRect, HWND hDestWindowOverride, const RGNDATA* pDirtyRegion) override {
//      std::wcout << "Present!" << std::endl;
      return target->Present(pSourceRect, pDestRect, hDestWindowOverride, pDirtyRegion);
   }
   HRESULT __stdcall GetBackBuffer(UINT iSwapChain, UINT iBackBuffer, D3DBACKBUFFER_TYPE Type, IDirect3DSurface9** ppBackBuffer) override {
      return target->GetBackBuffer(iSwapChain, iBackBuffer, Type, ppBackBuffer);
   }
   HRESULT __stdcall GetRasterStatus(UINT iSwapChain, D3DRASTER_STATUS* pRasterStatus) override {
      return target->GetRasterStatus(iSwapChain, pRasterStatus);
   }
   HRESULT __stdcall SetDialogBoxMode(BOOL bEnableDialogs) override {
      return target->SetDialogBoxMode(bEnableDialogs);
   }
   void __stdcall SetGammaRamp(UINT iSwapChain, DWORD Flags, const D3DGAMMARAMP* pRamp) override {
      return target->SetGammaRamp(iSwapChain, Flags, pRamp);
   }
   void __stdcall GetGammaRamp(UINT iSwapChain, D3DGAMMARAMP* pRamp) override {
      return target->GetGammaRamp(iSwapChain, pRamp);
   }
   HRESULT __stdcall CreateTexture(UINT Width, UINT Height, UINT Levels, DWORD Usage, D3DFORMAT Format, D3DPOOL Pool, IDirect3DTexture9** ppTexture, HANDLE* pSharedHandle) override {
      std::wcout << "At CreateTexture " << Width << " by " << Height << std::endl;

      CreateTextureArgs args = {};
      args.width = Width;
      args.height = Height;
      args.levels = Levels;
      args.usage = Usage;
      args.format = Format;
      args.pool = Pool;
      args.ppTexture = reinterpret_cast<void**>(ppTexture);
      args.pSharedHandle = pSharedHandle;
      
      auto result = target->CreateTexture(Width, Height, Levels, Usage, Format, Pool, ppTexture, pSharedHandle);

      CreateTextureEventArgsPost postArgs = {};
      postArgs.arguments = &args;
      postArgs.retval = result;

      hookEventPublisher->PublishCreateTextureEventPost(&postArgs);
      return result;
   }
   HRESULT __stdcall CreateVolumeTexture(UINT Width, UINT Height, UINT Depth, UINT Levels, DWORD Usage, D3DFORMAT Format, D3DPOOL Pool, IDirect3DVolumeTexture9** ppVolumeTexture, HANDLE* pSharedHandle) override {
      return target->CreateVolumeTexture(Width, Height, Depth, Levels, Usage, Format, Pool, ppVolumeTexture, pSharedHandle);
   }
   HRESULT __stdcall CreateCubeTexture(UINT EdgeLength, UINT Levels, DWORD Usage, D3DFORMAT Format, D3DPOOL Pool, IDirect3DCubeTexture9** ppCubeTexture, HANDLE* pSharedHandle) override {
      return target->CreateCubeTexture(EdgeLength, Levels, Usage, Format, Pool, ppCubeTexture, pSharedHandle);
   }
   HRESULT __stdcall CreateVertexBuffer(UINT Length, DWORD Usage, DWORD FVF, D3DPOOL Pool, IDirect3DVertexBuffer9** ppVertexBuffer, HANDLE* pSharedHandle) override {
      return target->CreateVertexBuffer(Length, Usage, FVF, Pool, ppVertexBuffer, pSharedHandle);
   }
   HRESULT __stdcall CreateIndexBuffer(UINT Length, DWORD Usage, D3DFORMAT Format, D3DPOOL Pool, IDirect3DIndexBuffer9** ppIndexBuffer, HANDLE* pSharedHandle) override {
      return target->CreateIndexBuffer(Length, Usage, Format, Pool, ppIndexBuffer, pSharedHandle);
   }
   HRESULT __stdcall CreateRenderTarget(UINT Width, UINT Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, DWORD MultisampleQuality, BOOL Lockable, IDirect3DSurface9** ppSurface, HANDLE* pSharedHandle) override {
      return target->CreateRenderTarget(Width, Height, Format, MultiSample, MultisampleQuality, Lockable, ppSurface, pSharedHandle);
   }
   HRESULT __stdcall CreateDepthStencilSurface(UINT Width, UINT Height, D3DFORMAT Format, D3DMULTISAMPLE_TYPE MultiSample, DWORD MultisampleQuality, BOOL Discard, IDirect3DSurface9** ppSurface, HANDLE* pSharedHandle) override {
      return target->CreateDepthStencilSurface(Width, Height, Format, MultiSample, MultisampleQuality, Discard, ppSurface, pSharedHandle);
   }
   HRESULT __stdcall UpdateSurface(IDirect3DSurface9* pSourceSurface, const RECT* pSourceRect, IDirect3DSurface9* pDestinationSurface, const POINT* pDestPoint) override {
      return target->UpdateSurface(pSourceSurface, pSourceRect, pDestinationSurface, pDestPoint);
   }
   HRESULT __stdcall UpdateTexture(IDirect3DBaseTexture9* pSourceTexture, IDirect3DBaseTexture9* pDestinationTexture) override {
      return target->UpdateTexture(pSourceTexture, pDestinationTexture);
   }
   HRESULT __stdcall GetRenderTargetData(IDirect3DSurface9* pRenderTarget, IDirect3DSurface9* pDestSurface) override {
      return target->GetRenderTargetData(pRenderTarget, pDestSurface);
   }
   HRESULT __stdcall GetFrontBufferData(UINT iSwapChain, IDirect3DSurface9* pDestSurface) override {
      return target->GetFrontBufferData(iSwapChain, pDestSurface);
   }
   HRESULT __stdcall StretchRect(IDirect3DSurface9* pSourceSurface, const RECT* pSourceRect, IDirect3DSurface9* pDestSurface, const RECT* pDestRect, D3DTEXTUREFILTERTYPE Filter) override {
      return target->StretchRect(pSourceSurface, pSourceRect, pDestSurface, pDestRect, Filter);
   }
   HRESULT __stdcall ColorFill(IDirect3DSurface9* pSurface, const RECT* pRect, D3DCOLOR color) override {
      return target->ColorFill(pSurface, pRect, color);
   }
   HRESULT __stdcall CreateOffscreenPlainSurface(UINT Width, UINT Height, D3DFORMAT Format, D3DPOOL Pool, IDirect3DSurface9** ppSurface, HANDLE* pSharedHandle) override {
      return target->CreateOffscreenPlainSurface(Width, Height, Format, Pool, ppSurface, pSharedHandle);
   }
   HRESULT __stdcall SetRenderTarget(DWORD RenderTargetIndex, IDirect3DSurface9* pRenderTarget) override {
//      std::wcout << "Set Render Target!" << std::endl;
      return target->SetRenderTarget(RenderTargetIndex, pRenderTarget);
   }
   HRESULT __stdcall GetRenderTarget(DWORD RenderTargetIndex, IDirect3DSurface9** ppRenderTarget) override {
//      std::wcout << "Get Render Target!" << std::endl;
      return target->GetRenderTarget(RenderTargetIndex, ppRenderTarget);
   }
   HRESULT __stdcall SetDepthStencilSurface(IDirect3DSurface9* pNewZStencil) override {
      return target->SetDepthStencilSurface(pNewZStencil);
   }
   HRESULT __stdcall GetDepthStencilSurface(IDirect3DSurface9** ppZStencilSurface) override {
      return target->GetDepthStencilSurface(ppZStencilSurface);
   }
   HRESULT __stdcall BeginScene() override {
//      std::wcout << "Begin Scene!" << std::endl;
      return target->BeginScene();
   }
   HRESULT __stdcall EndScene() override {
//      std::wcout << "End Scene!" << std::endl;
      return target->EndScene();
   }
   HRESULT __stdcall Clear(DWORD Count, const D3DRECT* pRects, DWORD Flags, D3DCOLOR Color, float Z, DWORD Stencil) override {
//      std::wcout << "Clear!" << std::endl;
      return target->Clear(Count, pRects, Flags, Color, Z, Stencil);
   }
   HRESULT __stdcall SetTransform(D3DTRANSFORMSTATETYPE State, const D3DMATRIX* pMatrix) override {
      return target->SetTransform(State, pMatrix);
   }
   HRESULT __stdcall GetTransform(D3DTRANSFORMSTATETYPE State, D3DMATRIX* pMatrix) override {
      return target->GetTransform(State, pMatrix);
   }
   HRESULT __stdcall MultiplyTransform(D3DTRANSFORMSTATETYPE transformStateType, const D3DMATRIX* matrix) override {
      return target->MultiplyTransform(transformStateType, matrix);
   }
   HRESULT __stdcall SetViewport(const D3DVIEWPORT9* pViewport) override {
      return target->SetViewport(pViewport);
   }
   HRESULT __stdcall GetViewport(D3DVIEWPORT9* pViewport) override {
      return target->GetViewport(pViewport);
   }
   HRESULT __stdcall SetMaterial(const D3DMATERIAL9* pMaterial) override {
      return target->SetMaterial(pMaterial);
   }
   HRESULT __stdcall GetMaterial(D3DMATERIAL9* pMaterial) override {
      return target->GetMaterial(pMaterial);
   }
   HRESULT __stdcall SetLight(DWORD Index, const D3DLIGHT9* light) override {
      return target->SetLight(Index, light);
   }
   HRESULT __stdcall GetLight(DWORD Index, D3DLIGHT9* light) override {
      return target->GetLight(Index, light);
   }
   HRESULT __stdcall LightEnable(DWORD Index, BOOL Enable) override {
      return target->LightEnable(Index, Enable);
   }
   HRESULT __stdcall GetLightEnable(DWORD Index, BOOL* pEnable) override {
      return target->GetLightEnable(Index, pEnable);
   }
   HRESULT __stdcall SetClipPlane(DWORD Index, const float* pPlane) override {
      return target->SetClipPlane(Index, pPlane);
   }
   HRESULT __stdcall GetClipPlane(DWORD Index, float* pPlane) override {
      return target->GetClipPlane(Index, pPlane);
   }
   HRESULT __stdcall SetRenderState(D3DRENDERSTATETYPE State, DWORD Value) override {
      return target->SetRenderState(State, Value);
   }
   HRESULT __stdcall GetRenderState(D3DRENDERSTATETYPE State, DWORD* pValue) override {
      return target->GetRenderState(State, pValue);
   }
   HRESULT __stdcall CreateStateBlock(D3DSTATEBLOCKTYPE Type, IDirect3DStateBlock9** ppSB) override {
      return target->CreateStateBlock(Type, ppSB);
   }
   HRESULT __stdcall BeginStateBlock() override {
      return target->BeginStateBlock();
   }
   HRESULT __stdcall EndStateBlock(IDirect3DStateBlock9** ppSB) override {
      return target->EndStateBlock(ppSB);
   }
   HRESULT __stdcall SetClipStatus(const D3DCLIPSTATUS9* pClipStatus) override {
      return target->SetClipStatus(pClipStatus);
   }
   HRESULT __stdcall GetClipStatus(D3DCLIPSTATUS9* pClipStatus) override {
      return target->GetClipStatus(pClipStatus);
   }
   HRESULT __stdcall GetTexture(DWORD Stage, IDirect3DBaseTexture9** ppTexture) override {
      return target->GetTexture(Stage, ppTexture);
   }
   HRESULT __stdcall SetTexture(DWORD Stage, IDirect3DBaseTexture9* pTexture) override {
      SetTextureArgs args = {};
      args.stage = Stage;
      args.pTexture = pTexture;
      
      auto result = target->SetTexture(Stage, pTexture);

      SetTextureEventArgsPost postArgs = {};
      postArgs.arguments = &args;
      postArgs.retval = result;

      hookEventPublisher->PublishSetTextureEventPost(&postArgs);

      return result;
   }
   HRESULT __stdcall GetTextureStageState(DWORD Stage, D3DTEXTURESTAGESTATETYPE Type, DWORD* pValue) override {
      return target->GetTextureStageState(Stage, Type, pValue);
   }
   HRESULT __stdcall SetTextureStageState(DWORD Stage, D3DTEXTURESTAGESTATETYPE Type, DWORD Value) override {
      return target->SetTextureStageState(Stage, Type, Value);
   }
   HRESULT __stdcall GetSamplerState(DWORD Sampler, D3DSAMPLERSTATETYPE Type, DWORD* pValue) override {
      return target->GetSamplerState(Sampler, Type, pValue);
   }
   HRESULT __stdcall SetSamplerState(DWORD Sampler, D3DSAMPLERSTATETYPE Type, DWORD Value) override {
      return target->SetSamplerState(Sampler, Type, Value);
   }
   HRESULT __stdcall ValidateDevice(DWORD* pNumPasses) override {
      return target->ValidateDevice(pNumPasses);
   }
   HRESULT __stdcall SetPaletteEntries(UINT PaletteNumber, const PALETTEENTRY* pEntries) override {
      return target->SetPaletteEntries(PaletteNumber, pEntries);
   }
   HRESULT __stdcall GetPaletteEntries(UINT PaletteNumber, PALETTEENTRY* pEntries) override {
      return target->GetPaletteEntries(PaletteNumber, pEntries);
   }
   HRESULT __stdcall SetCurrentTexturePalette(UINT PaletteNumber) override {
      return target->SetCurrentTexturePalette(PaletteNumber);
   }
   HRESULT __stdcall GetCurrentTexturePalette(UINT* PaletteNumber) override {
      return target->GetCurrentTexturePalette(PaletteNumber);
   }
   HRESULT __stdcall SetScissorRect(const RECT* pRect) override {
      return target->SetScissorRect(pRect);
   }
   HRESULT __stdcall GetScissorRect(RECT* pRect) override {
      return target->GetScissorRect(pRect);
   }
   HRESULT __stdcall SetSoftwareVertexProcessing(BOOL bSoftware) override {
      return target->SetSoftwareVertexProcessing(bSoftware);
   }
   BOOL __stdcall GetSoftwareVertexProcessing() override {
      return target->GetSoftwareVertexProcessing();
   }
   HRESULT __stdcall SetNPatchMode(float nSegments) override {
      return target->SetNPatchMode(nSegments);
   }
   float __stdcall GetNPatchMode() override {
      return target->GetNPatchMode();
   }
   HRESULT __stdcall DrawPrimitive(D3DPRIMITIVETYPE PrimitiveType, UINT StartVertex, UINT PrimitiveCount) override {
      return target->DrawPrimitive(PrimitiveType, StartVertex, PrimitiveCount);
   }
   HRESULT __stdcall DrawIndexedPrimitive(D3DPRIMITIVETYPE primitiveType, INT BaseVertexIndex, UINT MinVertexIndex, UINT NumVertices, UINT startIndex, UINT primCount) override {
      return target->DrawIndexedPrimitive(primitiveType, BaseVertexIndex, MinVertexIndex, NumVertices, startIndex, primCount);
   }
   HRESULT __stdcall DrawPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, UINT PrimitiveCount, const void* pVertexStreamZeroData, UINT VertexStreamZeroStride) override {
      return target->DrawPrimitiveUP(PrimitiveType, PrimitiveCount, pVertexStreamZeroData, VertexStreamZeroStride);
   }
   HRESULT __stdcall DrawIndexedPrimitiveUP(D3DPRIMITIVETYPE PrimitiveType, UINT MinVertexIndex, UINT NumVertices, UINT PrimitiveCount, const void* pIndexData, D3DFORMAT IndexDataFormat, const void* pVertexStreamZeroData, UINT VertexStreamZeroStride) override {
      return target->DrawIndexedPrimitiveUP(PrimitiveType, MinVertexIndex, NumVertices, PrimitiveCount, pIndexData, IndexDataFormat, pVertexStreamZeroData, VertexStreamZeroStride);
   }
   HRESULT __stdcall ProcessVertices(UINT SrcStartIndex, UINT DestIndex, UINT VertexCount, IDirect3DVertexBuffer9* pDestBuffer, IDirect3DVertexDeclaration9* pVertexDecl, DWORD Flags) override {
      return target->ProcessVertices(SrcStartIndex, DestIndex, VertexCount, pDestBuffer, pVertexDecl, Flags);
   }
   HRESULT __stdcall CreateVertexDeclaration(const D3DVERTEXELEMENT9* pVertexElements, IDirect3DVertexDeclaration9** ppDecl) override {
      return target->CreateVertexDeclaration(pVertexElements, ppDecl);
   }
   HRESULT __stdcall SetVertexDeclaration(IDirect3DVertexDeclaration9* pDecl) override {
      return target->SetVertexDeclaration(pDecl);
   }
   HRESULT __stdcall GetVertexDeclaration(IDirect3DVertexDeclaration9** ppDecl) override {
      return target->GetVertexDeclaration(ppDecl);
   }
   HRESULT __stdcall SetFVF(DWORD FVF) override {
      return target->SetFVF(FVF);
   }
   HRESULT __stdcall GetFVF(DWORD* pFVF) override {
      return target->GetFVF(pFVF);
   }
   HRESULT __stdcall CreateVertexShader(const DWORD* pFunction, IDirect3DVertexShader9** ppShader) override {
      return target->CreateVertexShader(pFunction, ppShader);
   }
   HRESULT __stdcall SetVertexShader(IDirect3DVertexShader9* pShader) override {
      return target->SetVertexShader(pShader);
   }
   HRESULT __stdcall GetVertexShader(IDirect3DVertexShader9** ppShader) override {
      return target->GetVertexShader(ppShader);
   }
   HRESULT __stdcall SetVertexShaderConstantF(UINT StartRegister, const float* pConstantData, UINT Vector4fCount) override {
      return target->SetVertexShaderConstantF(StartRegister, pConstantData, Vector4fCount);
   }
   HRESULT __stdcall GetVertexShaderConstantF(UINT StartRegister, float* pConstantData, UINT Vector4fCount) override {
      return target->GetVertexShaderConstantF(StartRegister, pConstantData, Vector4fCount);
   }
   HRESULT __stdcall SetVertexShaderConstantI(UINT StartRegister, const int* pConstantData, UINT Vector4iCount) override {
      return target->SetVertexShaderConstantI(StartRegister, pConstantData, Vector4iCount);
   }
   HRESULT __stdcall GetVertexShaderConstantI(UINT StartRegister, int* pConstantData, UINT Vector4iCount) override {
      return target->GetVertexShaderConstantI(StartRegister, pConstantData, Vector4iCount);
   }
   HRESULT __stdcall SetVertexShaderConstantB(UINT StartRegister, const BOOL* pConstantData, UINT BoolCount) override {
      return target->SetVertexShaderConstantB(StartRegister, pConstantData, BoolCount);
   }
   HRESULT __stdcall GetVertexShaderConstantB(UINT StartRegister, BOOL* pConstantData, UINT BoolCount) override {
      return target->GetVertexShaderConstantB(StartRegister, pConstantData, BoolCount);
   }
   HRESULT __stdcall SetStreamSource(UINT StreamNumber, IDirect3DVertexBuffer9* pStreamData, UINT OffsetInBytes, UINT Stride) override {
      return target->SetStreamSource(StreamNumber, pStreamData, OffsetInBytes, Stride);
   }
   HRESULT __stdcall GetStreamSource(UINT StreamNumber, IDirect3DVertexBuffer9** ppStreamData, UINT* pOffsetInBytes, UINT* pStride) override {
      return target->GetStreamSource(StreamNumber, ppStreamData, pOffsetInBytes, pStride);
   }
   HRESULT __stdcall SetStreamSourceFreq(UINT StreamNumber, UINT Setting) override {
      return target->SetStreamSourceFreq(StreamNumber, Setting);
   }
   HRESULT __stdcall GetStreamSourceFreq(UINT StreamNumber, UINT* pSetting) override {
      return target->GetStreamSourceFreq(StreamNumber, pSetting);
   }
   HRESULT __stdcall SetIndices(IDirect3DIndexBuffer9* pIndexData) override {
      return target->SetIndices(pIndexData);
   }
   HRESULT __stdcall GetIndices(IDirect3DIndexBuffer9** ppIndexData) override {
      return target->GetIndices(ppIndexData);
   }
   HRESULT __stdcall CreatePixelShader(const DWORD* pFunction, IDirect3DPixelShader9** ppShader) override {
      return target->CreatePixelShader(pFunction, ppShader);
   }
   HRESULT __stdcall SetPixelShader(IDirect3DPixelShader9* pShader) override {
      return target->SetPixelShader(pShader);
   }
   HRESULT __stdcall GetPixelShader(IDirect3DPixelShader9** ppShader) override {
      return target->GetPixelShader(ppShader);
   }
   HRESULT __stdcall SetPixelShaderConstantF(UINT StartRegister, const float* pConstantData, UINT Vector4fCount) override {
      return target->SetPixelShaderConstantF(StartRegister, pConstantData, Vector4fCount);
   }
   HRESULT __stdcall GetPixelShaderConstantF(UINT StartRegister, float* pConstantData, UINT Vector4fCount) override {
      return target->GetPixelShaderConstantF(StartRegister, pConstantData, Vector4fCount);
   }
   HRESULT __stdcall SetPixelShaderConstantI(UINT StartRegister, const int* pConstantData, UINT Vector4iCount) override {
      return target->SetPixelShaderConstantI(StartRegister, pConstantData, Vector4iCount);
   }
   HRESULT __stdcall GetPixelShaderConstantI(UINT StartRegister, int* pConstantData, UINT Vector4iCount) override {
      return target->GetPixelShaderConstantI(StartRegister, pConstantData, Vector4iCount);
   }
   HRESULT __stdcall SetPixelShaderConstantB(UINT StartRegister, const BOOL* pConstantData, UINT BoolCount) override {
      return target->SetPixelShaderConstantB(StartRegister, pConstantData, BoolCount);
   }
   HRESULT __stdcall GetPixelShaderConstantB(UINT StartRegister, BOOL* pConstantData, UINT BoolCount) override {
      return target->GetPixelShaderConstantB(StartRegister, pConstantData, BoolCount);
   }
   HRESULT __stdcall DrawRectPatch(UINT Handle, const float* pNumSegs, const D3DRECTPATCH_INFO* pRectPatchInfo) override {
      return target->DrawRectPatch(Handle, pNumSegs, pRectPatchInfo);
   }
   HRESULT __stdcall DrawTriPatch(UINT Handle, const float* pNumSegs, const D3DTRIPATCH_INFO* pTriPatchInfo) override {
      return target->DrawTriPatch(Handle, pNumSegs, pTriPatchInfo);
   }
   HRESULT __stdcall DeletePatch(UINT Handle) override {
      return target->DeletePatch(Handle);
   }
   HRESULT __stdcall CreateQuery(D3DQUERYTYPE Type, IDirect3DQuery9** ppQuery) override {
      return target->CreateQuery(Type, ppQuery);
   }
};

class Direct3D9Proxy : public IDirect3D9 {
   IDirect3D9* target;
   Direct3D9HookEventPublisher* hookEventPublisher;

public:
   Direct3D9Proxy(
      Direct3D9HookEventPublisher* hookEventPublisher, 
      IDirect3D9* target
   ) : target(target), hookEventPublisher(hookEventPublisher){
   }
   virtual ~Direct3D9Proxy() {}

   HRESULT __stdcall QueryInterface(const IID& riid, void** ppvObj) override {
      return target->QueryInterface(riid, ppvObj);
   }

   ULONG __stdcall AddRef() override {
      return target->AddRef();
   }

   ULONG __stdcall Release() override {
      return target->Release();
   }

   HRESULT __stdcall RegisterSoftwareDevice(void* pInitializeFunction) override {
      return target->RegisterSoftwareDevice(pInitializeFunction);
   }

   UINT __stdcall GetAdapterCount() override {
      return target->GetAdapterCount();
   }

   HRESULT __stdcall GetAdapterIdentifier(UINT Adapter, DWORD Flags, D3DADAPTER_IDENTIFIER9* pIdentifier) override {
      return target->GetAdapterIdentifier(Adapter, Flags, pIdentifier);
   }

   UINT __stdcall GetAdapterModeCount(UINT Adapter, D3DFORMAT Format) override {
      return target->GetAdapterModeCount(Adapter, Format);
   }

   HRESULT __stdcall EnumAdapterModes(UINT Adapter, D3DFORMAT Format, UINT Mode, D3DDISPLAYMODE* pMode) override {
      return target->EnumAdapterModes(Adapter, Format, Mode, pMode);
   }

   HRESULT __stdcall GetAdapterDisplayMode(UINT Adapter, D3DDISPLAYMODE* pMode) override {
      return target->GetAdapterDisplayMode(Adapter, pMode);
   }

   HRESULT __stdcall CheckDeviceType(UINT Adapter, D3DDEVTYPE DevType, D3DFORMAT AdapterFormat, D3DFORMAT BackBufferFormat, BOOL bWindowed) override {
      return target->CheckDeviceType(Adapter, DevType, AdapterFormat, BackBufferFormat, bWindowed);
   }

   HRESULT __stdcall CheckDeviceFormat(UINT Adapter, D3DDEVTYPE DeviceType, D3DFORMAT AdapterFormat, DWORD Usage, D3DRESOURCETYPE RType, D3DFORMAT CheckFormat) override {
      return target->CheckDeviceFormat(Adapter, DeviceType, AdapterFormat, Usage, RType, CheckFormat);
   }

   HRESULT __stdcall CheckDeviceMultiSampleType(UINT Adapter, D3DDEVTYPE DeviceType, D3DFORMAT SurfaceFormat, BOOL Windowed, D3DMULTISAMPLE_TYPE MultiSampleType, DWORD* pQualityLevels) override {
      return target->CheckDeviceMultiSampleType(Adapter, DeviceType, SurfaceFormat, Windowed, MultiSampleType, pQualityLevels);
   }

   HRESULT __stdcall CheckDepthStencilMatch(UINT Adapter, D3DDEVTYPE DeviceType, D3DFORMAT AdapterFormat, D3DFORMAT RenderTargetFormat, D3DFORMAT DepthStencilFormat) override {
      return target->CheckDepthStencilMatch(Adapter, DeviceType, AdapterFormat, RenderTargetFormat, DepthStencilFormat);
   }

   HRESULT __stdcall CheckDeviceFormatConversion(UINT Adapter, D3DDEVTYPE DeviceType, D3DFORMAT SourceFormat, D3DFORMAT TargetFormat) override {
      return target->CheckDeviceFormatConversion(Adapter, DeviceType, SourceFormat, TargetFormat);
   }

   HRESULT __stdcall GetDeviceCaps(UINT Adapter, D3DDEVTYPE DeviceType, D3DCAPS9* pCaps) override {
      return target->GetDeviceCaps(Adapter, DeviceType, pCaps);
   }

   HMONITOR __stdcall GetAdapterMonitor(UINT Adapter) override {
      return target->GetAdapterMonitor(Adapter);
   }

   HRESULT __stdcall CreateDevice(UINT Adapter, D3DDEVTYPE DeviceType, HWND hFocusWindow, DWORD BehaviorFlags, D3DPRESENT_PARAMETERS* pPresentationParameters, IDirect3DDevice9** ppReturnedDeviceInterface) override {
      std::wcout << "Handling CreateDevice " << Adapter << " " << DeviceType << " " << hFocusWindow << " " << BehaviorFlags << " " << pPresentationParameters << " " << ppReturnedDeviceInterface << std::endl;
      IDirect3DDevice9* actualDevice;
      auto result = target->CreateDevice(Adapter, DeviceType, hFocusWindow, BehaviorFlags, pPresentationParameters, &actualDevice);
      
      CreateDeviceEventArgsPost postArgs = {};
      postArgs.retval = actualDevice;

      hookEventPublisher->PublishCreateDeviceEventPost(&postArgs);

      *ppReturnedDeviceInterface = new Direct3DDevice9Proxy(hookEventPublisher, this, actualDevice);
      return result;
   }
};

IDirect3D9* WINAPI Direct3D9Subsystem::MyDirect3DCreate9(UINT SDKVersion) {
   std::wcout << "Handling MyDirect3DCreate9 with version " << SDKVersion << "." << std::endl;
   auto targetDirect3DObject = m_trampDirect3DCreate9(SDKVersion);
   return new Direct3D9Proxy(direct3D9HookEventPublisher, targetDirect3DObject);
}