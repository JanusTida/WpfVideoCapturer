#pragma once
#include <time.h>
#include "Common.h"
#include "IO.h"

typedef struct StFtyp;
typedef struct StMoov;
typedef struct StMoovList;

//���Ʒ������
enum eDeviceType
{
	eDeviceType_ERROR,		//δ֪�豸
	eDeviceType_WFS04,		//wfs 0.4
	eDeviceType_DHFS41,		//DHFS 4.1
	eDeviceType_DHFS2,		//DHFS II
	eDeviceType_HIKVISION,	//��������
	eDeviceType_CANON5D,	//Canon EOS 5D Mark III	
	eDeviceType_CANON60D,	//Canon EOS 60D
	eDeviceType_SONYMTS,	//Sony MTS
	eDeviceType_HBMS,		//���� HBMS
	eDeviceType_JVBK		//��άjvbk
};

enum eSearchType
{
	eSearchType_FULL,
	eSearchType_FREE,
	eSearchType_FS,
	eSearchType_QUICK
};

struct StFileFrag {
	UINT64 StartAddress;
	UINT64 StartAddress1;
	UINT64 StartAddress2;
	UINT64 Size;
	int ChannelNO;
	UINT32 StartDate;                  //�ļ���ʼʱ��
	UINT32 EndDate;                    //�ļ�����ʱ��
	StFileFrag* Next;
};

#pragma pack(1)
struct StVideo {
	UINT32 FrameNO;                    //֡��
	UINT32 ChannelNO;                  //ͨ����
	UINT32 StartDate;                  //�ļ���ʼʱ��
	UINT32 EndDate;                    //�ļ�����ʱ��
	UINT64 Size;             //�ļ���С
	UINT64 SizeTrue;
	UINT64 StartAddress;      //�ļ���ʼ��ַ
							  //���ÿռ�ɨ��ʱ�õ�����Ƭ����
	StFileFrag * stStAdd;
	StFtyp *FtypList;
	StMoovList *MoovList;
	StVideo * Next;                    //��һ���ļ�
};
#pragma pack()

#pragma pack(1)
struct StMoov
{
	UINT32 nMoovSize;
	char szMoov[4];
	UINT32 nMvhdSize;
	char szMvhd[4];
	UINT32 nVersion;
	UINT32 nCreateTime;
	UINT32 nModifyTime;
	UINT32 nTimeScale;
	UINT32 nDuration;
};
#pragma pack()

#pragma pack(1)
struct StMoovList
{
	StMoov *stMoov;
	unsigned __int64 nStartAddress;
	int nFlag;
	StMoovList *next;
};

#pragma pack()

#pragma pack(1)
struct StMdat
{
	UINT8 nUnknown[36];
	UINT32 nMdatSize;
};
#pragma pack()

#pragma pack(1)
struct StFtyp
{
	StMdat *stMdat;
	unsigned __int64 nStartAddress;
	int nFlag;
	StFtyp *next;
};
#pragma pack()

struct StDateCategory {
	//�ļ�����ʼʱ�� ������;
	UINT32 Date;	

	//��������µ��ļ� ʱ-��-����ļ�
	StVideo *File;

	StFtyp *FtypList;	

	StMoovList *MoovList;

	//��һ����������
	StDateCategory *Next;
};


//��ʱ�ļ��ṹ;
class TempVideo {
public:
	StVideo* StVideo;
	StDateCategory* StDTGory;
};

__interface ISeacher
{
	bool cdfc_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
		UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk);

	StDateCategory* cdfc_search_start(HANDLE handle, int type, int* error);

	StDateCategory* cdfc_search_start_f(HANDLE handle, int type, int* error);

	UINT64 cdfc_current_sector();

	StDateCategory* cdfc_filelist();

	void cdfc_exit(StDateCategory* cate);

	void cdfc_stop();

	void cdfc_date_converter(UINT32 date, UINT8* resDate);

	//extern "C" _declspec(dllexport) void cdfc_freetask(IntPtr stFile);

	void cdfc_set_regionsize(UINT64 regionSize);

	StDateCategory* cdfc_filelist_system();

	void cdfc_set_preview(UINT64 nSize);

	bool cdfc_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

	bool cdfc_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

	bool cdfc_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

	bool cdfc_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);
};

//�趨�Ա�ʱ��;
extern "C" _declspec(dllexport) void set_com_tm(tm comTm);

//��ô󻪵�ʱ��;
extern "C" _declspec(dllexport)  tm get_dh_time(UINT32 dateNum);

//���Unixʱ��;��1970/01/01 00:00:00��ʼ;
extern "C" _declspec(dllexport)  tm get_unix_time(UINT32 dateNum);

//�趨��̬ȡʱί��;
extern "C" _declspec(dllexport) void set_com_func(tm (*getTime)(UINT32));
//����Ƿ�Ϊͬһ��;
extern "C" _declspec(dllexport) bool is_cate_same_day(StDateCategory* cate);

extern "C" _declspec(dllexport) StDateCategory* get_next_category(StDateCategory * cate);
extern "C" _declspec(dllexport) StVideo* get_next_video(StVideo * cate);
extern "C" _declspec(dllexport) eSearchType convert_type_to_searchtype(int type);

//�ӱ�׼ʱ��ת��ΪUINT8����;
extern "C" _declspec(dllexport) void convert_tm_to_uint(tm tm, UINT8* resDate);
extern "C" _declspec(dllexport) UINT64 get_file_count(StDateCategory *cate);

//��ͨDavSearcher;
class NormalDavSearcher : public ISeacher {
protected:
	//HANDLE hFile;							//��ª��WinApi;
	
