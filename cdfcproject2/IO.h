#pragma once
#include "Windows.h"
#include "Common.h"
//#include <atlstr.h>

#define CreateRealOnlyFile(lpFileName) MOpenFile(lpFileName,GENERIC_READ)
//自行封装的打开文件;
extern "C" _declspec(dllexport) HANDLE MOpenFile(LPCWSTR lpFileName, DWORD dwDesiredAccess);
extern "C" _declspec(dllexport) HANDLE MCreateFile(LPCWSTR lpFileName, DWORD dwCreationDisposition);
extern "C" _declspec(dllexport) HANDLE MCreateFileFromUnicode(UINT16 *codes, INT32 length);
extern "C" _declspec(dllexport) HANDLE MOpenFileFromUnicode(UINT16 *codes, INT32 length);

extern "C" _declspec(dllexport) UINT GetFsType(HANDLE handle);

extern "C" _declspec(dllexport) void dahua_get_date(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount);

//自行封装的读取文件;
extern "C" _declspec(dllexport) UINT MReadFile(_In_ HANDLE hFile,
	_Out_writes_bytes_to_opt_(nNumberOfBytesToRead, *lpNumberOfBytesRead) __out_data_source(FILE) LPVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToRead);

//自行封装的关闭文件;
extern "C" _declspec(dllexport) BOOL MCloseHandle(
	_In_ HANDLE hObject
);

//以扇区对齐的方式读取文件，以应对windows读取设备的限制;

UINT MReadFileInSector(HANDLE hFile,
	_Out_writes_bytes_to_opt_(nNumberOfBytesToRead, *lpNumberOfBytesRead) __out_data_source(FILE) LPVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToRead,int nSecSize,UINT64 nStartPos);

//自行封装的跳转文件;
UINT MSetFilePointer(
	_In_ HANDLE hFile,
	_In_ UINT64 lDistanceToMove,
	_In_ DWORD dwMoveMethod
);
//自行封装的写入文件;
UINT MWriteFile(
	_In_ HANDLE hFile,
	_In_reads_bytes_opt_(nNumberOfBytesToWrite) LPCVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToWrite
);

extern "C" _declspec(dllexport) unsigned __int64 cdfc_common_imagefile_size(HANDLE hFile);
extern "C" _declspec(dllexport) bool cdfc_common_read(HANDLE hDisk, unsigned __int64 nPos, char *szBuffer, unsigned __int64 nSize, DWORD *nDwSize, bool bPos);


