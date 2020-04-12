#include "IO.h"
#include <Windows.h>
#include <tchar.h>

using namespace std;

HANDLE MOpenFile(LPCWSTR lpFileName, DWORD dwDesiredAccess = GENERIC_READ) {
	//#define FILE_SHARE_READ                 0x00000001
	//#define FILE_SHARE_WRITE                0x00000002  
	//#define OPEN_EXISTING       3
	//#define FILE_ATTRIBUTE_NORMAL               0x00000080  
	//#define GENERIC_READ                     (0x80000000L)
	_tprintf_s(L"MCreateFile:We got %ls", lpFileName);
	return CreateFile(lpFileName, dwDesiredAccess, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
}

HANDLE MCreateFile(LPCWSTR lpFileName, DWORD dwCreationDisposition = CREATE_ALWAYS) {
	_tprintf_s(L"MCreateFile:We got %ls", lpFileName);
	return CreateFile(lpFileName,GENERIC_ALL,FILE_SHARE_READ,NULL, dwCreationDisposition, FILE_ATTRIBUTE_NORMAL, NULL);
}
void dahua_get_date(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount) {
	if (nOffsetSec == nullptr) {
		printf("The OffsetSec can't be null!");
		return;
	}
	if (nFileCount == nullptr) {
		printf("The nFileCount can't be null!");
		return;
	}

	for (size_t i = 0; i < 100; i++)
	{
		*nOffsetSec = i++;
		*nFileCount = i * 2;
		Sleep(1000);
	}
}

HANDLE MCreateFileFromUnicode(UINT16 *codes, INT32 length) {
	auto fileName = (LPCWSTR) Mmalloc(2 * length + 2);
	Mmemset((void *)fileName, 0, 2 * length + 2);
	Mmemcpy((void *) fileName, codes ,2 * length);

	wprintf(_T("MCreateFileFromUnicode:We got %s the length is %d"), fileName,length);
	auto handle = MCreateFile(fileName);
	delete fileName;

	return handle;
}

HANDLE MOpenFileFromUnicode(UINT16 *codes, INT32 length) {
	auto fileName = (LPCWSTR)Mmalloc(2 * length + 2);
	Mmemset((void *)fileName, 0, 2 * length + 2);
	Mmemcpy((void *)fileName, codes, 2 * length);
	wprintf(_T("MOpenFileFromUnicode:We got %s the length is %d"), fileName, length);
	
	auto handle = MOpenFile(fileName);
	delete fileName;

	return handle;
}

UINT MReadFile(_In_ HANDLE hFile,
	_Out_writes_bytes_to_opt_(nNumberOfBytesToRead, *lpNumberOfBytesRead) __out_data_source(FILE) LPVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToRead) {
	if (hFile == nullptr) {
		return 0;
	}
	else {
		DWORD numberOfBytesRead = 0;
		ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, &numberOfBytesRead, nullptr);
		return numberOfBytesRead;
	}
}

UINT GetFsType(HANDLE handle) {
	char buffer[7];
	MReadFile(handle, buffer, 7);
	if (buffer[0] == 'D' && buffer[1] == 'H') {
		return 2;
	}
	return GetFileSize(handle, nullptr);
}

//自行封装的关闭文件;
BOOL MCloseHandle(
	_In_ HANDLE hObject
) {
	if (hObject == nullptr) {
		return false;
	}
	else {
		return CloseHandle(hObject);

	}
}
//以扇区对齐的方式读取文件，以应对windows读取设备的限制(必须以整数倍扇区读取);
UINT MReadFileInSector(HANDLE hFile,
	_Out_writes_bytes_to_opt_(nNumberOfBytesToRead, *lpNumberOfBytesRead) __out_data_source(FILE) LPVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToRead,int nSecSize,UINT64 nStartPos) {
	auto readSize = 0;
	auto endIndex = nStartPos + nNumberOfBytesToRead;
	auto secBuffer = (byte *)Mmalloc(nSecSize);

	//记录初始/终止位置是否在扇区边界上;
	auto bStartBySector = nStartPos % nSecSize == 0;
	auto bEndBySector = endIndex % nSecSize == 0;

	//先行判断读取位置是否能够被扇区整除;
	if (!bStartBySector) {
		//若不能;则读取一个扇区大小,迁移至缓冲区中;
		auto s2 = MSetFilePointer(hFile, nStartPos / nSecSize * nSecSize ,FILE_BEGIN);	//跳转至扇区倍数位置;

		auto preSize = MReadFile(hFile, secBuffer, nSecSize);
		if (preSize == nSecSize) {
			Mmemcpy(
				(byte *)lpBuffer,
				secBuffer + (nStartPos % nSecSize), 
				nSecSize - nStartPos % nSecSize);
		}
		readSize += nSecSize - nStartPos % nSecSize;
	}

	//读取中段能够被整除的内容;
	auto midRSize = (endIndex / nSecSize * nSecSize - nStartPos) / nSecSize * nSecSize;
	auto midSize = MReadFile(hFile, 
		(byte *)lpBuffer +  (bStartBySector ? 0 : ( nSecSize - nStartPos % nSecSize )), 
		midRSize);

	if (midSize == midRSize) {
		readSize += midSize;
	}

	//读取后段的内容;
	if (!bEndBySector) {
		auto endSize = MReadFile(hFile, secBuffer, nSecSize);
		Mmemcpy((byte *) lpBuffer + (bStartBySector ? 0 : (nSecSize - nStartPos % nSecSize)) + midSize, 
			secBuffer, endIndex % nSecSize);
		readSize += endIndex % nSecSize;
	}

	
	delete secBuffer;
	return readSize;
}

//自行封装的跳转文件;
UINT MSetFilePointer(
	_In_ HANDLE hFile,
	_In_ UINT64 lDistanceToMove,
	_In_ DWORD dwMoveMethod
) {
	if (hFile != nullptr) {
		auto posLow = (LONG)((lDistanceToMove << 32) >> 32);
		auto posHigh = (LONG)(lDistanceToMove >> 32);

		return SetFilePointer(hFile, posLow, &posHigh, dwMoveMethod);
	}
	else {
		return 0;
	}
}

//自行封装的写入文件;
UINT MWriteFile(
	_In_ HANDLE hFile,
	_In_reads_bytes_opt_(nNumberOfBytesToWrite) LPCVOID lpBuffer,
	_In_ DWORD nNumberOfBytesToWrite
) {
	DWORD writeSize = 0;
	return WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite,&writeSize, nullptr);
	return writeSize;
}

unsigned unsigned __int64 cdfc_common_imagefile_size(HANDLE hFile)
{
	if (hFile == nullptr) {
		return 0;
	}
	ULARGE_INTEGER f_l;
	f_l.LowPart = GetFileSize(hFile, &f_l.HighPart);
	unsigned __int64 tFileSize1 = f_l.QuadPart;

	return tFileSize1;
}

bool cdfc_common_read(HANDLE hDisk, unsigned __int64 nPos, char *szBuffer, unsigned __int64 nSize, DWORD *nDwSize, bool bPos)
{
	LARGE_INTEGER s_li;
	if (bPos)
	{
		s_li.QuadPart = nPos;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	}
	*nDwSize = MReadFile(hDisk, szBuffer, nSize);
	return *nDwSize != 0;
}
