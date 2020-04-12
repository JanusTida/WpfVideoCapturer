#ifndef __VIDEO_RECOVER_H__
#define __VIDEO_RECOVER_H__

#define Tranverse16( X ) ( ( ( ( int16_t )( X ) & 0xff00 ) >> 8 ) | ( ( ( int16_t )( X ) & 0x00ff ) << 8 ) )
#define Tranverse32( X ) ( ( ( ( UINT32)( X ) & 0xff000000 ) >> 24 ) | ( ( ( UINT32 )( X ) & 0x00ff0000 ) >> 8) | ( ( ( UINT32 )( X ) & 0x0000ff00 ) << 8 ) | ( ( ( UINT32 )( X ) & 0x000000ff ) << 24 ) )

enum eSearchType
{
	eSearchType_FULL,
	eSearchType_FREE,
	eSearchType_FS,
	eSearchType_QUICK
};

//监控品牌类型
enum eDeviceType
{
	eDeviceType_ERROR,		//未知设备
	eDeviceType_WFS04,		//wfs 0.4
	eDeviceType_DHFS41,		//DHFS 4.1
	eDeviceType_DHFS2,		//DHFS II
	eDeviceType_HIKVISION,	//海康威视
	eDeviceType_CANON5D,	//Canon EOS 5D Mark III	
	eDeviceType_CANON60D,	//Canon EOS 60D
	eDeviceType_SONYMTS,	//Sony MTS
	eDeviceType_HBMS,		//汉邦 HBMS
	eDeviceType_JVBK		//中维jvbk
};

#pragma pack(1)
typedef struct tagMdat
{
	UINT8 nUnknown[36];
	UINT32 nMdatSize;
}StMdat;
#pragma pack()


#pragma pack(1)
typedef struct tagFtyp
{
	StMdat *stMdat;
	unsigned __int64 nStartAddress;
	int nFlag;
	tagFtyp *next;
}StFtypList;
#pragma pack()

#pragma pack(1)
typedef struct tagMoov
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
}StMoov;
#pragma pack()

#pragma pack(1)
typedef struct tagMoovList
{
	StMoov *stMoov;
	unsigned __int64 nStartAddress;
	int nFlag;
	tagMoovList *next;
}StMoovList;
#pragma pack()

#pragma pack(1)
typedef struct tagStartAdd
{
	unsigned __int64 nStartAddress;
	unsigned __int64 nStartAddress1;
	unsigned __int64 nStartAddress2;
	unsigned __int64 nSize;
	UINT32 nChannelNO;
	UINT32 nStartDate;					//文件开始时间
	UINT32 nEndDate;					//文件结束时间
	tagStartAdd *next;
}StStartAdd;
#pragma pack()

//文件链表
#pragma pack(1)
typedef struct tagStImdh
{
	UINT32 nFrameNO;					//帧号
	UINT32 nChannelNO;					//通道号
	UINT32 nStartDate;					//文件开始时间
	UINT32 nEndDate;					//文件结束时间
	unsigned __int64 nSize;				//文件大小
	unsigned __int64 nSizeTrue;
	unsigned __int64 nStartAddress;		//文件起始地址
	StStartAdd *stStAdd;				//已用空间扫描时用到的碎片链表(全盘扫描不用)
	StFtypList *FtypList;
	StMoovList *MoovList;
	tagStImdh *next;
}StImdh;
#pragma pack()

//
#pragma pack(1)
typedef struct tagDate
{
	UINT32 nDate;		//文件的起始时间
	StImdh *File;		//文件链表
	StFtypList *FtypList;
	StMoovList *MoovList;
	tagDate *next;
}StDate;
#pragma pack()

/*
功能：申请内存
参数：size  需要申请的大小
返回值：返回内存起始地址
*/
void *videorecover_malloc( size_t size );

/*
功能：申请释放
参数：szPoint  需要释放的内存指针
*/
void videorecover_free( void *szPoint );

