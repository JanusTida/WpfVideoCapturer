/*
WFS所需恢复方法;
*/
#pragma once
#include "Common.h"
#include "Common_Recovery.h"

extern "C" _declspec(dllexport) bool cdfc_wfs_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_wfs_search_start_free(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_wfs_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_wfs_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_wfs_filelist();

extern "C" _declspec(dllexport) void cdfc_wfs_exit();

extern "C" _declspec(dllexport) void cdfc_wfs_stop();

extern "C" _declspec(dllexport) void cdfc_wfs_date_converter(UINT64 date, UINT32* resDate);

//extern "C" _declspec(dllexport) void cdfc_wfs_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_wfs_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_wfs_search_start_free(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_wfs_filelist_system();

extern "C" _declspec(dllexport) void cdfc_wfs_set_preview(UINT64 nSize);
							    
extern "C" _declspec(dllexport) bool cdfc_wfs_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);
							    
extern "C" _declspec(dllexport) bool cdfc_wfs_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);
							    
extern "C" _declspec(dllexport) bool cdfc_wfs_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);
							    
extern "C" _declspec(dllexport) bool cdfc_wfs_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

const byte WFSFramePin[] = { 0x00,0x00,0x01,0xfc };				//wfs签名标识;

struct StWFSFrame {
	UINT32 FramePin;
	UINT32 UnKnown0;
	UINT32 DateNum;
	UINT32 UnKnown1;
	byte UnKnown2[40];
	byte FrameNo;
	byte UnKnown3[7];
};
