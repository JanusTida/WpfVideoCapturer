#ifndef OPERATE_NTFS_HEAD_FILE
#define OPERATE_NTFS_HEAD_FILE

#pragma once

/*
    _Device_List			*GlobPointer;
    _Partiton_List		    *GlobPointerPartiton;
    ��Initialize.dll������ȫ�ֱ��� 
*/

struct Partiton_Struct
{
	int							m_LoGo;					//Ϊ�Ǹ������豸�ķ���
	char						VolumeName[0x20];		//�������
	char						FileSystem[0x20];		//�ļ�ϵͳ
	unsigned char				*m_Name;				//��������
	unsigned __int64			m_Size;					//������С
	unsigned int				m_Type;					//��������
	unsigned __int64			m_Offset;				//MBR��ƫ��
	bool						m_Boot;					//�Ƿ�����
	void						*m_pDev;				//ָ���豸
	char						m_Sign;					//�����̷�
	void						*pDBR;					//Boot�������ļ�ϵͳ��һ������
};

struct arch_fnct_struct
{
	void	*Partiton;
};
struct	Differ_System_RW
{	
	//���豸д����
	int (*Pwrite)(void *pDev, const void *buf, const unsigned int count, const unsigned __int64 offset);
	//���豸����
	int (*Pread)(void *pDev,  const void *buf, const unsigned int count, const unsigned __int64 offset);
	//�ر��豸
	int (*Pclose)(void *pDev); 
	//ˢ���ļ�����������д������
	int	(*Psync)(void *pDev);						
};

struct CHS_struct
{
	unsigned __int64 			m_Cylinder;					//������
	unsigned int				m_Head_Track;				//ÿ����ŵ���
	unsigned int				m_Track_Sector;				//ÿ�ŵ�������	
};

struct _RW_Buffer
{
	unsigned char				*m_Buffer;					//���ݵ�ַ
	unsigned int				m_DataSize;					//���ݴ�С
	unsigned int				m_Read;						//��ȡ��С
	BOOLEAN						m_Flags;					//�����ź�
};

struct Physics_Device_Struct 
{
	int							m_LoGo;						//�豸����(���Ϊ16�����������ƴ�)
	char Lable[64];
	unsigned char				m_DevName[20];				//��������
	unsigned __int64			m_DevSize;					//�豸��С
	CHS_struct					m_DevCHS;					//�豸����
	unsigned long				m_DevMomd;					//����ģʽ
	UINT						m_DevType;					//�豸����
#if !defined (__APPLE__)
	HANDLE						m_Handle;					//�豸���
#else
	int							m_FIleHandle;				//LINUX,MAC�µľ��
#endif
	long						m_SectorSize;				//�����ֽ�
	_RW_Buffer					*m_Buffer;					//��д����
	Partiton_Struct				*Partiton;					//�����ṹ
	const arch_fnct_struct		*m_Arch;					//����ָ��
	const Differ_System_RW		*m_DevRW;					//�豸��д
	bool						m_DevState;					//�Ƿ�ʹ��
};

//�豸����

struct _Device_List
{
	Physics_Device_Struct		*m_ThisDevice;				//��ǰ�豸
	_Device_List				*m_prev;					//��һ����
	_Device_List				*m_next;					//��һ����
};

struct	_Partiton_List
{
	Partiton_Struct				*m_ThisPartiton;		//��ǰ����
	_Partiton_List				*m_prev;
	_Partiton_List				*m_next;
};

typedef	enum File_System_Type
{
	U_nknown,	//δ֪����
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
i����ֵ0
���ܣ�
�豸��ʼ������������豸ѡ��ǰ���ȵ��ô˽ӿڵõ����з�����Ȼ����ʾ�ڽ�����
����
�������������
extern __declspec( dllimport ) _Device_List	*GlobPointer;
huitian_devinit( 0 );
_Device_List *m_pDevicelist = GlobPointer;
���з����ṹ�嶼��m_pDevicelist���ͨ��forѭ��������Ľṹ��ȡ����
*/
bool huitian_devinit( int i );

/*
����˳����߻ص������ҳʱ������ô˽ӿڣ��ͷŷ����ڴ�
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