/*
功能：读磁盘/分区/文件二进制数据
参数：hDisk		  需要操作的设备句柄
	  nPosition   从设备第几个偏移地址开始读(相对于0)
	  szBuffer	  保存实际数据的buffer
	  nSize		  需要读取的大小,不能超过szBuffer长度
	  nRetSize	  返回实际读到的字节数
*/
bool videorecover_read( HANDLE hDisk, unsigned __int64 nPosition, char *szBuffer, unsigned __int64 nSize, DWORD *nRetSize );

/*
功能：写磁盘/分区/文件二进制数据
参数：hFile		  需要操作的设备句柄
	  nPosition   从设备第几个偏移地址开始写(相对于0)
	  szBuffer	  实际要写入数据的buffer
	  nSize		  需要的数据大小,不能超过szBuffer长度
	  nRetSize	  返回实际写入的字节数
*/
bool videorecover_write( HANDLE hFile, unsigned __int64 nPosition, char *szBuffer, unsigned __int64 nSize, DWORD *nRetSize );

/*
功能：写磁盘/分区/文件二进制数据
参数：hDisk		  需要操作的设备句柄
返回值：返回设备类型,查看结构体
*/
eDeviceType videorecover_fstype( HANDLE hDisk );

/*
功能：开始扫描
参数：hDisk        扫描的设备句柄
	  eType        扫描方法(全盘、文件系统、剩余空间、跳跃)
	  nStartSec    设备起始扇区
	  nEndSec      设备结束扇区
	  nSecSize     扇区大小
	  nAreaSize    用于跳跃扫描设置的大小
	  nClusterSize 簇大小
	  nLBAPos	   保留
	  bJournal	   是否保存日志
	  nError	   错误信息返回
返回值：文件链表，参考详细结构体链表
*/
StDate *dahua_searchstart( HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec, 
							 int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError );
StDate *zhongwei_searchstart( HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec, 
							 int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError );

/*
功能：恢复文件
参数：hDisk        扫描的设备句柄
	  eType        扫描方法(全盘、文件系统、剩余空间、跳跃)
	  stFile       要恢复的源文件
	  hFileOut     要保存的目标文件句柄
	  nCurrSizeDW  当前已经保存的大小
	  nError	   错误信息返回
返回值：true	   正确
		false      详细可查看nError
*/
bool dahua_recover( HANDLE hDisk, eSearchType eType, StImdh *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError );
bool zhongwei_recover( HANDLE hDisk, eSearchType eType, StImdh *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError );

/*
功能：退出当前模块释放内存
参数：stFile       要释放的文件头链表
*/
void dahua_exit( void *stFile );
void zhongwei_exit( void *stFile );

/*
功能：停止扫描
*/
void dahua_stop();
void zhongwei_stop();

/*
功能：时时返回当前扫描扇区偏移和文件数量
参数：nOffsetSec  当前扫描到的扇区地址
	  nFileCount  当前已扫描到的文件总数
*/
void dahua_get_date( unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount );
void zhongwei_get_date( unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount );

/*
功能：返回预览数据到szBuffer
参数：szFile      要预览的文件
	  eType       扫描方法(全盘、文件系统、剩余空间、跳跃)
	  hDisk		  预览的设备句柄
	  szBuffer	  返回预览的数据(需要外部申请好内存传指针进来)
	  nBuffSize	  szBuffer大小
*/
bool dahua_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );
bool zhongwei_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );

/*
功能：十进制时间转成 年-月-日 时:分:秒
参数：nDate	    十进制时间
	  date		date[0]-date[1]-date[2] date[3]:date[4]:date[5](传6字节数组)
*/
void dahua_date_converter( UINT32 nDate, int8_t *date );
void zhongwei_date_converter( UINT32 nDate, int8_t *date );

unsigned __int64 videorecover_filesize( HANDLE hFile );

bool zhongwei_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );
StImdh *zhongwei_create_file( StFtypList *stFtyp, StMoovList *stMoov );

#endif