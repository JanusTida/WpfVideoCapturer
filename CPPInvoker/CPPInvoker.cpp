// CPPInvoker.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "../cdfcproject2/DH_NVR_Recovery.h"
#include "../cdfcproject2/YinTanRecovery.h"
#include "../cdfcproject2/RSFSRecovery.h"
#include "../cdfcproject2/Common.h"
#include "../cdfcproject2/IO.h"
#include "../cdfcproject2/DHRecovery.h"
#include "../CPPInvoker/Contracts.h"
#include "../CPPInvoker/Demo.h"
#include "../cdfcproject2/HBRecovery.h"

#include "DHRecovery.h"
#include <vector>
#include <functional>
#include <time.h>
#include <Windows.h>
#include <atltime.h>

using namespace std;
const UINT32 DHHexNum[]{
	67108864,4194304,131072,4096,64,1
};

#pragma pack(1)
typedef struct Physics_Device_Struct {
	int							m_LoGo;						//设备标数(如果为16不以物理名称打开)
	char Lable[64];
	unsigned char				m_DevName[20];				//驱动名称
	unsigned __int64			m_DevSize;					//设备大小
	int							m_DevCHS[4];					//设备几何
	unsigned int				m_DevMomd;					//访问模式
	UINT						m_DevType;					//设备类型
	HANDLE						m_Handle;					//设备句柄
	int						m_SectorSize;				//扇区字节
	int					*m_Buffer;					//读写缓存
	int				*Partiton;					//分区结构
	int		*m_Arch;					//调用指针
	int		*m_DevRW;					//设备读写
	bool		m_DevState;					//是否使用;
};
//struct _Device_List {
//	
//};
typedef StDateCategory* (*search)(HANDLE hDisk, int eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError);

typedef struct _Device_List
{
	Physics_Device_Struct * m_ThisDevice; //当前设备
	_Device_List * m_prev;				 //上一链表
	_Device_List * m_next;				 //下一链表
}DeviceList;

const UINT16 arr[] = {
	0x004a,0x003a,0x002f,0x002f,0x76d1,0x63a7,0x0020,0x7248,0x672c,0x002f,0x0064,0x0068,0x0074,0x0065,0x0073,0x0074,0x002e,0x0069,0x006d,0x0067
};
static const unsigned short tab_GBK_to_UCS2[][2] =
{
	/* GBK    Unicode     字 */
	{ 0x8140, 0x4E02 }, // 丂
	{ 0x8141, 0x4E04 }, // 丄
	{ 0x8142, 0x4E05 }, // 丅
	{ 0x8143, 0x4E06 }, // 丆
	{ 0x8144, 0x4E0F }, // 丏
	{0x817F, 0x0001}// XXXXX
};


__interface IHuMan {
	int GetAge();
};



int main(int count, char* args)
{	
	//auto handle = MOpenFile(_T("D://mp4info.exe"), GENERIC_READ);

	//auto stream = new ImgRawStream(MOpenFile())
	//Mmemset(&tm, 0, sizeof(tm));
	auto hFile = MOpenFile(_T("I:\\安联锐士\\rsfs.dd"), GENERIC_READ);
	//auto hFile = MOpenFile(_T("J://anli_test/汉邦/HBMS2.img"), GENERIC_READ);
	//auto hFile = MOpenFile(_T("J://anli_test/安联锐视/安联锐视-rsfs.dd"), GENERIC_READ);
	//auto hFile = MOpenFile(_T("E:/安联锐视-rsfs.dd"), GENERIC_READ);
	printf("\n");
	for (size_t i = 0; i < 3; i++)
	{
		DWORD szHigh = 0;
		auto sz = new LARGE_INTEGER();
		GetFileSizeEx(hFile, sz);
		int error = 0;
		auto cate = rsfs_searchstart(
			hFile,
			eSearchType_FULL,
			0,
			sz->QuadPart / 512,
			512,
			0L,
			0,
			0,
			false,
			&error);

		UINT64 cur = 0;
		int err = 0;
		/*if (cate != nullptr) {
		dahua_recover(hFile, eSearchType_FULL, cate->File, hFile, &cur, &err);
		}*/
		ULONG64 count2 = 0;
		ULONG64 pos = 0;
		hb_get_date(&pos, &count2);
		
		/*auto cn = new StDateCategory();
		hb_exit(cn);
		hb_get_date(&pos, &count2);*/
		
		
		auto fCount = 0;
		auto cate2 = cate;
		while (cate2 != nullptr) {
			auto fileNode = cate2->File;
			while (fileNode != nullptr) {
				printf("文件大小:%d\n", fileNode->Size);
				fCount++;
				fileNode = fileNode->Next;
			}
			cate2 = cate2->Next;
		}
		//hb_exit(cate);
		printf("文件总数:%d\n", count2);
		
 	}

	//rsfs_exit(cate);
	
	printf("完成");
	scanf_s("dasd");
	MCloseHandle(hFile);
	//MCreateFileFromUnicode
	//auto hFile = MOpenFile(_T("I:/Dahua2018-01-08/2018-01-08-09-14-06.dav"), GENERIC_READ);
	//cdfc_dh_nvr_init(0, GetFileSize(hFile, nullptr) / 512, 512, 0, 0, hFile);
	////auto s = cdfc_dh_nvr_search_start(hFile, 0, nullptr);
	//
	//Sleep(1000);
	//cdfc_dh_nvr_exit();
	

	/*auto mFile = MCreateFile(_T("I://1.ddav"),CREATE_ALWAYS);
	auto hFile = MOpenFile(_T("I://yt1.DD"), GENERIC_READ);
	
	cdfc_dahua_init(0, GetFileSize(hFile, nullptr) / 512, 512, 0, 0, hFile);
	auto s = cdfc_dahua_search_start(hFile, 0, nullptr);
	
	char path[256] = { 0 };

	UINT64 cur = 0;
	//cdfc_tdwy_filesave(s->File, hFile, mFile, &cur, nullptr);
	//cdfc_tdwy_exit();
	*/
	return 0;
}




//
//template <class T2>
//function<(int)(T2)> Func;
//int *binaryFindIndex(vector<T2> vec,Func func) {
//	
//	return nullptr;
//}
//
//bool cdfc_wfs_init2(UINT3264 nStartSec) {
//	return false;
//}


