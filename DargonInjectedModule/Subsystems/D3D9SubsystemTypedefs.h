#pragma once

#include "stdafx.h"

//-------------------------------------------------------------------------------------------------
// ::Direct3DCreate9
//-------------------------------------------------------------------------------------------------
typedef IDirect3D9* ( WINAPI FunctionDirect3DCreate9 ) (UINT SDKVersion);
typedef FunctionDirect3DCreate9* PFunctionDirect3DCreate9;
typedef void ( FunctionDirect3DCreate9NoCC ) (UINT SDKVersion);
typedef FunctionDirect3DCreate9NoCC* PFunctionDirect3DCreate9NoCC;

//-------------------------------------------------------------------------------------------------
// ::Direct3DCreate9Ex
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionDirect3DCreate9Ex)(UINT SDKVersion, IDirect3D9Ex**);
typedef FunctionDirect3DCreate9Ex* PFunctionDirect3DCreate9Ex;
typedef void (FunctionDirect3DCreate9ExNoCC)(UINT SDKVersion, IDirect3D9Ex**);
typedef FunctionDirect3DCreate9ExNoCC* PFunctionDirect3DCreate9ExNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3D9::CreateDevice
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionCreateDevice)(IDirect3D9* FAR pThis, UINT adapter, D3DDEVTYPE deviceType, 
                                              HWND hFocusWindow, DWORD BehaviorFlags, D3DPRESENT_PARAMETERS* pPresentationParameters, 
                                              IDirect3DDevice9** ppReturnedDeviceInterface);
typedef FunctionCreateDevice* PFunctionCreateDevice;
typedef void (FunctionCreateDeviceNoCC)(IDirect3D9* FAR pThis, UINT adapter, D3DDEVTYPE deviceType, 
                                        HWND hFocusWindow, DWORD BehaviorFlags, D3DPRESENT_PARAMETERS* pPresentationParameters, 
                                        IDirect3DDevice9** ppReturnedDeviceInterface);
typedef FunctionCreateDeviceNoCC* PFunctionCreateDeviceNoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXCreateTextureFromFileInMemory
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXCreateTextureFromFileInMemory)(LPDIRECT3DDEVICE9 pDevice, LPCVOID pSrcData, UINT srcDataSize, LPDIRECT3DTEXTURE9* ppTexture);
typedef FunctionD3DXCreateTextureFromFileInMemory* PFunctionD3DXCreateTextureFromFileInMemory;
typedef FunctionD3DXCreateTextureFromFileInMemory FunctionCreateTextureFromMemory;
typedef FunctionCreateTextureFromMemory* PFunctionCreateTextureFromMemory;

//typedef void (FunctionD3DXCreateTextureFromFileInMemoryNoCC)(LPDIRECT3DDEVICE9 pDevice, LPCVOID pSrcData, UINT srcDataSize, LPDIRECT3DTEXTURE9* ppTexture);
//typedef FunctionD3DXCreateTextureFromFileInMemoryNoCC* PFunctionD3DXCreateTextureFromFileInMemoryNoCC;

