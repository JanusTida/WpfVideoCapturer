
/**
 * 模块 :
 */

#ifndef __DH_RECOVER_H__
#define __DH_RECOVER_H__

/*
这个界面不需要用到
*/
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

/*
StDate：这个链表是记录日期，比如2015-10-01
StImdh：这个链表是记录StDate这一天有多少个录像03:05:46---06:03:33一个时间段一个文件
*/
//文件链表,记录文件的基本信息,通过next找到下一个文件
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
	tagStImdh *next;					//下一个文件
}StImdh;
#pragma pack()


/*
这个结构体链表是所有整个扫描文件的链表
*/
#pragma pack(1)
typedef struct tagDate
{
	UINT32 nDate;	//日期 年-月-日
	StImdh *File;	//这个日期下的文件 时-分-秒的文件
	tagDate *next;	//下一个日期链表
}StDate;
#pragma pack()

/*
功能：初始化扫描区域
参数：nStartSec，设备起始扇区
	  nEndSec，设置结束扇区(设备的大小)
	  nSecSize，设置每扇区字节数(默认512字节)
	  nTimePos，时间偏移
	  nLBAPos，LBA偏移
	  hDisk，设备的句柄
*/
bool cdfc_dahua_init( unsigned __int64 nStartSec, unsigned __int64 nEndSec, int nSecSize, unsigned __int64 nTimePos,
					   unsigned __int64 nLBAPos, HANDLE hDisk );
unsigned __int64 cdfc_dahua_current_sector();
/*
功能：获取镜像文件大小(字节)
参数：hFile，调用dh_get_img_handle返回的文件句柄
返回值：镜像文件总字节数
*/
unsigned __int64 cdfc_dahua_imagefile_size( HANDLE hFile );

/*
功能：获取镜像文件的操作句柄
参数：szImgFile，镜像文件的绝对路径
返回值：镜像文件句柄
*/
HANDLE cdfc_dahua_imagefile_handle( char *szImgFile );

/*
功能：大华监控全盘扫描功能接口
参数：hDisk，对象设备的句柄
	  nCountFile，实际扫描到的文件数(暂时无用,传个非空即可)
	  nType：大华监控版本 大华1，大华2
返回值：StDate结构体链表，这个一个文件链表
*/
StDate *cdfc_dahua_search_start( HANDLE hDisk, int nType );

/*
功能：大华监控文件系统扫描功能接口
参数：hDisk，对象设备的句柄
	  nCountFile，实际扫描到的文件数(暂时无用,传个非空即可)
	  nType：大华监控版本 大华1，大华2
返回值：StDate结构体链表，这个一个文件链表
*/
StDate *cdfc_dahua_search_start_f( HANDLE hDisk, int nType );

/*
功能：软件关闭时调用,释放所有任务占用在内存
参数：
*/
void cdfc_dahua_exit();

/*
功能：释放指定的任务内存,关闭某个任务时释放
参数：stFile，传入任务结构体StDate
*/
void cdfc_dahua_freetask( StDate *stFile );

/*
功能：恢复已使用空间搜索的文件到本地
参数：szFile：传入需要恢复的文件StImdh结构体
	  hDisk：当前任务占用在设备句柄
	  szFile_2：外部生成的文件句柄,接口数据会往里面写
	  nCurrSizeDW：时时返回已经保存的大小,可以用于计算恢复速度
*/
bool cdfc_dahua_filesave_f( StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError );

/*
功能：恢复全盘空间搜索的文件到本地
参数：szFile：传入需要恢复的文件StImdh结构体
	  hDisk：当前任务占用在设备句柄
	  szFile_2：外部生成的文件句柄,接口数据会往里面写
	  nCurrSizeDW：时时返回已经保存的大小,可以用于计算恢复速度
*/
bool cdfc_dahua_filesave( StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError );

/*
功能：停止当前扫描任务
参数：
*/
void cdfc_dahua_stop();

/*
功能：时时返回已经搜索的文件列表,界面上可以1秒调用一次获取文件列表显示StDate链表
返回值：StDate结构体链表，这个一个文件链表
*/
StDate *cdfc_dahua_filelist();

/*
功能：时间转换
参数：nDate，输入的32位整形时间数
	  date，输出年-月-日 时-分-秒
注：char 就是 uint_8
*/
void cdfc_dahua_date_converter( UINT32 nDate, char *date );

/*
功能：读取设备数据
参数：hDisk，操作的设备
	  nPos，读的位置
	  szBuffer，读取数据存放的buffer
	  nSize，读取的大小(字节)
	  nSize，返回实际读到的字节数
*/
bool cdfc_dahua_read( HANDLE hDisk, unsigned __int64 nPos, char *szBuffer, unsigned __int64 nSize, DWORD *nDwSize );

/*
cdfc_wfs_init
cdfc_wfs_search_start
cdfc_wfs_search_start_f
cdfc_wfs_current_sector
cdfc_wfs_filelist
cdfc_wfs_filesave_f
cdfc_wfs_exit
cdfc_wfs_date_converter
cdfc_wfs_stop
cdfc_wfs_filesave
cdfc_wfs_freetask
cdfc_wfs_set_preview

cdfc_haikang_init
cdfc_haikang_search_start
cdfc_haikang_search_start_f
cdfc_haikang_current_sector
cdfc_haikang_filelist
cdfc_haikang_exit
cdfc_haikang_date_converter
cdfc_haikang_stop
cdfc_haikang_filesave
cdfc_haikang_freetask
cdfc_haikang_set_preview
*/
#endif /*__DH_RECOVER_H__*/