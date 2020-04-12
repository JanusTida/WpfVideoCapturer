#pragma once
#pragma once
#pragma once
#include "Common_Recovery.h"
#include "Common.h"

#define Tranverse16( X ) ( ( ( ( INT16 )( X ) & 0xff00 ) >> 8 ) | ( ( ( INT16 )( X ) & 0x00ff ) << 8 ) )

extern "C" _declspec(dllexport) StDateCategory* dahua_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError);

extern "C" _declspec(dllexport) bool dahua_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError);

extern "C" _declspec(dllexport) void dahua_exit(void *stFile);

extern "C" _declspec(dllexport) void dahua_stop();

extern "C" _declspec(dllexport) void dahua_get_data(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount);

extern "C" _declspec(dllexport) bool dahua_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize);

extern "C" _declspec(dllexport) void dahua_date_converter(UINT32 nDate, int8_t *date);




//楚河汉界;
extern "C" _declspec(dllexport) bool cdfc_dahua_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_search_start_free(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_dahua_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_filelist_2();

extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_filelist();

extern "C" _declspec(dllexport) void cdfc_dahua_exit();

extern "C" _declspec(dllexport) void cdfc_dahua_stop();

extern "C" _declspec(dllexport) void cdfc_dahua_date_converter(UINT64 date, UINT32* resDate);

//extern "C" _declspec(dllexport) void cdfc_dahua_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_dahua_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_dahua_filelist_system();

extern "C" _declspec(dllexport) void cdfc_dahua_set_preview(UINT64 nSize);

extern "C" _declspec(dllexport) bool cdfc_dahua_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_dahua_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_dahua_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

extern "C" _declspec(dllexport) bool cdfc_dahua_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

//普通视频帧;
const UINT8 NormalDHPin = 0xFC;

//关键视频帧;
const UINT16 BigDHPin = 0xFD;

//大华头标识("HBMS");
const byte DHFrameHeader[] = { 0x44,0x48,0x41,0x56 };

#pragma pack(1)
struct StDHFrame {
private:
	UINT32 Header;     //大华头标识;0x44484156

	byte FramePin;	//帧标识类别;[0xfc,0xf1,0xfd(关键帧)];
	byte Ignored[3];

	UINT16 FrameNo;
	UINT16 Unknown;
	UINT32 Size;		//本帧长度;

	UINT32 DateNum;		//时间;
	byte Unknown2;
public:
	//获得时间数;
	UINT32 GetDateNum() {
		return DateNum;
	}
	//是否为关键帧;
	bool IsImportFrame() {
		return FramePin == BigDHPin;
	}

	byte GetPin() {
		return FramePin;
	}

	//获得帧号1;
	UINT16 GetFrameNum() {
		return FrameNo;
	}

	//获得之前的一个帧号;
	UINT32 GetPreFrameNum() {
		return FrameNo - 1;
	}
};
#pragma pack()


//extern "C" _declspec(dllexport)
//extern "C" _declspec(dllexport)
//extern "C" _declspec(dllexport)
//extern "C" _declspec(dllexport)
//extern "C" _declspec(dllexport)
//extern "C" _declspec(dllexport)