	UINT64 startSec;					//��ʼ������;
	UINT64 endSec;						//��ֹ������;
	UINT64 timePos;						//ʱ��ƫ��;
	UINT64 lbaPos;						//ƫ��lba;
	int secSize;						//������С;

	INT64 pos;						//��ǰɨ��ƫ��;
	bool stopped;					//�Ƿ�ֹͣ��;
	UINT64 previewSize = 0;			//Ԥ����С;
	int count2 = 0;
 	tm(*get_time)(UINT32) = get_unix_time;
	UINT64 _nFileCount = 0;

	void cdfc_clean() {
		pos = 0;
		previewSize = 0;
		stopped = false;
	}

	void dispose_list(StDateCategory *stFile) {
		//�ͷ��ļ��б�;
		auto cateNode = stFile;
		while (cateNode != nullptr) {
			auto vNode = cateNode->File;
			while (vNode != nullptr) {
				auto fragNode = vNode->stStAdd;
				while (fragNode != nullptr) {
					auto temNode = fragNode;
					fragNode = fragNode->Next;
					delete temNode;
				}
				auto temNode = vNode;
				vNode = vNode->Next;
				delete temNode;
			}
			auto temNode = cateNode;
			cateNode = cateNode->Next;
			delete temNode;
		}

	}
public:
	bool cdfc_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
		UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk) {
		if (hDisk == nullptr) {
			return false;
		}
		cdfc_clean();
		startSec = nStartSec;
		endSec = nEndSec;
		timePos = nTimePos;
		lbaPos = nLBAPos;
		secSize = nSecSize;
		_nFileCount = 0;
		pos = nStartSec * secSize;

		if (secSize <= 0 || secSize % 512 != 0) {
			return false;
		}
		
		return true;
	}

	UINT64 cdfc_current_sector() {
		if (secSize == 0) {
			printf_s("secSize can't be zero.");
			return 0;
		}

		return pos / secSize;
	}

	StDateCategory* cdfc_filelist() {
		return nullptr;
	}

	void cdfc_stop() {
		stopped = true;
	}

	virtual StDateCategory* cdfc_search_start(HANDLE handle, int type, int* error) = 0;

	void get_data(UINT64 *nOffsetSec, UINT64 *nFileCount) {
		if (nOffsetSec == nullptr) {
			printf_s("nOffsetSec can't be null.");
			return;
		}
		if (nFileCount == nullptr) {
			printf_s("nFileCount can't be null.");
			return;
		}

		*nOffsetSec = cdfc_current_sector();
		*nFileCount = _nFileCount;
		


		/*if (StDCHeader != nullptr) {
			auto dateNode = StDCHeader;
			
			while (dateNode != nullptr) {
				auto fileNode = dateNode->File;
				while (fileNode != nullptr) {
					*nFileCount = *nFileCount + 1;
					fileNode = fileNode->Next;
				}
				dateNode = dateNode->Next;
			}
		}*/
	}

	void cdfc_set_preview(UINT64 nSize) {
		previewSize = nSize;
	}

	bool cdfc_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
		auto bufferSize = 1024 * 1024 * 16;
		auto buffer = (byte *)Mmalloc(bufferSize);
		auto frag = szFile->stStAdd;
		*nCurrSizeDW = 0;

		while (frag != nullptr) {
			MSetFilePointer(hDisk, frag->StartAddress, FILE_BEGIN);

			UINT64 readIndex = 0;
			while (readIndex < frag->Size) {
				auto thisReadSize = (frag->Size - readIndex) > bufferSize ? bufferSize : frag->Size % bufferSize;
				if (previewSize != 0) {
					thisReadSize = min(previewSize - *nCurrSizeDW, thisReadSize);
				}

				MReadFileInSector(hDisk, buffer, thisReadSize, secSize, frag->StartAddress + readIndex);
				MWriteFile(saveFileHandle, buffer, thisReadSize);

				readIndex += thisReadSize;
				*nCurrSizeDW += thisReadSize;
				if (*nCurrSizeDW >= previewSize && previewSize != 0) {
					break;
				}
			}

			if (*nCurrSizeDW >= previewSize && previewSize != 0) {
				break;
			}
			frag = frag->Next;
		}
		delete buffer;
		return true;
	}

	bool cdfc_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
		auto frag = szFile->stStAdd;

		UINT64 recSize = 0;

		while (frag != nullptr) {
			MSetFilePointer(hDisk, frag->StartAddress, FILE_BEGIN);

			auto thisReadSize = min(frag->Size, nBuffSize - recSize);

			MReadFileInSector(hDisk, szBuffer + recSize, thisReadSize, secSize, frag->StartAddress);
			frag = frag->Next;
			recSize += thisReadSize;

			if (recSize >= nBuffSize) {
				break;
			}
		}

		return true;
	}

	StDateCategory* cdfc_search_start_f(HANDLE handle, int type, int* error) {
		return nullptr;
	}

	void cdfc_set_regionsize(UINT64 regionSize) {

	}

	StDateCategory* cdfc_filelist_system() {
		return nullptr;
	}

	bool cdfc_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
		return false;
	}

	//wfs�˳���
	void cdfc_exit(StDateCategory* cate) {
		dispose_list(cate);
		cdfc_clean();
	}



	bool cdfc_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
		return false;
	}

	virtual tm cdfc_convert_int_to_tm(UINT32 date) {
		return get_time(date);
	}

	void cdfc_date_converter(UINT32 date, UINT8* resDate) {
		auto tm = cdfc_convert_int_to_tm(date);

		resDate[0] = tm.tm_year - 2000;
		resDate[1] = tm.tm_mon;
		resDate[2] = tm.tm_mday;
		resDate[3] = tm.tm_hour;
		resDate[4] = tm.tm_min;
		resDate[5] = tm.tm_sec;
	}

	
};


