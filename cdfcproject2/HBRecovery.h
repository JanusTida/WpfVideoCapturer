#pragma once
#include "Common.h"
#include "Common_Recovery.h"
//��������Ӧ����;
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

//��Ƶ֡;
//const UINT16 AudioHBPin = 0x1040;
//const UINT32 NormalHBPin2 = 0x01000000;

//��ͨ��Ƶ֡;
const UINT16 NormalHBPin1 = 0x48B0;

//�ؼ���Ƶ֡;
const UINT16 BigHBPin1 = 0x48B1;

//����ͷ��ʶ("HBMS");
const byte HBFrameHeader[] = { 0x48,0x42,0x4d,0x53 };

#pragma pack(1)
struct StHBFrame {
	private:
		UINT32 FrameHeader;					//֡ͷ;HBMS 0x48424d53;
		UINT32 Unknown0;					//δ֪
		UINT16 Pin1;							//֡����;����ֵΪNormalHBPin1,BigHBPin1;
		
		UINT8 FrameNo1;						//֡��1;(���Ϊ1)

		//UINT32 FrameLength;					//֡����(����);

		UINT8 Unknown1;						//δ֪;

		UINT32 DateNum;						//ʱ��;

		UINT8 FrameNo1BackUp;				//֡��1����;

		UINT8 Unknown2[3];			

		UINT32 Pin2;						//֡����2;

		UINT8 Pin3;							//����H264֡;
		
		UINT8 FrameNo2;				//֡��2;(����Ϊe0~fe,f0~f2/e2,���Ϊ2);


	public:
		//���ʱ����;
		UINT32 GetDateNum() {
			return DateNum ;
		}
		//�Ƿ�Ϊ�ؼ�֡;
		bool IsImportFrame() {
			return Pin1 == BigHBPin1;
		}

		UINT16 GetPin1() {
			return Pin1;
		}

		//���֡��1;
		UINT8 GetFrameNum() {
			return FrameNo1;
		}

		//���֮ǰ��֡��һ;
		UINT8 GetPreFrameNum1() {
			return FrameNo1 - 1;
		}

		//���֡��2;
		UINT8 GetFrameNum2() {
			return FrameNo2;
		}

		//����Ƿ�Ϊ���õ�ǰ֡2;
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