// ::D3DXCreateTextureFromFileInMemory => CreateTextureFromMemory Alias
//typedef FunctionD3DXCreateTextureFromFileInMemoryNoCC FunctionCreateTextureFromMemoryNoCC;
//typedef FunctionCreateTextureFromMemoryNoCC* PFunctionCreateTextureFromMemoryNoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXLoadSurfaceFromMemory
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXLoadSurfaceFromMemory)(LPDIRECT3DSURFACE9 pDestSurface, const PALETTEENTRY *pDestPalette, const RECT *pDestRect, LPCVOID pSrcMemory, D3DFORMAT SrcFormat, UINT SrcPitch, const PALETTEENTRY *pSrcPalette, const RECT *pSrcRect, DWORD Filter, D3DCOLOR ColorKey);
typedef FunctionD3DXLoadSurfaceFromMemory* PFunctionD3DXLoadSurfaceFromMemory;
typedef void (FunctionD3DXLoadSurfaceFromMemoryNoCC)(LPDIRECT3DSURFACE9 pDestSurface, const PALETTEENTRY *pDestPalette, const RECT *pDestRect, LPCVOID pSrcMemory, D3DFORMAT SrcFormat, UINT SrcPitch, const PALETTEENTRY *pSrcPalette, const RECT *pSrcRect, DWORD Filter, D3DCOLOR ColorKey);
typedef FunctionD3DXLoadSurfaceFromMemoryNoCC* PFunctionD3DXLoadSurfaceFromMemoryNoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXLoadSurfaceFromFileInMemory
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXLoadSurfaceFromFileInMemory) (LPDIRECT3DSURFACE9 pDestSurface, CONST PALETTEENTRY* pDestPalette, CONST RECT* pDestRect, LPCVOID pSrcData, UINT SrcDataSize, CONST RECT* pSrcRect, DWORD Filter, D3DCOLOR ColorKey, D3DXIMAGE_INFO* pSrcInfo);
typedef FunctionD3DXLoadSurfaceFromFileInMemory* PFunctionD3DXLoadSurfaceFromFileInMemory;
typedef void (FunctionD3DXLoadSurfaceFromFileInMemoryNoCC) (LPDIRECT3DSURFACE9 pDestSurface, CONST PALETTEENTRY* pDestPalette, CONST RECT* pDestRect, LPCVOID pSrcData, UINT SrcDataSize, CONST RECT* pSrcRect, DWORD Filter, D3DCOLOR ColorKey, D3DXIMAGE_INFO* pSrcInfo);
typedef FunctionD3DXLoadSurfaceFromFileInMemoryNoCC* PFunctionD3DXLoadSurfaceFromFileInMemoryNoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXGetImageInfoFromFileInMemory
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXGetImageInfoFromFileInMemory)(LPCVOID pSrcData, UINT SrcDataSize, D3DXIMAGE_INFO *pSrcInfo);
typedef FunctionD3DXGetImageInfoFromFileInMemory* PFunctionD3DXGetImageInfoFromFileInMemory;
typedef void (FunctionD3DXGetImageInfoFromFileInMemoryNoCC)(LPCVOID pSrcData, UINT SrcDataSize, D3DXIMAGE_INFO *pSrcInfo);
typedef FunctionD3DXGetImageInfoFromFileInMemoryNoCC* PFunctionD3DXGetImageInfoFromFileInMemoryNoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXCreateTextureFromFileInMemoryEx
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXCreateTextureFromFileInMemoryEx)(LPDIRECT3DDEVICE9 pDevice, LPCVOID pSrcData, UINT srcDataSize, UINT width, UINT height, UINT mipLevels, DWORD usage, D3DFORMAT format, D3DPOOL pool, DWORD filter, DWORD mipFilter, D3DCOLOR colorKey, D3DXIMAGE_INFO* pSrcInfo, PALETTEENTRY* pPalette, LPDIRECT3DTEXTURE9* ppTexture);
typedef FunctionD3DXCreateTextureFromFileInMemoryEx* PFunctionD3DXCreateTextureFromFileInMemoryEx;
typedef FunctionD3DXCreateTextureFromFileInMemoryEx FunctionCreateTextureFromMemoryEx;
typedef PFunctionD3DXCreateTextureFromFileInMemoryEx PFunctionCreateTextureFromMemoryEx;

//-------------------------------------------------------------------------------------------------
// ::D3DXCreateFontA
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXCreateFontA)(LPDIRECT3DDEVICE9 pDevice, INT height, UINT width, UINT weight, UINT mipLevels, BOOL italic, DWORD charSet, DWORD outputPrecision, DWORD quality, DWORD pitchAndFamily, LPCSTR pFaceName, LPD3DXFONT* ppFont);
typedef FunctionD3DXCreateFontA* PFunctionD3DXCreateFontA;
typedef void (FunctionD3DXCreateFontANoCC)(LPDIRECT3DDEVICE9 pDevice, INT height, UINT width, UINT weight, UINT mipLevels, BOOL italic, DWORD charSet, DWORD outputPrecision, DWORD quality, DWORD pitchAndFamily, LPCSTR pFaceName, LPD3DXFONT* ppFont);
typedef FunctionD3DXCreateFontANoCC* PFunctionD3DXCreateFontANoCC;

//-------------------------------------------------------------------------------------------------
// ::D3DXCreateFontW
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionD3DXCreateFontW) (LPDIRECT3DDEVICE9 pDevice, INT height, UINT width, UINT weight, UINT mipLevels, BOOL italic, DWORD charSet, DWORD outputPrecision, DWORD quality, DWORD pitchAndFamily, LPCWSTR pFaceName, LPD3DXFONT* ppFont);
typedef FunctionD3DXCreateFontW* PFunctionD3DXCreateFontW;
typedef void (FunctionD3DXCreateFontWNoCC) (LPDIRECT3DDEVICE9 pDevice, INT height, UINT width, UINT weight, UINT mipLevels, BOOL italic, DWORD charSet, DWORD outputPrecision, DWORD quality, DWORD pitchAndFamily, LPCWSTR pFaceName, LPD3DXFONT* ppFont);
typedef FunctionD3DXCreateFontWNoCC* PFunctionD3DXCreateFontWNoCC;


