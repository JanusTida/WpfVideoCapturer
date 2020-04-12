
/**
 * ģ�� :
 */

#ifndef __DH_RECOVER_H__
#define __DH_RECOVER_H__

/*
������治��Ҫ�õ�
*/
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

/*
StDate����������Ǽ�¼���ڣ�����2015-10-01
StImdh����������Ǽ�¼StDate��һ���ж��ٸ�¼��03:05:46---06:03:33һ��ʱ���һ���ļ�
*/
//�ļ�����,��¼�ļ��Ļ�����Ϣ,ͨ��next�ҵ���һ���ļ�
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
	tagStImdh *next;					//��һ���ļ�
}StImdh;
#pragma pack()


/*
����ṹ����������������ɨ���ļ�������
*/
#pragma pack(1)
typedef struct tagDate
{
	UINT32 nDate;	//���� ��-��-��
	StImdh *File;	//��������µ��ļ� ʱ-��-����ļ�
	tagDate *next;	//��һ����������
}StDate;
#pragma pack()

/*
���ܣ���ʼ��ɨ������
������nStartSec���豸��ʼ����
	  nEndSec�����ý�������(�豸�Ĵ�С)
	  nSecSize������ÿ�����ֽ���(Ĭ��512�ֽ�)
	  nTimePos��ʱ��ƫ��
	  nLBAPos��LBAƫ��
	  hDisk���豸�ľ��
*/
bool cdfc_dahua_init( unsigned __int64 nStartSec, unsigned __int64 nEndSec, int nSecSize, unsigned __int64 nTimePos,
					   unsigned __int64 nLBAPos, HANDLE hDisk );
unsigned __int64 cdfc_dahua_current_sector();
/*
���ܣ���ȡ�����ļ���С(�ֽ�)
������hFile������dh_get_img_handle���ص��ļ����
����ֵ�������ļ����ֽ���
*/
unsigned __int64 cdfc_dahua_imagefile_size( HANDLE hFile );

/*
���ܣ���ȡ�����ļ��Ĳ������
������szImgFile�������ļ��ľ���·��
����ֵ�������ļ����
*/
HANDLE cdfc_dahua_imagefile_handle( char *szImgFile );

/*
���ܣ��󻪼��ȫ��ɨ�蹦�ܽӿ�
������hDisk�������豸�ľ��
	  nCountFile��ʵ��ɨ�赽���ļ���(��ʱ����,�����ǿռ���)
	  nType���󻪼�ذ汾 ��1����2
����ֵ��StDate�ṹ���������һ���ļ�����
*/
StDate *cdfc_dahua_search_start( HANDLE hDisk, int nType );

/*
���ܣ��󻪼���ļ�ϵͳɨ�蹦�ܽӿ�
������hDisk�������豸�ľ��
	  nCountFile��ʵ��ɨ�赽���ļ���(��ʱ����,�����ǿռ���)
	  nType���󻪼�ذ汾 ��1����2
����ֵ��StDate�ṹ���������һ���ļ�����
*/
StDate *cdfc_dahua_search_start_f( HANDLE hDisk, int nType );

/*
���ܣ�����ر�ʱ����,�ͷ���������ռ�����ڴ�
������
*/
void cdfc_dahua_exit();

/*
���ܣ��ͷ�ָ���������ڴ�,�ر�ĳ������ʱ�ͷ�
������stFile����������ṹ��StDate
*/
void cdfc_dahua_freetask( StDate *stFile );

/*
���ܣ��ָ���ʹ�ÿռ��������ļ�������
������szFile��������Ҫ�ָ����ļ�StImdh�ṹ��
	  hDisk����ǰ����ռ�����豸���
	  szFile_2���ⲿ���ɵ��ļ����,�ӿ����ݻ�������д
	  nCurrSizeDW��ʱʱ�����Ѿ�����Ĵ�С,�������ڼ���ָ��ٶ�
*/
bool cdfc_dahua_filesave_f( StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError );

/*
���ܣ��ָ�ȫ�̿ռ��������ļ�������
������szFile��������Ҫ�ָ����ļ�StImdh�ṹ��
	  hDisk����ǰ����ռ�����豸���
	  szFile_2���ⲿ���ɵ��ļ����,�ӿ����ݻ�������д
	  nCurrSizeDW��ʱʱ�����Ѿ�����Ĵ�С,�������ڼ���ָ��ٶ�
*/
bool cdfc_dahua_filesave( StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError );

/*
���ܣ�ֹͣ��ǰɨ������
������
*/
void cdfc_dahua_stop();

/*
���ܣ�ʱʱ�����Ѿ��������ļ��б�,�����Ͽ���1�����һ�λ�ȡ�ļ��б���ʾStDate����
����ֵ��StDate�ṹ���������һ���ļ�����
*/
StDate *cdfc_dahua_filelist();

/*
���ܣ�ʱ��ת��
������nDate�������32λ����ʱ����
	  date�������-��-�� ʱ-��-��
ע��char ���� uint_8
*/
void cdfc_dahua_date_converter( UINT32 nDate, char *date );

/*
���ܣ���ȡ�豸����
������hDisk���������豸
	  nPos������λ��
	  szBuffer����ȡ���ݴ�ŵ�buffer
	  nSize����ȡ�Ĵ�С(�ֽ�)
	  nSize������ʵ�ʶ������ֽ���
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