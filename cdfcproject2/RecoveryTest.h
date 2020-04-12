#pragma once
#include "Common_Recovery.h"
#include "DHRecovery.h"

//extern "C" _declspec(dllexport) StDateCategory* object_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
//	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError);
extern "C" _declspec(dllexport) StDateCategory* object_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError) {
	return dahua_searchstart(hDisk, eType, nStartSec, nEndSec, nSecSize, nAreaSize, nClusterSize, nLBAPos, bJournal, nError);
}
extern "C" _declspec(dllexport) bool object_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError) {
	return dahua_recover(hDisk, eType, stFile, hFileOut, nCurrSizeDW, nError);
}

extern "C" _declspec(dllexport) void object_exit(void *stFile) {
	dahua_exit(stFile);
}

extern "C" _declspec(dllexport) void object_stop() {
	dahua_stop();
}

extern "C" _declspec(dllexport) void object_get_data(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount) {
	dahua_get_data(nOffsetSec, nFileCount);
}

extern "C" _declspec(dllexport) bool object_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize) {
	return dahua_preview(szFile, eType, hDisk, szBuffer, nBuffSize);
}

extern "C" _declspec(dllexport) void object_date_converter(UINT32 nDate, int8_t *date) {
	dahua_date_converter(nDate, date);
}