//typedef void (FunctionD3DXCreateTextureFromFileInMemoryExNoCC)(LPDIRECT3DDEVICE9 pDevice, LPCVOID pSrcData, UINT srcDataSize, UINT width, UINT height, UINT mipLevels, DWORD usage, D3DFORMAT format, D3DPOOL pool, DWORD filter, DWORD mipFilter, D3DCOLOR colorKey, D3DXIMAGE_INFO* pSrcInfo, PALETTEENTRY* pPalette, LPDIRECT3DTEXTURE9* ppTexture);
//typedef FunctionD3DXCreateTextureFromFileInMemoryExNoCC* PFunctionD3DXCreateTextureFromFileInMemoryExNoCC;

// ::D3DXCreateTextureFromFileInMemoryEx => CreateTextureFromMemoryEx Alias
//typedef FunctionD3DXCreateTextureFromFileInMemoryExNoCC FunctionCreateTextureFromMemoryExNoCC;
//typedef PFunctionD3DXCreateTextureFromFileInMemoryExNoCC PFunctionCreateTextureFromMemoryExNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::SetRenderTarget
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionSetRenderTarget) (IDirect3DDevice9* FAR m_pDevice, DWORD RenderTargetIndex, IDirect3DSurface9* pRenderTarget);
typedef FunctionSetRenderTarget* PFunctionSetRenderTarget;
typedef void (FunctionSetRenderTargetNoCC) (IDirect3DDevice9* FAR m_pDevice, DWORD RenderTargetIndex, IDirect3DSurface9* pRenderTarget);
typedef FunctionSetRenderTargetNoCC* PFunctionSetRenderTargetNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::Reset
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionReset)(IDirect3DDevice9* FAR m_pDevice, D3DPRESENT_PARAMETERS* pPresentationParameters);
typedef FunctionReset* PFunctionReset;
typedef void (FunctionResetNoCC)(IDirect3DDevice9* FAR m_pDevice, D3DPRESENT_PARAMETERS* pPresentationParameters);
typedef FunctionResetNoCC* PFunctionResetNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::Present
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionPresent) (IDirect3DDevice9* FAR m_pDevice, CONST RECT* pSourceRect,CONST RECT* pDestRect,HWND hDestWindowOverride,CONST RGNDATA* pDirtyRegion);
typedef FunctionPresent* PFunctionPresent;
typedef void (FunctionPresentNoCC) (IDirect3DDevice9* FAR m_pDevice, CONST RECT* pSourceRect,CONST RECT* pDestRect,HWND hDestWindowOverride,CONST RGNDATA* pDirtyRegion);
typedef FunctionPresentNoCC* PFunctionPresentNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::CreateTexture
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionCreateTexture) (IDirect3DDevice9* pDevice, UINT width, UINT height, UINT levels, DWORD usage, D3DFORMAT format, D3DPOOL pool, IDirect3DTexture9** ppTexture, HANDLE* pSharedHandle);
typedef FunctionCreateTexture* PFunctionCreateTexture;
typedef void (FunctionCreateTextureNoCC) (IDirect3DDevice9* pDevice, UINT width, UINT height, UINT levels, DWORD usage, D3DFORMAT format, D3DPOOL pool, IDirect3DTexture9** ppTexture, HANDLE* pSharedHandle);
typedef FunctionCreateTextureNoCC* PFunctionCreateTextureNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::CreateVertexBuffer
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionCreateVertexBuffer) (IDirect3DDevice9* pDevice, UINT length, DWORD usage, DWORD fvf, D3DPOOL pool, IDirect3DVertexBuffer9** ppVertexBuffer, HANDLE* pSharedHandle);
typedef FunctionCreateVertexBuffer* PFunctionCreateVertexBuffer;
typedef void (FunctionCreateVertexBufferNoCC) (IDirect3DDevice9* pDevice, UINT length, DWORD usage, DWORD fvf, D3DPOOL pool, IDirect3DVertexBuffer9** ppVertexBuffer, HANDLE* pSharedHandle);
typedef FunctionCreateVertexBufferNoCC* PFunctionCreateVertexBufferNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::BeginScene
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionBeginScene) ( IDirect3DDevice9* FAR m_pDevice );
typedef FunctionBeginScene* PFunctionBeginScene;
typedef void (FunctionBeginSceneNoCC) ( IDirect3DDevice9* FAR m_pDevice );
typedef FunctionBeginSceneNoCC* PFunctionBeginSceneNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::EndScene
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionEndScene) ( IDirect3DDevice9* FAR m_pDevice );
typedef FunctionEndScene* PFunctionEndScene;
typedef void (FunctionEndSceneNoCC) ( IDirect3DDevice9* FAR m_pDevice );
typedef FunctionEndSceneNoCC* PFunctionEndSceneNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::Clear
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionClear) (IDirect3DDevice9* FAR m_pDevice, DWORD Count, CONST D3DRECT* pRects, DWORD Flags, D3DCOLOR Color, float Z, DWORD Stencil);
typedef FunctionClear* PFunctionClear;
typedef void (FunctionClearNoCC) (IDirect3DDevice9* FAR m_pDevice, DWORD Count, CONST D3DRECT* pRects, DWORD Flags, D3DCOLOR Color, float Z, DWORD Stencil);
typedef FunctionClearNoCC* PFunctionClearNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::SetSamplerState
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionSetSamplerState)(IDirect3DDevice9* m_pDevice, DWORD sampler, D3DSAMPLERSTATETYPE type, DWORD value);
typedef FunctionSetSamplerState* PFunctionSetSamplerState;
typedef void (FunctionSetSamplerStateNoCC)(IDirect3DDevice9* m_pDevice, DWORD sampler, D3DSAMPLERSTATETYPE type, DWORD value);
typedef FunctionSetSamplerStateNoCC* PFunctionSetSamplerStateNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::DrawPrimative
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionDrawPrimative) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT StartVertex, UINT PrimitiveCount);
typedef FunctionDrawPrimative* PFunctionDrawPrimative;
typedef void (FunctionDrawPrimativeNoCC) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT StartVertex, UINT PrimitiveCount);
typedef FunctionDrawPrimativeNoCC* PFunctionDrawPrimativeNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::DrawIndexedPrimative
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionDrawIndexedPrimative) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, INT BaseVertexIndex, UINT MinVertexIndex, UINT NumVertices, UINT startIndex, UINT primCount);
typedef FunctionDrawIndexedPrimative* PFunctionDrawIndexedPrimative;
typedef void (FunctionDrawIndexedPrimativeNoCC) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, INT BaseVertexIndex, UINT MinVertexIndex, UINT NumVertices, UINT startIndex, UINT primCount);
typedef FunctionDrawIndexedPrimativeNoCC* PFunctionDrawIndexedPrimativeNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::DrawPrimativeUP
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionDrawPrimativeUP) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT PrimitiveCount, CONST void* pVertexStreamZeroData, UINT VertexStreamZeroStride);
typedef FunctionDrawPrimativeUP* PFunctionDrawPrimativeUP;
typedef void (FunctionDrawPrimativeUPNoCC) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT PrimitiveCount, CONST void* pVertexStreamZeroData, UINT VertexStreamZeroStride);
typedef FunctionDrawPrimativeUPNoCC* PFunctionDrawPrimativeUPNoCC;

//-------------------------------------------------------------------------------------------------
// IDirect3DDevice9::DrawIndexedPrimativeUP
//-------------------------------------------------------------------------------------------------
typedef HRESULT (WINAPI FunctionDrawIndexedPrimativeUP) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT MinVertexIndex, UINT NumVertices, UINT PrimitiveCount, CONST void* pIndexData, D3DFORMAT IndexDataFormat, CONST void* pVertexStreamZeroData, UINT VertexStreamZeroStride);
typedef FunctionDrawIndexedPrimativeUP* PFunctionDrawIndexedPrimativeUP;
typedef void (FunctionDrawIndexedPrimativeUPNoCC) (IDirect3DDevice9* FAR m_pDevice, D3DPRIMITIVETYPE PrimitiveType, UINT MinVertexIndex, UINT NumVertices, UINT PrimitiveCount, CONST void* pIndexData, D3DFORMAT IndexDataFormat, CONST void* pVertexStreamZeroData, UINT VertexStreamZeroStride);
typedef FunctionDrawIndexedPrimativeUPNoCC* PFunctionDrawIndexedPrimativeUPNoCC;