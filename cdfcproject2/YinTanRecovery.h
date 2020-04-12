/*
//鹰潭所需恢复方法;
*/

#pragma once
#include "Common.h"
#include "Common_Recovery.h"

extern "C" _declspec(dllexport) bool cdfc_tdwy_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_tdwy_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_tdwy_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_tdwy_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_tdwy_filelist();

extern "C" _declspec(dllexport) void cdfc_tdwy_exit();

extern "C" _declspec(dllexport) void cdfc_tdwy_stop();

extern "C" _declspec(dllexport) void cdfc_tdwy_date_converter(UINT64 date, UINT32* resDate);

//extern "C" _declspec(dllexport) void cdfc_tdwy_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_tdwy_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_tdwy_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_tdwy_filelist_system();

extern "C" _declspec(dllexport) void cdfc_tdwy_set_preview(UINT64 nSize);

extern "C" _declspec(dllexport) bool cdfc_tdwy_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_tdwy_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_tdwy_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

extern "C" _declspec(dllexport) bool cdfc_tdwy_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

const byte YinTanFrameHeader[] = { 0x00,0x00,0x00,0x02 };
const byte YinTanFramePin[] = { 0x06,0xf0,0x10,0x54 };				//鹰潭关键帧签名标识;

//鹰潭帧;
#pragma pack(1)
struct StYinTanFrame {
public:
	UINT32 FrameHeader;					//帧头;0x00000002;
	UINT16 FrameLength;					//帧长度;
	byte Unknown0[6];
	UINT32 FrameNo;						//帧号;
	byte Unknown1[20];
	UINT32 FramePin;					//关键帧标识;0x5410F006;
	byte UnKnown3[11];
	UINT32 DateNum;						//时间(仅对关键帧有效);				
	UINT32 GetFrameNum() {
		return FrameNo;
	}
	UINT32 GetDateNum() {
		if (IsImportFrame()) {
			return DateNum;
		}
		return 0;
	}
	//是否为关键帧;
	bool IsImportFrame() {
		return FramePin == 0x5410F006;
	}
};
#pragma pack()