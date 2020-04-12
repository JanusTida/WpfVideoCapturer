#include "stdafx.h"
extern "C" _declspec(dllexport) bool IniMemoryPreviewer(UINT8* buf, UINT64 bufSize, int *error);
extern "C" _declspec(dllexport) int Play();
extern "C" _declspec(dllexport) void Stop();