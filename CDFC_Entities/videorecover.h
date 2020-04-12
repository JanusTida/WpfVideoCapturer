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
	UINT32 nStartDate;					//�ļ���ʼʱ��
	UINT32 nEndDate;					//�ļ�����ʱ��
	tagStartAdd *next;
}StStartAdd;
#pragma pack()

//�ļ�����
#pragma pack(1)
typedef struct tagStImdh
{
	UINT32 nFrameNO;					//֡��
	UINT32 nChannelNO;					//ͨ����
	UINT32 nStartDate;					//�ļ���ʼʱ��
	UINT32 nEndDate;					//�ļ�����ʱ��
	unsigned __int64 nSize;				//�ļ���С
	unsigned __int64 nSizeTrue;
	unsigned __int64 nStartAddress;		//�ļ���ʼ��ַ
	StStartAdd *stStAdd;				//���ÿռ�ɨ��ʱ�õ�����Ƭ����(ȫ��ɨ�費��)
	StFtypList *FtypList;
	StMoovList *MoovList;
	tagStImdh *next;
}StImdh;
#pragma pack()

//
#pragma pack(1)
typedef struct tagDate
{
	UINT32 nDate;		//�ļ�����ʼʱ��
	StImdh *File;		//�ļ�����
	StFtypList *FtypList;
	StMoovList *MoovList;
	tagDate *next;
}StDate;
#pragma pack()

/*
���ܣ������ڴ�
������size  ��Ҫ����Ĵ�С
����ֵ�������ڴ���ʼ��ַ
*/
void *videorecover_malloc( size_t size );

/*
���ܣ������ͷ�
������szPoint  ��Ҫ�ͷŵ��ڴ�ָ��
*/
void videorecover_free( void *szPoint );

/*
���ܣ�������/����/�ļ�����������
������hDisk		  ��Ҫ�������豸���
	  nPosition   ���豸�ڼ���ƫ�Ƶ�ַ��ʼ��(�����0)
	  szBuffer	  ����ʵ�����ݵ�buffer
	  nSize		  ��Ҫ��ȡ�Ĵ�С,���ܳ���szBuffer����
	  nRetSize	  ����ʵ�ʶ������ֽ���
*/
bool videorecover_read( HANDLE hDisk, unsigned __int64 nPosition, char *szBuffer, unsigned __int64 nSize, DWORD *nRetSize );

/*
���ܣ�д����/����/�ļ�����������
������hFile		  ��Ҫ�������豸���
	  nPosition   ���豸�ڼ���ƫ�Ƶ�ַ��ʼд(�����0)
	  szBuffer	  ʵ��Ҫд�����ݵ�buffer
	  nSize		  ��Ҫ�����ݴ�С,���ܳ���szBuffer����
	  nRetSize	  ����ʵ��д����ֽ���
*/
bool videorecover_write( HANDLE hFile, unsigned __int64 nPosition, char *szBuffer, unsigned __int64 nSize, DWORD *nRetSize );

/*
���ܣ�д����/����/�ļ�����������
������hDisk		  ��Ҫ�������豸���
����ֵ�������豸����,�鿴�ṹ��
*/
eDeviceType videorecover_fstype( HANDLE hDisk );

/*
���ܣ���ʼɨ��
������hDisk        ɨ����豸���
	  eType        ɨ�跽��(ȫ�̡��ļ�ϵͳ��ʣ��ռ䡢��Ծ)
	  nStartSec    �豸��ʼ����
	  nEndSec      �豸��������
	  nSecSize     ������С
	  nAreaSize    ������Ծɨ�����õĴ�С
	  nClusterSize �ش�С
	  nLBAPos	   ����
	  bJournal	   �Ƿ񱣴���־
	  nError	   ������Ϣ����
����ֵ���ļ������ο���ϸ�ṹ������
*/
StDate *dahua_searchstart( HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec, 
							 int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError );
StDate *zhongwei_searchstart( HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec, 
							 int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError );

/*
���ܣ��ָ��ļ�
������hDisk        ɨ����豸���
	  eType        ɨ�跽��(ȫ�̡��ļ�ϵͳ��ʣ��ռ䡢��Ծ)
	  stFile       Ҫ�ָ���Դ�ļ�
	  hFileOut     Ҫ�����Ŀ���ļ����
	  nCurrSizeDW  ��ǰ�Ѿ�����Ĵ�С
	  nError	   ������Ϣ����
����ֵ��true	   ��ȷ
		false      ��ϸ�ɲ鿴nError
*/
bool dahua_recover( HANDLE hDisk, eSearchType eType, StImdh *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError );
bool zhongwei_recover( HANDLE hDisk, eSearchType eType, StImdh *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError );

/*
���ܣ��˳���ǰģ���ͷ��ڴ�
������stFile       Ҫ�ͷŵ��ļ�ͷ����
*/
void dahua_exit( void *stFile );
void zhongwei_exit( void *stFile );

/*
���ܣ�ֹͣɨ��
*/
void dahua_stop();
void zhongwei_stop();

/*
���ܣ�ʱʱ���ص�ǰɨ������ƫ�ƺ��ļ�����
������nOffsetSec  ��ǰɨ�赽��������ַ
	  nFileCount  ��ǰ��ɨ�赽���ļ�����
*/
void dahua_get_date( unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount );
void zhongwei_get_date( unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount );

/*
���ܣ�����Ԥ�����ݵ�szBuffer
������szFile      ҪԤ�����ļ�
	  eType       ɨ�跽��(ȫ�̡��ļ�ϵͳ��ʣ��ռ䡢��Ծ)
	  hDisk		  Ԥ�����豸���
	  szBuffer	  ����Ԥ��������(��Ҫ�ⲿ������ڴ洫ָ�����)
	  nBuffSize	  szBuffer��С
*/
bool dahua_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );
bool zhongwei_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );

/*
���ܣ�ʮ����ʱ��ת�� ��-��-�� ʱ:��:��
������nDate	    ʮ����ʱ��
	  date		date[0]-date[1]-date[2] date[3]:date[4]:date[5](��6�ֽ�����)
*/
void dahua_date_converter( UINT32 nDate, int8_t *date );
void zhongwei_date_converter( UINT32 nDate, int8_t *date );

unsigned __int64 videorecover_filesize( HANDLE hFile );

bool zhongwei_preview( StImdh *szFile, eSearchType eType, HANDLE hDisk, char *szBuffer, unsigned __int64 nBuffSize );
StImdh *zhongwei_create_file( StFtypList *stFtyp, StMoovList *stMoov );

#endif