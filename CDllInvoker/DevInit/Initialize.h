#ifndef OPERATE_NTFS_HEAD_FILE
#define OPERATE_NTFS_HEAD_FILE

#pragma once

/*
    _Device_List			*GlobPointer;
    _Partiton_List		    *GlobPointerPartiton;
    由Initialize.dll导出的全局变量 
*/

struct Partiton_Struct
{
	int							m_LoGo;					//为那个物理设备的分区
	char						VolumeName[0x20];		//卷标名称
	char						FileSystem[0x20];		//文件系统
	unsigned char				*m_Name;				//分区名称
	unsigned __int64			m_Size;					//分区大小
	unsigned int				m_Type;					//分区类型
	unsigned __int64			m_Offset;				//MBR的偏移
	bool						m_Boot;					//是否引导
	void						*m_pDev;				//指向设备
	char						m_Sign;					//分区盘符
	void						*pDBR;					//Boot扇区（文件系统第一扇区）
};

struct arch_fnct_struct
{
	void	*Partiton;
};
struct	Differ_System_RW
{	
	//向设备写数据
	int (*Pwrite)(void *pDev, const void *buf, const unsigned int count, const unsigned __int64 offset);
	//读设备数据
	int (*Pread)(void *pDev,  const void *buf, const unsigned int count, const unsigned __int64 offset);
	//关闭设备
	int (*Pclose)(void *pDev); 
	//刷新文件缓存区，并写入数据
	int	(*Psync)(void *pDev);						
};

struct CHS_struct
{
	unsigned __int64 			m_Cylinder;					//柱面数
	unsigned int				m_Head_Track;				//每柱面磁道数
	unsigned int				m_Track_Sector;				//每磁道扇区数	
};

struct _RW_Buffer
{
	unsigned char				*m_Buffer;					//数据地址
	unsigned int				m_DataSize;					//数据大小
	unsigned int				m_Read;						//读取大小
	BOOLEAN						m_Flags;					//读入信号
};

struct Physics_Device_Struct 
{
	int							m_LoGo;						//设备标数(如果为16不以物理名称打开)
	char Lable[64];
	unsigned char				m_DevName[20];				//驱动名称
	unsigned __int64			m_DevSize;					//设备大小
	CHS_struct					m_DevCHS;					//设备几何
	unsigned long				m_DevMomd;					//访问模式
	UINT						m_DevType;					//设备类型
#if !defined (__APPLE__)
	HANDLE						m_Handle;					//设备句柄
#else
	int							m_FIleHandle;				//LINUX,MAC下的句柄
#endif
	long						m_SectorSize;				//扇区字节
	_RW_Buffer					*m_Buffer;					//读写缓存
	Partiton_Struct				*Partiton;					//分区结构
	const arch_fnct_struct		*m_Arch;					//调用指针
	const Differ_System_RW		*m_DevRW;					//设备读写
	bool						m_DevState;					//是否使用
};

//设备链表

struct _Device_List
{
	Physics_Device_Struct		*m_ThisDevice;				//当前设备
	_Device_List				*m_prev;					//上一链表
	_Device_List				*m_next;					//下一链表
};

struct	_Partiton_List
{
	Partiton_Struct				*m_ThisPartiton;		//当前分区
	_Partiton_List				*m_prev;
	_Partiton_List				*m_next;
};

typedef	enum File_System_Type
{
	U_nknown,	//未知类型
	U_FAT12,
	U_FAT16,
	U_FAT32,
	U_NTFS,
	U_EXFAT,
	U_EXT2,
	U_EXT3,
	U_EXT4,
	U_HFS,
	U_HFSP,
	U_HFSX
};

/*
i：传值0
功能：
设备初始化，软件进入设备选择前，先调用此接口得到所有分区，然后显示在界面上
例：
先声明这个变量
extern __declspec( dllimport ) _Device_List	*GlobPointer;
huitian_devinit( 0 );
_Device_List *m_pDevicelist = GlobPointer;
所有分区结构体都在m_pDevicelist这里，通过for循环把里面的结构体取出来
*/
bool huitian_devinit( int i );

/*
软件退出或者回到软件首页时，请调用此接口，释放分区内存
*/
bool huitian_devexit( int i );

_Device_List *huitian_get_device();
_Partiton_List *huitian_get_partition();
void huitian_get_diskinfo( char *szDriver, char *szLable, unsigned __int64 *nFreeBytes );
char *init_query( char *szSN, char *EndPointURL );
char *init_query_vip2( char *szSN, char *EndPointURL );
void init_CoUninitialize();
void init_CoInitialize();
int init_query_sn_check( char *szSN, char *EndPointURL );
int init_query_sn_check_vip2( char *szSN, char *EndPointURL );
void *mov_load_history( HANDLE hHistoryFile, unsigned __int64 *nEndSec, int *nSearchMath, int *nVideoType );


bool init_bitmap_init( UINT32 nMapSize_M );
void init_bitmap_free();
void init_bitmap_set( unsigned __int64 nClusterNum );
int init_bitmap_get( unsigned __int64 nClusterNum );
#endif