#pragma once
/*
//大华-NVR所需恢复方法;
*/

#include "Common.h"
#include "Common_Recovery.h"

extern "C" _declspec(dllexport) bool cdfc_dh_nvr_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_dh_nvr_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dh_nvr_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_dh_nvr_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_dh_nvr_filelist();

extern "C" _declspec(dllexport) void cdfc_dh_nvr_exit();

extern "C" _declspec(dllexport) void cdfc_dh_nvr_stop();

extern "C" _declspec(dllexport) void cdfc_dh_nvr_date_converter(UINT64 date, UINT32* resDate);

//extern "C" _declspec(dllexport) void cdfc_dh_nvr_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_dh_nvr_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dh_nvr_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dh_nvr_filelist_system();

extern "C" _declspec(dllexport) void cdfc_dh_nvr_set_preview(UINT64 nSize);

extern "C" _declspec(dllexport) bool cdfc_dh_nvr_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_dh_nvr_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_dh_nvr_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

extern "C" _declspec(dllexport) bool cdfc_dh_nvr_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

const byte DaHuaFrameHeader[] = { 0x44,0x48,0x41,0x56 };

const byte DHNVRImportFramePin =  0xfd;				//大华NVR关键帧签名标识;
const byte DHNVRNormalFramePin = 0xfc;		//大华NVR普通帧签名标识;
const byte DHNVRIgnoreFramePin = 0xf1;		//大华NVR忽略帧签名标识;



//大华NVR帧结构;
#pragma pack(1)
struct StDHNVRFrame {
	UINT32 FrameHeader; //帧头0x44484156;

	byte FramePin;	//帧标识类别;[0xf0,0xf1,0xfd(关键帧)];
	byte Ignored[3];


	UINT32 FrameNo;		//帧号;
	byte Unknown0[2];	
	UINT16 ChannelNo;
	UINT32 DateNum;

	byte UnKnown1[43];
	UINT32 GetFrameNum() {
		return FrameNo;
	}
	UINT32 GetDateNum() {
		return DateNum;
	}
	UINT16 GetChannelNo() {
		if (IsImportFrame()) {
			return ChannelNo + 1;
		}
		return 0;
	}
	bool IsNormalFrame() {
		return FramePin == DHNVRNormalFramePin;
	}
	//是否为关键帧;
	bool IsImportFrame() {
		return FramePin == DHNVRImportFramePin;
	}
};
#pragma pack()
