#pragma once
#include "Common.h"
#include "Common_Recovery.h"
//安联锐士-RSFS所对应方法;

extern "C" _declspec(dllexport) StDateCategory *rsfs_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError);

extern "C" _declspec(dllexport) bool rsfs_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError);

extern "C" _declspec(dllexport) void rsfs_exit(StDateCategory *stFile);

extern "C" _declspec(dllexport) void rsfs_stop();

extern "C" _declspec(dllexport) void rsfs_get_date(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount);

extern "C" _declspec(dllexport) bool rsfs_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize);

extern "C" _declspec(dllexport) void rsfs_date_converter(UINT32 nDate, UINT8 *date);


extern "C" _declspec(dllexport) bool cdfc_rsfs_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);


extern "C" _declspec(dllexport) StDateCategory* cdfc_rsfs_search_start(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_rsfs_search_start_f(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) UINT64 cdfc_rsfs_current_sector();

extern "C" _declspec(dllexport) StDateCategory* cdfc_rsfs_filelist();

extern "C" _declspec(dllexport) void cdfc_rsfs_exit();

extern "C" _declspec(dllexport) void cdfc_rsfs_stop();

extern "C" _declspec(dllexport) void cdfc_rsfs_date_converter(UINT32 date, UINT8* resDate);

//extern "C" _declspec(dllexport) void cdfc_rsfs_freetask(IntPtr stFile);

extern "C" _declspec(dllexport) void cdfc_rsfs_set_regionsize(UINT64 regionSize);

extern "C" _declspec(dllexport) StDateCategory* cdfc_rsfs_search_start_free(HANDLE handle, int type, int* error);

extern "C" _declspec(dllexport) StDateCategory* cdfc_rsfs_filelist_system();

extern "C" _declspec(dllexport) void cdfc_rsfs_set_preview(UINT64 nSize);

extern "C" _declspec(dllexport) bool cdfc_rsfs_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_rsfs_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

extern "C" _declspec(dllexport) bool cdfc_rsfs_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

extern "C" _declspec(dllexport) bool cdfc_rsfs_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

//帧类型标识;
const byte CH_RSFSFramePinP = 'P';
const byte CH_RSFSFramePinI = 'I';

//RSFS头标识("RSFm");
const byte RSFSFrameHeader[] = { 0x52,0x53,0x46,0x6d };

//RSFS帧;
#pragma pack(1)
struct StRSFSFrame {
	private:
		UINT32 FrameHeader;					//帧头;RSFm 0x5253466D;
		UINT8 Pin;							//帧类型,P/I
		UINT8 Unknown0;						//未知;
		UINT16 ChannelNo;					//通道号;(需加一)
		UINT32 FrameNo;						//帧号;
		UINT32 FrameLength;					//帧长度;
	

		UINT64 Date;						//时间;单位为微妙;

	public:
		//获得时间数;
		UINT32 GetDateNum() {
			/*UINT64 DateNum = 0;
			Mmemcpy(&DateNum, &Date,8);
*/
			return Date / 0x0F4240;
		}

		//UINT32 FramePin;					//关键帧标识;0x5410F006;
		//byte UnKnown3[11];
		//UINT32 DateNum;						//时间(仅对关键帧有效);				
		UINT32 GetFrameNum() {
			return FrameNo;
		}

		UINT32 GetChannelNo() {
			return ChannelNo + 1;
		}
		//UINT32 GetDateNum() {
		//	if (IsImportFrame()) {
		//		return DateNum;
		//	}
		//	return 0;
		//}
		//是否为关键帧;
		bool IsImportFrame() {
			return Pin == CH_RSFSFramePinI;
		}
};
#pragma pack()