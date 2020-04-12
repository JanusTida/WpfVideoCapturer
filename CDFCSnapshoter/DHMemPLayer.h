#pragma once
extern "C" _declspec(dllexport) BOOL DH_PLAY_OpenStream(LONG nPort, PBYTE pFileHeadBuf, DWORD nSize, DWORD nBufPoolSize);
extern "C" _declspec(dllexport) BOOL DH_PLAY_InputData(LONG nPort, PBYTE pBuf, DWORD nSize);
extern "C" _declspec(dllexport) DWORD DH_PLAY_GetLastError(LONG nPort);
extern "C" _declspec(dllexport) BOOL DH_MemPLAY_Init(UINT32 port, HWND containerHandle, UINT8 * stream, UINT32 streamSize);
extern "C" _declspec(dllexport) BOOL DH_MemPLAY_SnapAt(UINT32 nPort, UINT32 milliSeconds, PBYTE bmpBuffer, UINT32 bufferSize, unsigned long * actualSize, UINT32* error);
extern "C" _declspec(dllexport) BOOL DH_MemPLAY_PLAY(UINT32 nPort, UINT32* error);
extern "C" _declspec(dllexport) BOOL DH_MemPLAY_Free(UINT32 nPort);
extern "C" _declspec(dllexport) BOOL DH_MemPLAY_Stop(UINT32 nPort, UINT32* error);