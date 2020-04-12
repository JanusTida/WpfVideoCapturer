#pragma once
#include "Common.h"
#include "Common_Recovery.h"
//������ʿ-RSFS����Ӧ����;

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

//֡���ͱ�ʶ;
const byte CH_RSFSFramePinP = 'P';
const byte CH_RSFSFramePinI = 'I';

//RSFSͷ��ʶ("RSFm");
const byte RSFSFrameHeader[] = { 0x52,0x53,0x46,0x6d };

//RSFS֡;
#pragma pack(1)
struct StRSFSFrame {
	private:
		UINT32 FrameHeader;					//֡ͷ;RSFm 0x5253466D;
		UINT8 Pin;							//֡����,P/I
		UINT8 Unknown0;						//δ֪;
		UINT16 ChannelNo;					//ͨ����;(���һ)
		UINT32 FrameNo;						//֡��;
		UINT32 FrameLength;					//֡����;
	

		UINT64 Date;						//ʱ��;��λΪ΢��;

	public:
		//���ʱ����;
		UINT32 GetDateNum() {
			/*UINT64 DateNum = 0;
			Mmemcpy(&DateNum, &Date,8);
*/
			return Date / 0x0F4240;
		}

		//UINT32 FramePin;					//�ؼ�֡��ʶ;0x5410F006;
		//byte UnKnown3[11];
		//UINT32 DateNum;						//ʱ��(���Թؼ�֡��Ч);				
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
		//�Ƿ�Ϊ�ؼ�֡;
		bool IsImportFrame() {
			return Pin == CH_RSFSFramePinI;
		}
};
#pragma pack()