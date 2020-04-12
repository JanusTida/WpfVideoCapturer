
/**
 * Ä£¿é :
 */

#ifndef __DISK32_H__
#define __DISK32_H__


#pragma pack(1)
typedef struct tagHDDInfo2
{
	int id;
	char szModelNumber[512];
	char szSerialNumber[512];
	char szControllerNumber[512];
	tagHDDInfo2 *Next;
}stHDDInfo2;
#pragma pack()

#pragma pack(1)
typedef struct tagHDDInfo
{
	int id;
	char VendorID[512];				//
	char ProductID[512];
	char ProductRevision[512];
	char SerialNumber[512];
	char Lable;
	stHDDInfo2 *info;
	tagHDDInfo *Next;
}stHDDInfo;
#pragma pack()

void *get_hdd_vender();

void exit_hdd_vender();

DWORD GetMACaddress();
#endif /*__DISK32_H__*/