#pragma once
#include "Common.h"
#include "Common_Recovery.h"
//汉邦所对应方法;
extern "C" _declspec(dllexport) StDateCategory* hb_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError);

extern "C" _declspec(dllexport) bool hb_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError);

extern "C" _declspec(dllexport) void hb_exit(StDateCategory *stFile);

extern "C" _declspec(dllexport) void hb_stop();

extern "C" _declspec(dllexport) void hb_get_date(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount);

extern "C" _declspec(dllexport) bool hb_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize);

extern "C" _declspec(dllexport) void hb_date_converter(UINT32 nDate, int8_t *date);

extern "C" _declspec(dllexport) bool cdfc_hb_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_hb_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_hb_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_hb_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_hb_filelist();

extern "C" _declspec(dllexport) void cdfc_hb_exit();

extern "C" _declspec(dllexport) void cdfc_hb_stop();

extern "C" _declspec(dllexport) void cdfc_hb_date_converter(UINT64 date, UINT8* resDate);

//extern "C" _declspec(dllexport) void cdfc_hb_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_hb_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_hb_search_start_free(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_hb_filelist_system();

extern "C" _declspec(dllexport) void cdfc_hb_set_preview(UINT64 nSize);

extern "C" _declspec(dllexport) bool cdfc_hb_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_hb_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_hb_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

extern "C" _declspec(dllexport) bool cdfc_hb_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

//音频帧;
//const UINT16 AudioHBPin = 0x1040;
//const UINT32 NormalHBPin2 = 0x01000000;

//普通视频帧;
const UINT16 NormalHBPin1 = 0x48B0;

//关键视频帧;
const UINT16 BigHBPin1 = 0x48B1;

//汉邦头标识("HBMS");
const byte HBFrameHeader[] = { 0x48,0x42,0x4d,0x53 };

#pragma pack(1)
struct StHBFrame {
	private:
		UINT32 FrameHeader;					//帧头;HBMS 0x48424d53;
		UINT32 Unknown0;					//未知
		UINT16 Pin1;							//帧类型;可用值为NormalHBPin1,BigHBPin1;
		
		UINT8 FrameNo1;						//帧号1;(间距为1)

		//UINT32 FrameLength;					//帧长度(可能);

		UINT8 Unknown1;						//未知;

		UINT32 DateNum;						//时间;

		UINT8 FrameNo1BackUp;				//帧号1备份;

		UINT8 Unknown2[3];			

		UINT32 Pin2;						//帧类型2;

		UINT8 Pin3;							//区分H264帧;
		
		UINT8 FrameNo2;				//帧号2;(区间为e0~fe,f0~f2/e2,间距为2);


	public:
		//获得时间数;
		UINT32 GetDateNum() {
			return DateNum ;
		}
		//是否为关键帧;
		bool IsImportFrame() {
			return Pin1 == BigHBPin1;
		}

		UINT16 GetPin1() {
			return Pin1;
		}

		//获得帧号1;
		UINT8 GetFrameNum() {
			return FrameNo1;
		}

		//获得之前的帧号一;
		UINT8 GetPreFrameNum1() {
			return FrameNo1 - 1;
		}

		//获得帧号2;
		UINT8 GetFrameNum2() {
			return FrameNo2;
		}

		//检查是否为可用的前帧2;
		bool CheckIsValidPreFrameNum2(UINT8 preNum2) {
			//0xFE->0xE0
			if (preNum2 == 0xFE) {
				return FrameNo2 == 0xE0;
			}
			//0xF0->0xF2/0xE2
			else if (preNum2 == 0xf0) {
				return FrameNo2 == 0xf2 || FrameNo2 == 0xe2;
			}
			return preNum2 == FrameNo2 - 2;
		}

		
};
#pragma pack()