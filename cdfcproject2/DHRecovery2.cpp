/**
* 模块
*
* @version 	%I%, %G%
*/


#include "DHRecovery2.h"
#include"stdint.h"
#include <time.h>
#include <atlstr.h>

//做为判断的临时帧容器
//#pragma pack(1)
typedef struct tagFrame
{
	UINT32 StartDate;
	UINT32 EndDate;
	UINT32 Size;
	UINT32 Channel;
	UINT32 FrameNOEnd;
	UINT32 FrameNOStart;
	unsigned __int64 nStartAddress;
}stFrame_dh;
//#pragma pack()

typedef struct tagFramelist
{
	UINT32 StartDate;
	UINT32 EndDate;
	UINT32 Size;
	UINT32 Channel;
	UINT32 FrameNOEnd;
	UINT32 FrameNOStart;
	unsigned __int64 nStartAddress;
	tagFramelist *next;
}stFramelist_dh;

static StDate *m_stFile = NULL;
static StDate *m_stFileTmp = NULL;
static StDate *m_stFilePre = NULL;
static StDate *m_stFile_2 = NULL;

static int m_nFS = 0;
static DWORD s_dwReadSize = 0;
static unsigned __int64 s_i = 0, s_j = 0, s_nImgSize = 0;
static DWORD s_dwPtr = 0;
static HANDLE s_f_IMG = NULL, m_hBitMap = NULL;
static char *s_szBuff = NULL;
static bool s_bStop = true, m_bFSearch = true;
static BOOL s_bRet = true;
static LARGE_INTEGER s_li;
static StImdh *s_stFile, *s_stTmpFile, *s_stFileNext, *s_stFileExit;
static unsigned __int64 size = 0, size_sec = 0, s_sec_size = 0, s_nStartSec = 0;
static char *m_Tmpbuff = NULL;
static unsigned __int64 m_nDWSize = 0;
static int m_nType = 0;
static bool bRecovery = true;
static unsigned __int64 m_nTimePos = 0;
static unsigned __int64 m_nLBAPos = 0;
static unsigned __int64 m_nPreViewSize = 0;
static unsigned __int64 m_nRegionsize = 0;
static bool m_bFS_Free = false;
static unsigned __int64 m_nCountFragment = 0, m_nI = 0;
static unsigned __int64 *m_szFragment = NULL;
static StDate *stFile_tmp = NULL;


//处理多任务链表
static int m_nListCount = 0;
static StDate *m_stFileList[64] = { NULL };
//static int8_t date[6];

static HANDLE __dh_openflie(WCHAR *szFile);
static unsigned __int64 __dh_get_file_size(HANDLE hFile);
static HANDLE __dh_create_file(char *szFile);
static bool __check(char *szFrame, dhFrameHeader *header, dhFrameTail *tail, unsigned __int64 j);
static void __get_data(UINT32 nDate, int8_t *date);
static void *__malloc(size_t size);
static void __set_file(StImdh *file, dhFrameHeader *header);
static unsigned __int64 __init_start_pos(int *n);
static void __tranverse(dhFrameHeader *header, dhFrameTail *tail);
static void __read_file(unsigned __int64 pos, char *buf, DWORD size);
static int __get_filelen(char *buff16, char *buff);
static unsigned __int64 __chartoint(char *szBuff, int nLen);
static void __en_char(char *buffer);
static void __de_char(char *buffer);
static StDate *cdfc_dahua_search_start_3(HANDLE hDisk, unsigned __int64 *nCountFile, int nType, int *nError);
static bool __b_dhfs(HANDLE hDisk);
static StDate *__recovery_searchfile_2(HANDLE hDisk, int nType);
static void __count_freespace(unsigned __int64 nStartAddress);
static bool __count_freespace_b(unsigned __int64 nStartAddress);

static char szBuff[512];

#define nBufSize 512
#define MFTPOS 95744

#pragma pack(1)
typedef struct tagStn
{
	int8_t Unknown0[72];
	int16_t n;
}Stn;
#pragma pack()

/////////////////////////////////////////////////////////////////////////////////////////////////

//nEndSec搜索的结束扇区，nStartSec搜索的开始扇区，nSecSize搜索的单位，默认为512
bool cdfc_dahua_init(unsigned __int64 nStartSec, unsigned __int64 nEndSec, int nSecSize, unsigned __int64 nTimePos,
	unsigned __int64 nLBAPos, HANDLE hDisk)
{
	if (!hDisk)
		return false;
	s_dwReadSize = 0;
	s_f_IMG = hDisk;
	s_bRet = true;
	s_bStop = true;
	s_stFile = NULL;
	s_stTmpFile = NULL;
	s_stFileNext = NULL;
	s_stFileExit = NULL;
	m_nFS = 0;
	m_nPreViewSize = 0;
	m_nRegionsize = 0;
	m_bFS_Free = false;
	m_nCountFragment = 0;
	m_bFSearch = true;

	m_stFile = m_stFileTmp = m_stFilePre = NULL;
	if (m_Tmpbuff)
		free(m_Tmpbuff);
	m_Tmpbuff = (char *)__malloc(512);
	s_nStartSec = nStartSec;
	size_sec = nEndSec;
	s_sec_size = nSecSize;
	s_i = nStartSec;
	m_nTimePos = nTimePos;
	m_nLBAPos = nLBAPos;
	//init_bitmap_init( 500 );
	int n = sizeof(unsigned __int64);
	if (m_szFragment == NULL)
		m_szFragment = (unsigned __int64 *)__malloc(150 * 1024 * 1024);

	return true;
}

unsigned __int64 cdfc_dahua_current_sector()
{
	return s_i;
}

void cdfc_dahua_stop()
{
	s_bStop = false;
}

unsigned __int64 cdfc_common_imagefile_size(HANDLE hFile)
{
	return __dh_get_file_size(hFile);
}

HANDLE cdfc_common_imagefile_handle(WCHAR *szImgFile)
{
	HANDLE szFImg = __dh_openflie(szImgFile);
	return szFImg;
}

StDate *cdfc_dahua_filelist()
{
	if (m_bFSearch)
		return stFile_tmp;
	return m_stFile;
}

//返回未分配空间链表
StDate *cdfc_dahua_filelist_2()
{
	return m_stFile_2;
}

unsigned __int64 cdfc_dahua_get_writesize()
{
	return m_nDWSize;
}

void cdfc_dahua_init_writesize()
{
	m_nDWSize = 0;
}

/*
0：error
1：wfs 0.4
2：DHFS 4.1
3：DHFS II
4：HIKVISION
5：Canon EOS 5D Mark III
6：Canon EOS 60D
*/
int cdfc_common_fstype(HANDLE hDisk)
{
	if (!hDisk)
		return 0;
	DWORD nDwSize;
	char buffer[512];
	memset(buffer, 0, 512);
	s_li.QuadPart = 0;
	SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	ReadFile(hDisk, buffer, 512, &nDwSize, NULL);
	if (buffer[0] == 0x57 && buffer[1] == 0x46 && buffer[2] == 0x53 && buffer[3] == 0x30 && buffer[4] == 0x2E && buffer[5] == 0x34)
		return 1;
	if (buffer[0] == 0x44 && buffer[1] == 0x48 && buffer[2] == 0x46 && buffer[3] == 0x53 && buffer[4] == 0x34 && buffer[5] == 0x2E && buffer[6] == 0x31)
		return 2;

	s_li.QuadPart = 17408;
	SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	ReadFile(hDisk, buffer, 512, &nDwSize, NULL);
	if (buffer[328] == 0xFFFFFFAA && buffer[329] == 0x55 && buffer[330] == 0xFFFFFFAA && buffer[331] == 0x55)
		return 3;
	s_li.QuadPart = 512;
	SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	ReadFile(hDisk, buffer, 512, &nDwSize, NULL);
	if (buffer[16] == 0x48 && buffer[17] == 0x49 && buffer[18] == 0x4B && buffer[19] == 0x56 &&
		buffer[20] == 0x49 && buffer[21] == 0x53 && buffer[22] == 0x49 && buffer[23] == 0x4F && buffer[24] == 0x4E)
		return 4;
	else
	{
		s_li.QuadPart = 80044032;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		ReadFile(hDisk, buffer, 512, &nDwSize, NULL);
		if (buffer[328] == 0xAA && buffer[329] == 0x55 && buffer[330] == 0xAA && buffer[331] == 0x55 && buffer[332] == 0xAA)
			return 4;
	}
	for (unsigned __int64 i = 0; i < 536870912; i += 512)
	{
		s_li.QuadPart = i;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		ReadFile(hDisk, buffer, 512, &nDwSize, NULL);
		if (buffer[246] == 0x43 && buffer[247] == 0x61 && buffer[248] == 0x6E && buffer[249] == 0x6F && buffer[250] == 0x6E &&
			buffer[258] == 0x45 && buffer[259] == 0x4F && buffer[260] == 0x53 && buffer[261] == 0x20 && buffer[262] == 0x36 &&
			buffer[263] == 0x30 && buffer[264] == 0x44)
			return 6;
		if (buffer[256] == 0x35 && buffer[257] == 0x44 && buffer[258] == 0x20 && buffer[259] == 0x4D && buffer[260] == 0x61 &&
			buffer[261] == 0x72 && buffer[262] == 0x6B && buffer[263] == 0x20 && buffer[264] == 0x49 && buffer[265] == 0x49 && buffer[266] == 0x49)
			return 5;
	}
	return 0;
}


bool cdfc_common_read(HANDLE hDisk, unsigned __int64 nPos, char *szBuffer, unsigned __int64 nSize, DWORD *nDwSize, bool bPos)
{
	if (bPos)
	{
		s_li.QuadPart = nPos;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	}
	s_bRet = ReadFile(hDisk, szBuffer, nSize, nDwSize, NULL);
	return s_bRet;
}

bool cdfc_common_write(HANDLE hDisk, unsigned __int64 nPos, char *szBuffer, unsigned __int64 nSize, DWORD *nDwSize, bool bPos)
{
	if (bPos)
	{
		s_li.QuadPart = nPos;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	}

	s_bRet = WriteFile(hDisk, szBuffer, nSize, nDwSize, NULL);
	return s_bRet;
}

void cdfc_dahua_set_preview(unsigned __int64 nSize)
{
	m_nPreViewSize = nSize;
}

void cdfc_dahua_set_regionsize(unsigned __int64 nSize)
{
	m_nRegionsize = nSize / 512;
}

bool cdfc_dahua_filesave(StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError)
{
	if (!szFile || !szFile_2 || !hDisk)
	{
		if (nError)
			(*nError) = 1;
		return false;
	}

	if (size_sec == 0)
		size_sec = __dh_get_file_size(hDisk) / 512;

	unsigned __int64 n = 0, n2 = 0, buffsize = 50 * 1024 * 1024, n3 = 0;		//n2统计已经读取的大小
	StStartAdd *addTmp = NULL;
	DWORD dwWritenSize;
	char *szBuf = (char *)__malloc(buffsize);
	memset(szBuf, 0, buffsize);
	StImdh *file = (StImdh *)szFile;
	for (addTmp = file->stStAdd; addTmp; addTmp = addTmp->next)
	{
		s_li.QuadPart = addTmp->nStartAddress;
		if (s_li.QuadPart >= (size_sec * 512))
			break;
		n = addTmp->nSize / 512 * 512 + 512;
		if (n <= buffsize)
		{
			memset(szBuf, 0, buffsize);
			SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
			ReadFile(hDisk, szBuf, n, &dwWritenSize, NULL);
			WriteFile(szFile_2, szBuf, n, &dwWritenSize, NULL);
			(*nCurrSizeDW) += n;
		}
		else
		{
			for (; ; )
			{
				if ((n - n2) < buffsize)
					buffsize = (n - n2) / 512 * 512 + 1;

				s_li.QuadPart = addTmp->nStartAddress + n2;
				memset(szBuf, 0, 50 * 1024 * 1024);
				SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
				ReadFile(hDisk, szBuf, buffsize, &dwWritenSize, NULL);
				WriteFile(szFile_2, szBuf, buffsize, &dwWritenSize, NULL);

				n2 += buffsize;
				if (buffsize < (50 * 1024 * 1024))
				{
					(*nCurrSizeDW) += buffsize;
					break;
				}
				(*nCurrSizeDW) += buffsize;
			}
		}

		if ((n3 += n) >= m_nPreViewSize && m_nPreViewSize)
			break;
	}
	if (szBuf)
		free(szBuf);
	szBuf = NULL;
	if (nError)
		(*nError) = 0;

	return true;
}

bool cdfc_dahua_filesave_f(StImdh *szFile, HANDLE hDisk, HANDLE szFile_2, unsigned __int64 *nCurrSizeDW, int *nError)
{
	if (size_sec == 0)
		size_sec = __dh_get_file_size(hDisk) / 512;

	DWORD dwWritenSize;
	char *szBuf = (char *)__malloc(2 * 1024 * 1024);
	memset(szBuf, 0, 2 * 1024 * 1024);
	unsigned __int64 n = 0, n3 = 0;
	StImdh *file = szFile;
	StImdh *stFileTmp = s_stFileExit = file;
	StStartAdd *ff = (StStartAdd *)stFileTmp->stStAdd;
	while (ff)
	{
		s_li.QuadPart = ff->nStartAddress;
		if (s_li.QuadPart >= (size_sec * 512))
			break;
		SetFilePointer(hDisk, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(hDisk, szBuf, 2 * 1024 * 1024, &dwWritenSize, NULL);
		m_nDWSize += 2 * 1024 * 1024;
		(*nCurrSizeDW) += 2 * 1024 * 1024;
		WriteFile(szFile_2, szBuf, 2 * 1024 * 1024, &dwWritenSize, NULL);

		if ((n3 += 2097152) >= m_nPreViewSize && m_nPreViewSize)	//预览保存
			break;
		ff = ff->next;
	}
	if (szBuf)
		free(szBuf);
	szBuf = NULL;
	return true;
}

//static int m_nType = 0;
StDate *cdfc_dahua_search_start_4(HANDLE hDisk, int nType)
{
	m_nType = nType;
	if (nType == 3)
	{
		return cdfc_dahua_search_start_3(hDisk, NULL, nType, NULL);
	}
	if (nType == 2)
	{
		return __recovery_searchfile_2(hDisk, nType);
	}
	if (!hDisk)
	{
		return NULL;
	}

	StLog *stLogo1 = (StLog *)__malloc(sizeof(StLog));
	int8_t date1[6], date2[6];
	unsigned __int64 time1 = 0, time2 = 0, time3 = 0, i = 0, j3 = 0;
	dhFrameHeader *header = (dhFrameHeader *)malloc(sizeof(dhFrameHeader));
	dhFrameTail *tail = (dhFrameTail *)malloc(sizeof(dhFrameTail));
	stFrame_dh *frame = NULL;
	StImdh *stTmpFile = NULL, *stTmpFileNext = NULL;
	StImdh *stFile1 = NULL, *stFile2 = NULL;
	StDate *stData1 = NULL, *stData2 = NULL;
	StStartAdd *stStartAdd1 = NULL, *stStartAdd2 = NULL;
	bool bMap = false;
	s_f_IMG = hDisk;
	unsigned __int64 nBufferSize = 512 * 204800, nReadSize = 0;	//nReadSize统计100M中已经读取的大小,超过100M的处理
	char *szSearchBuff = (char *)__malloc(nBufferSize);

	bool b1 = false;
	for (s_bStop = true; s_i < (size_sec - 1) && s_bStop; s_i = ((s_li.QuadPart + i) / 512 + 1) + m_nRegionsize)
	{
		//每次都读取100M数据
		s_li.QuadPart = s_i * 512;
		SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(s_f_IMG, szSearchBuff, nBufferSize, &s_dwReadSize, NULL);
		if (nBufferSize != s_dwReadSize)
			nBufferSize = s_dwReadSize;
		if (s_dwReadSize <= 0)
			break;
		//开始解析这100M数据
		for (i = 0, j3 = 0; i < (nBufferSize - 4); i++)
		{
			s_i = (s_li.QuadPart + i) / 512;
			j3 = (s_li.QuadPart + i) % 512;
			if (j3 == 0 && m_bFS_Free)
			{
				if (__count_freespace_b(s_i))
				{
					i = i + 2097151;	//如果这个扇区有被占用，那么跳过2MB
					continue;
				}
			}

			//找到帧的标识位44 48 41 56
			if (szSearchBuff[0 + i] == 0x44 && szSearchBuff[1 + i] == 0x48 && szSearchBuff[2 + i] == 0x41 && szSearchBuff[3 + i] == 0x56)
			{
				if (i > nBufferSize)		//处理最后一个帧在100M以外
				{
					i = i - frame->Size;	//把大小还原到之前没越界前
					s_i = (s_li.QuadPart + i) / 512;
					break;
				}

				memcpy(header, szSearchBuff + i, sizeof(dhFrameHeader));		//判断帧的合法性
				if (header->size > sizeof(dhFrameHeader))
				{
					if ((i + header->size) > nBufferSize)		//检测在尾部检查的时候是否尾部越界
					{
						s_i = (s_li.QuadPart + i) / 512;
						break;
					}
					//验证帧的尾部是否是这个帧的
					memcpy(tail, szSearchBuff + i + header->size - sizeof(dhFrameTail), sizeof(dhFrameTail));
					if (tail->tail == END_HEADER && header->size == tail->size)
					{
						if (frame == NULL)		//到这一步说明帧是合法帧,组碎片
						{
							b1 = true;
							frame = (stFrame_dh *)__malloc(sizeof(stFrame_dh));
							frame->StartDate = frame->EndDate = header->date;
							frame->Channel = header->channel;
							frame->Size = header->size;
							//frame->nStartAddress = ( s_i * 512 + i ) / 512 * 512;
							frame->nStartAddress = s_i * 512;
							i += header->size - 1;		//-1是因为循环那里还会加1所以先减掉这个1
							continue;
						}
						else
						{
							if (frame->Channel == header->channel)
							{
								__get_data(frame->EndDate, date1);
								__get_data(header->date, date2);
								time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
								time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
								if (time2 >= time1)
								{
									if ((time2 - time1) <= 10)
									{
										b1 = true;
										frame->EndDate = header->date;
										frame->Size += header->size;
										i += header->size - 1;
										continue;
									}
								}
							}
						}
					}
					else
						continue;
				}
				else
					continue;
			ff:
				//组文件
				if (!m_stFile)
				{
					m_stFile = (StDate *)__malloc(sizeof(StDate));
					m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
					m_stFile->nDate = frame->StartDate;
					m_stFile->File->nSize = frame->Size;
					m_stFile->File->nChannelNO = frame->Channel;
					m_stFile->File->nStartDate = frame->StartDate;
					m_stFile->File->nEndDate = frame->EndDate;

					m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
					m_stFile->File->stStAdd->nStartAddress = frame->nStartAddress;
					m_stFile->File->stStAdd->nSize = frame->Size;
					m_stFile->File->stStAdd->nStartDate = frame->StartDate;
					m_stFile->File->stStAdd->nEndDate = frame->EndDate;
					m_stFile->File->stStAdd->nChannelNO = frame->Channel;
				}
				else
				{
					for (stData1 = m_stFile; stData1; stData1 = stData1->next)
					{
						stData2 = stData1;
						//日期判断，定位到某个日期列表中
						__get_data(stData1->nDate, date1);
						__get_data(frame->StartDate, date2);
						if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
						{
							for (stFile1 = stData1->File; stFile1; stFile1 = stFile1->next)
							{
								stFile2 = stFile1;
								//对于超过1G的文件不再往后添加,跳过
								if (stFile1->nSize >= 1073741824)
									continue;

								if (stFile1->nChannelNO == frame->Channel)
								{
									//如果新的帧的起始时间与老文件的结束时间差4秒就是一个文件
									__get_data(stFile1->nEndDate, date1);
									time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
									time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
									//如果两个文件是同一通道号,同一日期,相同的起始时间,说明不对,跳过这个
									if (frame->StartDate >= stFile1->nStartDate && frame->StartDate < stFile1->nEndDate)
										goto f;

									if (time2 >= time1)
									{
										if ((time2 - time1) <= 10)
										{
											for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next);
											stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
											stStartAdd1->next->nStartAddress = frame->nStartAddress;
											stStartAdd1->next->nSize = frame->Size;
											stStartAdd1->next->nStartDate = frame->StartDate;
											stStartAdd1->next->nEndDate = frame->EndDate;
											stStartAdd1->next->nChannelNO = frame->Channel;


											stFile1->nSize += frame->Size;
											stFile1->nEndDate = frame->EndDate;

											goto f;
										}
									}
								}
							}
							//没有找到就新建一个
							if (stFile1 == NULL)
							{
								stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
								stFile2->next->nSize = frame->Size;
								stFile2->next->nStartDate = frame->StartDate;
								stFile2->next->nEndDate = frame->EndDate;
								stFile2->next->nChannelNO = frame->Channel;

								stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
								stFile2->next->stStAdd->nStartAddress = frame->nStartAddress;
								stFile2->next->stStAdd->nSize = frame->Size;
								stFile2->next->stStAdd->nStartDate = frame->StartDate;
								stFile2->next->stStAdd->nEndDate = frame->EndDate;
								stFile2->next->stStAdd->nChannelNO = frame->Channel;

								goto f;
							}
						}
					}
					if (stData1 == NULL)
					{
						stData2->next = (StDate *)__malloc(sizeof(StDate));
						stData2->next->nDate = frame->StartDate;
						stData2->next->File = (StImdh *)__malloc(sizeof(StImdh));
						stData2->next->File->nSize = frame->Size;
						stData2->next->File->nStartDate = frame->StartDate;
						stData2->next->File->nEndDate = frame->EndDate;
						stData2->next->File->nChannelNO = frame->Channel;

						stData2->next->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
						stData2->next->File->stStAdd->nStartAddress = frame->nStartAddress;
						stData2->next->File->stStAdd->nSize = frame->Size;
						stData2->next->File->stStAdd->nStartDate = frame->StartDate;
						stData2->next->File->stStAdd->nEndDate = frame->EndDate;
						stData2->next->File->stStAdd->nChannelNO = frame->Channel;
					}
				}
			f:
				if (frame)
				{
					b1 = false;
					free(frame);
					frame = NULL;
					i -= 1;
				}
			}
			else
			{
				//判断是没有找到帧的还是已经有帧的
				if (b1)
				{
					b1 = false;
					goto ff;
				}
			}
		}
	}
	if (header)
		free(header);
	if (tail)
		free(tail);
	if (szSearchBuff)
		free(szSearchBuff);
	if (frame)
		free(frame);
	if (stLogo1)
		free(stLogo1);
	header = NULL;
	tail = NULL;
	szSearchBuff = NULL;
	frame = NULL;
	stLogo1 = NULL;

	return m_stFile;
}

StDate *cdfc_dahua_search_start_3(HANDLE hDisk, unsigned __int64 *nCountFile, int nType, int *nError)
{
	if (!hDisk || !nCountFile)
	{
		if (nError)
			(*nError) = 1;
		return NULL;
	}

	int8_t date1[6], date2[6];
	unsigned __int64 time1 = 0, time2 = 0, time3 = 0, i = 0, nCuSize, j3 = 0;
	dhFrameHeader *header = (dhFrameHeader *)malloc(sizeof(dhFrameHeader));
	dhFrameTail *tail = (dhFrameTail *)malloc(sizeof(dhFrameTail));
	stFrame_dh *frame = NULL;
	StImdh *stTmpFile = NULL, *stTmpFileNext = NULL;
	StImdh *stFile1 = NULL, *stFile2 = NULL;
	StDate *stData1 = NULL, *stData2 = NULL;
	StStartAdd *stStartAdd1 = NULL, *stStartAdd2 = NULL;
	s_f_IMG = hDisk;
	unsigned __int64 nBufferSize = 512 * 204800, nReadSize = 0;	//nReadSize统计100M中已经读取的大小,超过100M的处理
	char *szSearchBuff = (char *)__malloc(nBufferSize);

	bool b1 = false;
	for (s_bStop = true; s_i < (size_sec - 1) && s_bStop; s_i = ((s_li.QuadPart + i) / 512 + 1) + m_nRegionsize)
	{
		//每次都读取100M数据
		s_li.QuadPart = s_i * 512;
		SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(s_f_IMG, szSearchBuff, nBufferSize, &s_dwReadSize, NULL);
		if (nBufferSize != s_dwReadSize)
			nBufferSize = s_dwReadSize;
		//开始解析这100M数据
		for (i = 0; i < (nBufferSize - 5); i++)
		{
			s_i = (s_li.QuadPart + i) / 512;
			j3 = (s_li.QuadPart + i) % 512;
			if (j3 == 0 && m_bFS_Free)
			{
				if (__count_freespace_b(s_i))
				{
					i = i + 2097151;	//如果这个扇区有被占用，那么跳过2MB
					continue;
				}
			}
			//找到帧的标识位44 48 41 56
			if (szSearchBuff[0 + i] == 0x44 && szSearchBuff[1 + i] == 0x48 && szSearchBuff[2 + i] == 0x41 && szSearchBuff[3 + i] == 0x56 &&
				szSearchBuff[4 + i] == 0xFFFFFFF0)
			{
				if (i > nBufferSize)		//处理最后一个帧在100M以外
				{
					i = i - frame->Size;	//把大小还原到之前没越界前
					s_i = (s_li.QuadPart + i) / 512;
					break;
				}

				memcpy(header, szSearchBuff + i, sizeof(dhFrameHeader));		//判断帧的合法性
				if (header->size > sizeof(dhFrameHeader))
				{
					if ((i + header->size) > nBufferSize)		//检测在尾部检查的时候是否尾部越界
					{
						s_i = (s_li.QuadPart + i) / 512;
						break;
					}
					//验证帧的尾部是否是这个帧的
					memcpy(tail, szSearchBuff + i + header->size - sizeof(dhFrameTail), sizeof(dhFrameTail));
					if (tail->tail == END_HEADER && header->size == tail->size)
					{
						if (frame == NULL)		//到这一步说明帧是合法帧,组碎片
						{
							b1 = true;
							frame = (stFrame_dh *)__malloc(sizeof(stFrame_dh));
							frame->StartDate = frame->EndDate = header->date;
							frame->Channel = header->channel;
							frame->FrameNOStart = frame->FrameNOEnd = header->frameNO;	//起始帧号
							frame->Size = header->size;
							frame->nStartAddress = s_i * 512;
							i += header->size - 1;		//-1是因为循环那里还会加1所以先减掉这个1
							continue;
						}
						else
						{
							if (frame->Channel == header->channel)
							{
								__get_data(frame->EndDate, date1);
								__get_data(header->date, date2);
								time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
								time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
								if (time2 >= time1)
								{
									if ((time2 - time1) <= 10)
									{
										if (header->frameNO >= frame->FrameNOEnd)	//帧号判断
										{
											if ((header->frameNO - frame->FrameNOEnd) <= 5)
											{
												b1 = true;
												frame->Size = s_i * 512 - frame->nStartAddress;
												frame->EndDate = header->date;
												frame->FrameNOEnd = header->frameNO;	//结束帧号
												i += header->size - 1;
												continue;
											}
										}
									}
								}
							}
						}
					}
					else
					{
						if (frame)
							goto ff;
						else
							continue;
					}
				}
				else
					continue;
			ff:
				//组文件
				if (!m_stFile)
				{
					m_stFile = (StDate *)__malloc(sizeof(StDate));
					m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
					m_stFile->nDate = frame->StartDate;
					m_stFile->File->nSize = frame->Size;
					m_stFile->File->nChannelNO = frame->Channel;
					m_stFile->File->nStartDate = frame->StartDate;
					m_stFile->File->nEndDate = frame->EndDate;
					m_stFile->File->nFrameNO = frame->FrameNOEnd;

					m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
					m_stFile->File->stStAdd->nStartAddress = frame->nStartAddress;
					m_stFile->File->stStAdd->nSize = frame->Size;
					m_stFile->File->stStAdd->nStartDate = frame->StartDate;
					m_stFile->File->stStAdd->nEndDate = frame->EndDate;
					m_stFile->File->stStAdd->nChannelNO = frame->Channel;
				}
				else
				{
					for (stData1 = m_stFile; stData1; stData1 = stData1->next)
					{
						stData2 = stData1;
						//日期判断，定位到某个日期列表中
						__get_data(stData1->nDate, date1);
						__get_data(frame->StartDate, date2);
						if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
						{
							for (stFile1 = stData1->File; stFile1; stFile1 = stFile1->next)
							{
								stFile2 = stFile1;
								//对于超过1G的文件不再往后添加,跳过
								if (stFile1->nSize >= 1073741824)
									continue;

								if (stFile1->nChannelNO == frame->Channel)
								{
									//如果新的帧的起始时间与老文件的结束时间差4秒就是一个文件
									__get_data(stFile1->nEndDate, date1);
									time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
									time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
									//如果两个文件是同一通道号,同一日期,相同的起始时间,说明不对,跳过这个
									//if( frame->StartDate >= stFile1->nStartDate && frame->StartDate < stFile1->nEndDate )
									//	goto f;

									if (time2 >= time1)
									{
										if ((time2 - time1) <= 10)
										{
											if (frame->FrameNOStart >= stFile1->nFrameNO)
											{
												if ((frame->FrameNOStart - stFile1->nFrameNO) <= 5)	//帧的第一个号与文件的最后一个帧
												{
													for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next);
													stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
													stStartAdd1->next->nStartAddress = frame->nStartAddress;
													stStartAdd1->next->nSize = frame->Size;
													stStartAdd1->next->nStartDate = frame->StartDate;
													stStartAdd1->next->nEndDate = frame->EndDate;
													stStartAdd1->next->nChannelNO = frame->Channel;

													stFile1->nSize += frame->Size;
													stFile1->nEndDate = frame->EndDate;
													stFile1->nFrameNO = frame->FrameNOEnd;

													goto f;
												}
											}
										}
									}
								}
							}
							//没有找到就新建一个
							if (stFile1 == NULL)
							{
								stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
								stFile2->next->nSize = frame->Size;
								stFile2->next->nStartDate = frame->StartDate;
								stFile2->next->nEndDate = frame->EndDate;
								stFile2->next->nChannelNO = frame->Channel;
								stFile2->next->nFrameNO = frame->FrameNOEnd;

								stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
								stFile2->next->stStAdd->nStartAddress = frame->nStartAddress;
								stFile2->next->stStAdd->nSize = frame->Size;
								stFile2->next->stStAdd->nStartDate = frame->StartDate;
								stFile2->next->stStAdd->nEndDate = frame->EndDate;
								stFile2->next->stStAdd->nChannelNO = frame->Channel;

								goto f;
							}
						}
					}
					if (stData1 == NULL)
					{
						stData2->next = (StDate *)__malloc(sizeof(StDate));
						stData2->next->nDate = frame->StartDate;
						stData2->next->File = (StImdh *)__malloc(sizeof(StImdh));
						stData2->next->File->nSize = frame->Size;
						stData2->next->File->nStartDate = frame->StartDate;
						stData2->next->File->nEndDate = frame->EndDate;
						stData2->next->File->nChannelNO = frame->Channel;
						stData2->next->File->nFrameNO = frame->FrameNOEnd;

						stData2->next->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
						stData2->next->File->stStAdd->nStartAddress = frame->nStartAddress;
						stData2->next->File->stStAdd->nSize = frame->Size;
						stData2->next->File->nStartDate = frame->StartDate;
						stData2->next->File->nEndDate = frame->EndDate;
						stData2->next->File->nChannelNO = frame->Channel;
					}
				}
			f:
				if (frame)
				{
					b1 = false;
					free(frame);
					frame = NULL;
					i -= 1;
				}
			}
#if 0
			else
			{
				//判断是没有找到帧的还是已经有帧的
				if (b1)
				{
					b1 = false;
					goto ff;
				}
			}
#endif
		}
	}
	if (header)
		free(header);
	if (header)
		free(tail);
	if (header)
		free(szSearchBuff);
	if (header)
		free(frame);
	header = NULL;
	tail = NULL;
	szSearchBuff = NULL;
	frame = NULL;
	return m_stFile;
}

StDate *cdfc_dahua_search_start(HANDLE hDisk, int nType, int *nError)
{
	stFramelist_dh *stFramelist = NULL, *stFramelistnext = NULL;
	int8_t date1[6], date2[6];
	unsigned __int64 time1 = 0, time2 = 0, time3 = 0, i = 0, nCuSize, j3 = 0, nFrameNumCount = 0, ttttt = 0;
	dhFrameHeader *header = (dhFrameHeader *)malloc(sizeof(dhFrameHeader));
	dhFrameTail *tail = (dhFrameTail *)malloc(sizeof(dhFrameTail));
	stFrame_dh *frame = NULL;
	StImdh *stTmpFile = NULL, *stTmpFileNext = NULL;
	StImdh *stFile1 = NULL, *stFile2 = NULL;
	StDate *stData1 = NULL, *stData2 = NULL;
	StStartAdd *stStartAdd1 = NULL, *stStartAdd2 = NULL;
	s_f_IMG = hDisk;
	unsigned __int64 nBufferSize = 512 * 204800, nReadSize = 0, nFCount = 0;	//nReadSize统计100M中已经读取的大小,超过100M的处理
	char *szSearchBuff = (char *)__malloc(nBufferSize);
	int FrameBuff[512];
	memset(FrameBuff, 0, 512 * sizeof(int));

	bool b1 = false;
	for (s_bStop = true; s_i < (size_sec - 1) && s_bStop; s_i = ((s_li.QuadPart + i) / 512 + 1) + m_nRegionsize)
	{
		//每次都读取100M数据
		s_li.QuadPart = s_i * 512;
		SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(s_f_IMG, szSearchBuff, nBufferSize, &s_dwReadSize, NULL);
		if (nBufferSize != s_dwReadSize)
			nBufferSize = s_dwReadSize;
		//开始解析这100M数据
		for (i = 0; i < (nBufferSize - 5); i++)
		{
			s_i = (s_li.QuadPart + i) / 512;
			//找到帧的标识位44 48 41 56
			if (szSearchBuff[0 + i] == 0x44 && szSearchBuff[1 + i] == 0x48 && szSearchBuff[2 + i] == 0x41 && szSearchBuff[3 + i] == 0x56 &&
				(szSearchBuff[4 + i] == 0xFFFFFFFC || szSearchBuff[4 + i] == 0xFFFFFFFD))
			{
				if (i > nBufferSize)		//处理最后一个帧在100M以外
				{
					i = i - frame->Size;	//把大小还原到之前没越界前
					s_i = (s_li.QuadPart + i) / 512;
					break;
				}

				memcpy(header, szSearchBuff + i, sizeof(dhFrameHeader));		//判断帧的合法性
				if (header->size > sizeof(dhFrameHeader))
				{
					if ((i + header->size) > nBufferSize)		//检测在尾部检查的时候是否尾部越界
					{
						s_i = (s_li.QuadPart + i) / 512;
						break;
					}
					if (header->channel > 64 || header->channel < 0)
						header->channel = 100;	//遇到过一个监控通道号非常大

					if (frame == NULL)		//到这一步说明帧是合法帧,组碎片
					{
						b1 = true;
						frame = (stFrame_dh *)__malloc(sizeof(stFrame_dh));
						frame->StartDate = frame->EndDate = header->date;
						frame->Channel = header->channel;
						frame->FrameNOStart = frame->FrameNOEnd = header->frameNO;	//起始帧号
						frame->Size = header->size;
						frame->nStartAddress = s_i * 512;
						i += 28;		//-1是因为循环那里还会加1所以先减掉这个1
						continue;
					}
					else
					{
						if (frame->Channel == header->channel)
						{
							__get_data(frame->EndDate, date1);
							__get_data(header->date, date2);
							time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
							time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
							if (time2 >= time1)
							{
								if ((time2 - time1) <= 600)
								{
									if (header->frameNO >= frame->FrameNOEnd)	//帧号判断
									{
										//if( ( ( header->frameNO - frame->FrameNOEnd ) == nFrameNumCount && nFrameNumCount ) || nFrameNumCount == 0 || nFrameNumCount == 1 || nFrameNumCount == 2 )
										{
											if (nFrameNumCount == 0)
												nFrameNumCount = header->frameNO - frame->FrameNOEnd;
											b1 = true;
											frame->Size = s_i * 512 - frame->nStartAddress;
											frame->EndDate = header->date;
											frame->FrameNOEnd = header->frameNO;	//结束帧号
											i += 28;
											continue;
										}
									}
								}
							}
						}
					}
				}
				else
					continue;

			f:
				if (stFramelist == NULL)
					stFramelistnext = stFramelist = (stFramelist_dh *)__malloc(sizeof(stFramelist_dh));
				else
				{
					stFramelistnext->next = (stFramelist_dh *)__malloc(sizeof(stFramelist_dh));
					stFramelistnext = stFramelistnext->next;
				}
				stFramelistnext->Channel = frame->Channel;
				stFramelistnext->nStartAddress = frame->nStartAddress;
				stFramelistnext->Size = frame->Size;
				stFramelistnext->StartDate = frame->StartDate;
				stFramelistnext->EndDate = frame->EndDate;
				stFramelistnext->FrameNOEnd = frame->FrameNOEnd;
				stFramelistnext->FrameNOStart = frame->FrameNOStart;
				i -= 1;
				if (frame)
					free(frame);
				frame = NULL;

				if (!m_stFile)
				{
					m_stFile = (StDate *)__malloc(sizeof(StDate));
					m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
					m_stFile->nDate = stFramelistnext->StartDate;
					m_stFile->File->nSize = stFramelistnext->Size;
					m_stFile->File->nChannelNO = stFramelistnext->Channel;
					m_stFile->File->nStartDate = stFramelistnext->StartDate;
					m_stFile->File->nEndDate = stFramelistnext->EndDate;
					m_stFile->File->nFrameNO = stFramelistnext->FrameNOEnd;

					m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
					m_stFile->File->stStAdd->nStartAddress = stFramelistnext->nStartAddress;
					m_stFile->File->stStAdd->nSize = stFramelistnext->Size;
					m_stFile->File->stStAdd->nStartDate = stFramelistnext->StartDate;
					m_stFile->File->stStAdd->nEndDate = stFramelistnext->EndDate;
					m_stFile->File->stStAdd->nChannelNO = stFramelistnext->Channel;
					nFCount++;
				}
				else
				{
					for (stFile1 = m_stFile->File; stFile1; stFile1 = stFile1->next)
					{
						stFile2 = stFile1;
						//对于超过1G的文件不再往后添加,跳过
						if (stFile1->nSize >= 1073741824)
							continue;

						if (stFile1->nChannelNO == stFramelistnext->Channel)
						{
							//__get_data( stFile1->nEndDate, date1 );
							//time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
							//time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];

							int nt = 0;
							if (stFramelistnext->StartDate >= stFile1->nEndDate)
								nt = stFramelistnext->StartDate - stFile1->nEndDate;
							else
								nt = stFile1->nEndDate - stFramelistnext->StartDate;

							if (nt <= 600)
							{
								if (stFramelistnext->FrameNOStart >= stFile1->nFrameNO)
								{
									int nnnn = stFramelistnext->FrameNOStart - stFile1->nFrameNO;
									//if( ( nnnn <= ( nFrameNumCount + 5 ) && nnnn >= ( nFrameNumCount ) ) || nnnn == 0 || nnnn == 1 || nnnn == 2 )	//帧的第一个号与文件的最后一个帧
									{
										int bF = false;	//碎片链表里是否有其它碎片通道号,false没有,true有
										for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next)
										{
											if (stStartAdd1->nChannelNO != stFramelistnext->Channel)
												bF = true;
										}
										if (bF == false)
										{
											stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
											stStartAdd1->next->nStartAddress = stFramelistnext->nStartAddress;
											stStartAdd1->next->nSize = stFramelistnext->Size;
											stStartAdd1->next->nStartDate = stFramelistnext->StartDate;
											stStartAdd1->next->nEndDate = stFramelistnext->EndDate;
											stStartAdd1->next->nChannelNO = stFramelistnext->Channel;

											stFile1->nSize += stFramelistnext->Size;
											stFile1->nEndDate = stFramelistnext->EndDate;
											stFile1->nFrameNO = stFramelistnext->FrameNOEnd;
											break;
										}
									}
								}
							}
						}
					}
					//没有找到就新建一个
					if (stFile1 == NULL)
					{
						stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
						stFile2->next->nSize = stFramelistnext->Size;
						stFile2->next->nStartDate = stFramelistnext->StartDate;
						stFile2->next->nEndDate = stFramelistnext->EndDate;
						stFile2->next->nChannelNO = stFramelistnext->Channel;
						stFile2->next->nFrameNO = stFramelistnext->FrameNOEnd;

						stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
						stFile2->next->stStAdd->nStartAddress = stFramelistnext->nStartAddress;
						stFile2->next->stStAdd->nSize = stFramelistnext->Size;
						stFile2->next->stStAdd->nStartDate = stFramelistnext->StartDate;
						stFile2->next->stStAdd->nEndDate = stFramelistnext->EndDate;
						stFile2->next->stStAdd->nChannelNO = stFramelistnext->Channel;
						nFCount++;
					}
				}
			}
		}
	}
	if (frame)
		goto f;
#if 0
	StImdh *filenext = NULL;
	//这里开始重组碎片到文件
	for (stFramelistnext = stFramelist; stFramelistnext; stFramelistnext = stFramelistnext->next)
	{

	}
#endif
	for (; stFramelist; stFramelist = stFramelistnext)
	{
		stFramelistnext = stFramelist->next;
		free(stFramelist);
		stFramelist = NULL;
	}

	if (header)
		free(header);
	if (tail)
		free(tail);
	if (szSearchBuff)
		free(szSearchBuff);
	if (frame)
		free(frame);
	header = NULL;
	tail = NULL;
	szSearchBuff = NULL;
	frame = NULL;
	return m_stFile;
}

StDate *cdfc_dahua_search_start_f(HANDLE hDisk, int nType, int *nError)
{
	m_nType = nType;
	m_nFS = 1;
	if (hDisk == NULL)
	{
		return NULL;
	}
	m_nCountFragment = 0;
	m_nFS = 1;
	s_f_IMG = hDisk;
	m_stFile = NULL;
	unsigned __int64 nLen = 0, nLen2 = 0, nTmpAddress = 0;
	StImdh *stTmpFile = NULL;
	StImdh *stTmpFileNext = NULL;
	StDate *stTmpDate = NULL;
	StDate *stTmpDate2 = NULL;
	StImdh *stTmpFile2 = NULL;
	StStartAdd *stSAdd = NULL;
	int8_t date1[6], date2[6];
	int n = 0, k = 0;
	char szBuff[32];
	memset(szBuff, 0, 32);
	StFSHeader *stFs = (StFSHeader *)__malloc(sizeof(StFSHeader));
	__init_start_pos(&n);
	//if( n == 0 || ( n >= ( size_sec * 512 ) ) )
	if (n == 0)
	{
		//		( *nError ) = 2;
		return NULL;
	}
	if (nType == 2)
	{
		k = 131072;
	}
	if (nType == 3)
	{
		k = 65536;
	}
	__read_file(MFTPOS, (char *)stFs, sizeof(StFSHeader));
	for (; stFs->bFile && s_bStop; nLen2 = 0)
	{
		if ((stFs->bFile >= 0x10 || stFs->bFile == 0xFFFFFFF6 || stFs->bFile >= 0xFFFFFF10) && stFs->bFile != 0xFFFFFFFE)
			break;
		for (; nLen2 != 512; nLen2 += 32, nLen += 32)
		{
			memcpy(stFs, m_Tmpbuff + nLen2, 32);
			if (stFs->bFile == 0xFFFFFFFE)
				continue;
			if (stFs->bFile == 0x01 || stFs->bFile == 0x03)
			{
				if (!stFile_tmp)
				{
					stFile_tmp = (StDate *)__malloc(sizeof(StDate));
					stFile_tmp->File = (StImdh *)__malloc(sizeof(StImdh));
					stFile_tmp->nDate = stFs->nStartDate;
					stFile_tmp->File->nChannelNO = stFs->nChannelNO & 0x0F;
					stFile_tmp->File->nStartDate = stFs->nStartDate;
					stFile_tmp->File->nEndDate = stFs->nEndDate;
					//dhfs1
					//stFile_tmp->File->nStartAddress = ( unsigned __int64 )( stFs->nStartAddress * 4096 + n ) * 512;
					stFile_tmp->File->nStartAddress = (unsigned __int64)(stFs->nStartAddress * 4096 + n + k) * 512;
					stFile_tmp->File->nSize += 2097152;
					// 					if( m_bFS_Free )	//如果是剩下空间扫描需要统计已经占用的空间
					// 						__count_freespace( stFile_tmp->File->nStartAddress );
					stTmpFileNext = stTmpFile = stFile_tmp->File;
				}
				else
				{
					for (stTmpDate = stFile_tmp; stTmpDate; stTmpDate = stTmpDate->next)
					{
						stTmpDate2 = stTmpDate;
						__get_data(stTmpDate->nDate, date1);
						__get_data(stFs->nStartDate, date2);
						//判断是否是同一天
						if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
						{
							stTmpFile2 = stTmpDate->File;
							while (stTmpFile2->next) {
								stTmpFile2 = stTmpFile2->next;
							}

							stTmpFile2->next = (StImdh *)__malloc(sizeof(StImdh));
							stTmpFile2->next->nChannelNO = stFs->nChannelNO & 0x0F;
							stTmpFile2->next->nStartDate = stFs->nStartDate;
							stTmpFile2->next->nEndDate = stFs->nEndDate;
							stTmpFile2->next->nStartAddress = (unsigned __int64)(stFs->nStartAddress * 4096 + n + k) * 512;
							// 							if( m_bFS_Free )	//如果是剩下空间扫描需要统计已经占用的空间
							// 								__count_freespace( stTmpFile2->next->nStartAddress );

							stTmpFile2->next->nSize += 2097152;
							break;
						}
					}
					//若未找到所属时间分类，则另外开辟一个时间分类实体;
					if (!stTmpDate)
					{
						stTmpDate2->next = (StDate *)__malloc(sizeof(StDate));
						stTmpDate2 = stTmpDate2->next;
						stTmpDate2->nDate = stFs->nStartDate;
						stTmpDate2->File = (StImdh *)__malloc(sizeof(StImdh));

						stTmpDate2->File->nChannelNO = stFs->nChannelNO & 0x0F;
						stTmpDate2->File->nStartDate = stFs->nStartDate;
						stTmpDate2->File->nEndDate = stFs->nEndDate;
						stTmpDate2->File->nStartAddress = (unsigned __int64)(stFs->nStartAddress * 4096 + n + k) * 512;
						// 						if( m_bFS_Free )	//如果是剩下空间扫描需要统计已经占用的空间
						// 							__count_freespace( stTmpDate2->File->nStartAddress );

						stTmpDate2->File->nSize += 2097152;
					}
				}
			}
			//碎片要先找到他所属的文件队列
			else if (stFs->bFile == 0x02)
			{
				//判断是否是同一天的
				/*
				这里有一个大的Bug待处理,因为你搜索到02后再根据日期往前判断属于哪个文件,这里问题就是,由于文件表没有搜索完,
				所以现在搜索出的02属于哪一天哪一通道不一定上面就有了,只有把整个文件表搜索完后才能确定这个02属于哪个日期通道
				*/
				for (stTmpDate = stFile_tmp; stTmpDate; stTmpDate = stTmpDate->next)
				{
					__get_data(stTmpDate->nDate, date1);
					__get_data(stFs->nStartDate, date2);
					if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
					{
						//判断是哪个文件的
						stTmpFile2 = stTmpDate->File;
						for (; stTmpFile2; stTmpFile2 = stTmpFile2->next)
						{
							int nt = stFs->nChannelNO & 0x0F;
							if (nt != stTmpFile2->nChannelNO)
								continue;
							nTmpAddress = (unsigned __int64)(stFs->nStartAddress * 4096 + n + k) * 512;
							if (stTmpFile2->nStartAddress == nTmpAddress)
							{
								stTmpFile2->nSize += 2097152;
								if (stTmpFile2->stStAdd == NULL)
								{
									stTmpFile2->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
									stTmpFile2->stStAdd->nStartAddress = (unsigned __int64)(stFs->nFragment * 4096 + n + k) * 512;
									stTmpFile2->stStAdd->nSize = 2097152;
									stTmpFile2->stStAdd->nStartDate = stFs->nStartDate;
									stTmpFile2->stStAdd->nEndDate = stFs->nEndDate;
									if (m_bFS_Free)	//如果是剩下空间扫描需要统计已经占用的空间
										__count_freespace(stTmpFile2->stStAdd->nStartAddress);

									break;
								}
								else
								{
									//插到碎片在最后一项
									for (stSAdd = stTmpFile2->stStAdd; stSAdd->next; stSAdd = stSAdd->next);
									stSAdd->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
									stSAdd->next->nSize = 2097152;
									stSAdd->next->nStartAddress = (unsigned __int64)(stFs->nFragment * 4096 + n + k) * 512;
									stSAdd->next->nStartDate = stFs->nStartDate;
									stSAdd->next->nEndDate = stFs->nEndDate;
									if (m_bFS_Free)	//如果是剩下空间扫描需要统计已经占用的空间
										__count_freespace(stSAdd->next->nStartAddress);

									break;
								}
							}
						}
					}
				}
			}
			else
				break;
		}
	R:
		__read_file(MFTPOS + nLen, (char *)stFs, sizeof(StFSHeader));
	}

	return stFile_tmp;
}

void cdfc_dahua_freetask(StDate *stFile)
{
	if (stFile)
	{
		StStartAdd *startadd = NULL, *startaddnext = NULL;
		StImdh *imdh = NULL, *imdhnext = NULL;
		StDate *data = (StDate *)stFile, *datanext;

		for (; data; data = datanext)
		{
			datanext = data->next;
			for (imdhnext = imdh = data->File; imdh; imdh = imdhnext)
			{
				imdhnext = imdh->next;
				////////////////////////////////////////////////////////////////////////////////////////////////////
				for (startaddnext = startadd = imdh->stStAdd; startadd; startadd = startaddnext)
				{
					startaddnext = startadd->next;

					free(startadd);
					startadd = NULL;
				}
				////////////////////////////////////////////////////////////////////////////////////////////////////
				free(imdh);
				imdh = NULL;
			}
			free(data);
			data = NULL;
		}
		if (m_Tmpbuff)
		{
			free(m_Tmpbuff);
			m_Tmpbuff = NULL;
		}
		stFile = NULL;
	}
}

void cdfc_dahua_exit()
{
	cdfc_dahua_freetask(m_stFile_2);
	cdfc_dahua_freetask(m_stFile);
	if (m_szFragment)
	{
		free(m_szFragment);
		m_szFragment = NULL;
	}
	m_stFile = NULL;
}

void cdfc_dahua_date_converter(UINT32 nDate, int8_t *date)
{
	__get_data(nDate, date);
}

StDate *cdfc_dahua_search_start_free(HANDLE hDisk, int nType, int *nError)
{
	m_bFSearch = false;
	unsigned __int64 nDiskSize = size_sec * 512;
	(*nError) = 100;
	m_bFS_Free = false;
	//先扫描文件系统,统计文件中已经占用的扇区
	m_stFile_2 = cdfc_dahua_search_start_f(hDisk, nType, nError);

	m_stFile = m_stFileTmp = m_stFilePre = NULL;
	(*nError) = 101;
	//全盘
	cdfc_dahua_search_start(hDisk, nType, nError);
	return m_stFile;
}

////////////////////////////////////////////////////////////////////////
StDate *__recsovery_searchfile_2(HANDLE hDisk, int nType)
{
	StLog *stLogo1 = (StLog *)__malloc(sizeof(StLog));
	int8_t date1[6], date2[6];
	unsigned __int64 time1 = 0, time2 = 0, time3 = 0, i = 0, nCuSize, j3 = 0;
	dhFrameHeader *header = (dhFrameHeader *)malloc(sizeof(dhFrameHeader));
	dhFrameTail *tail = (dhFrameTail *)malloc(sizeof(dhFrameTail));
	stFrame_dh *frame = NULL;
	StImdh *stTmpFile = NULL, *stTmpFileNext = NULL;
	StImdh *stFile1 = NULL, *stFile2 = NULL;
	StDate *stData1 = NULL, *stData2 = NULL;
	StStartAdd *stStartAdd1 = NULL, *stStartAdd2 = NULL;
	s_f_IMG = hDisk;
	unsigned __int64 nBufferSize = 512 * 204800, nReadSize = 0;	//nReadSize统计100M中已经读取的大小,超过100M的处理
	char *szSearchBuff = (char *)__malloc(nBufferSize);

	bool b1 = false;
	for (s_bStop = true; s_i < (size_sec - 1) && s_bStop; s_i = ((s_li.QuadPart + i) / 512 + 1) + m_nRegionsize)
	{
		//每次都读取100M数据
		s_li.QuadPart = s_i * 512;
		SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(s_f_IMG, szSearchBuff, nBufferSize, &s_dwReadSize, NULL);
		if (nBufferSize != s_dwReadSize)
			nBufferSize = s_dwReadSize;
		if (s_dwReadSize <= 0)
			break;
		//开始解析这100M数据
		for (i = 0; i < (nBufferSize - 4); i++)
		{
			s_i = (s_li.QuadPart + i) / 512;
			j3 = (s_li.QuadPart + i) % 512;
			if (j3 == 0 && m_bFS_Free)
			{
				if (__count_freespace_b(s_i))
				{
					i = i + 2097151;	//如果这个扇区有被占用，那么跳过2MB
					continue;
				}
			}
			//找到帧的标识位44 48 41 56
			if (szSearchBuff[0 + i] == 0x44 && szSearchBuff[1 + i] == 0x48 && szSearchBuff[2 + i] == 0x41 && szSearchBuff[3 + i] == 0x56)
			{
				if (i > nBufferSize)		//处理最后一个帧在100M以外
				{
					i = i - frame->Size;	//把大小还原到之前没越界前
					s_i = (s_li.QuadPart + i) / 512;
					break;
				}

				memcpy(header, szSearchBuff + i, sizeof(dhFrameHeader));		//判断帧的合法性
				if (header->size > sizeof(dhFrameHeader))
				{
					if ((i + header->size) > nBufferSize)		//检测在尾部检查的时候是否尾部越界
					{
						s_i = (s_li.QuadPart + i) / 512;
						break;
					}
					//验证帧的尾部是否是这个帧的
					memcpy(tail, szSearchBuff + i + header->size - sizeof(dhFrameTail), sizeof(dhFrameTail));
					if (tail->tail == END_HEADER && header->size == tail->size)
					{
						if (frame == NULL)		//到这一步说明帧是合法帧,组碎片
						{
							b1 = true;
							frame = (stFrame_dh *)__malloc(sizeof(stFrame_dh));
							frame->StartDate = frame->EndDate = header->date;
							frame->Channel = header->channel;
							frame->Size = header->size;
							//frame->nStartAddress = ( s_i * 512 + i ) / 512 * 512;
							frame->nStartAddress = s_i * 512;
							i += header->size - 1;		//-1是因为循环那里还会加1所以先减掉这个1
							continue;
						}
						else
						{
							if (frame->Channel == header->channel)
							{
								__get_data(frame->EndDate, date1);
								__get_data(header->date, date2);
								time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
								time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
								if (time2 >= time1)
								{
									if ((time2 - time1) <= 10)
									{
										b1 = true;
										frame->EndDate = header->date;
										frame->Size += header->size;
										i += header->size - 1;
										continue;
									}
								}
							}
						}
					}
					else
						continue;
				}
				else
					continue;
			ff:
				//组文件
				if (!m_stFile)
				{
					m_stFile = (StDate *)__malloc(sizeof(StDate));
					m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
					m_stFile->nDate = frame->StartDate;
					m_stFile->File->nSize = frame->Size;
					m_stFile->File->nChannelNO = frame->Channel;
					m_stFile->File->nStartDate = frame->StartDate;
					m_stFile->File->nEndDate = frame->EndDate;

					m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
					m_stFile->File->stStAdd->nStartAddress = frame->nStartAddress;
					m_stFile->File->stStAdd->nSize = frame->Size;
				}
				else
				{
					for (stData1 = m_stFile; stData1; stData1 = stData1->next)
					{
						stData2 = stData1;
						//日期判断，定位到某个日期列表中
						__get_data(stData1->nDate, date1);
						__get_data(frame->StartDate, date2);
						if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
						{
							for (stFile1 = stData1->File; stFile1; stFile1 = stFile1->next)
							{
								stFile2 = stFile1;
								//对于超过1G的文件不再往后添加,跳过
								if (stFile1->nSize >= 1073741824)
									continue;

								if (stFile1->nChannelNO == frame->Channel)
								{
									//如果新的帧的起始时间与老文件的结束时间差4秒就是一个文件
									__get_data(stFile1->nEndDate, date1);
									time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
									time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
									//如果两个文件是同一通道号,同一日期,相同的起始时间,说明不对,跳过这个
									if (frame->StartDate >= stFile1->nStartDate && frame->StartDate < stFile1->nEndDate)
										goto f;

									if (time2 >= time1)
									{
										if ((time2 - time1) <= 10)
										{
											for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next);
											stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
											stStartAdd1->next->nStartAddress = frame->nStartAddress;
											stStartAdd1->next->nSize = frame->Size;

											stFile1->nSize += frame->Size;
											stFile1->nEndDate = frame->EndDate;

											goto f;
										}
									}
								}
							}
							//没有找到就新建一个
							if (stFile1 == NULL)
							{
								stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
								stFile2->next->nSize = frame->Size;
								stFile2->next->nStartDate = frame->StartDate;
								stFile2->next->nEndDate = frame->EndDate;
								stFile2->next->nChannelNO = frame->Channel;

								stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
								stFile2->next->stStAdd->nStartAddress = frame->nStartAddress;
								stFile2->next->stStAdd->nSize = frame->Size;

								goto f;
							}
						}
					}
					if (stData1 == NULL)
					{
						stData2->next = (StDate *)__malloc(sizeof(StDate));
						stData2->next->nDate = frame->StartDate;
						stData2->next->File = (StImdh *)__malloc(sizeof(StImdh));
						stData2->next->File->nSize = frame->Size;
						stData2->next->File->nStartDate = frame->StartDate;
						stData2->next->File->nEndDate = frame->EndDate;
						stData2->next->File->nChannelNO = frame->Channel;

						stData2->next->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
						stData2->next->File->stStAdd->nStartAddress = frame->nStartAddress;
						stData2->next->File->stStAdd->nSize = frame->Size;
					}
				}
			f:
				if (frame)
				{
					b1 = false;
					free(frame);
					frame = NULL;
					i -= 1;
				}
			}
			else
			{
				//判断是没有找到帧的还是已经有帧的
				if (b1)
				{
					b1 = false;
					goto ff;
				}
			}
		}
	}
	if (header)
		free(header);
	if (header)
		free(tail);
	if (header)
		free(szSearchBuff);
	if (header)
		free(frame);
	header = NULL;
	tail = NULL;
	szSearchBuff = NULL;
	frame = NULL;
	return m_stFile;
}

static int __get_filelen(char *buff16, char *buff)
{
	memset(buff16, 0, 16);
	memcpy(buff16, buff, 16);

	return strlen(buff16);
}

static unsigned __int64 __chartoint(char *szBuff, int nLen)
{
	unsigned __int64 n = 0;
	n = atoi(szBuff + 1);

	return n;
}

bool __b_dhfs(HANDLE hDisk)
{
	char buf[512];
	memset(buf, 0, 512);
	//判断是否有视频数据,如果17408都为0
	memset(buf, 0, 512);
	s_li.QuadPart = 17408;
	SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	s_bRet = ReadFile(s_f_IMG, buf, 512, &s_dwReadSize, NULL);
	if (buf[56] == 0x00 && buf[57] == 0x00 && buf[58] == 0x00 && buf[59] == 0x00 &&
		buf[72] == 0x00 && buf[73] == 0x00 && buf[74] == 0x00)
	{
		return false;
	}
	return true;
}

static void __read_file(unsigned __int64 pos, char *buf, DWORD size)
{
	s_li.QuadPart = pos;
	SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	s_bRet = ReadFile(s_f_IMG, m_Tmpbuff, 512, &s_dwReadSize, NULL);
	memcpy(buf, m_Tmpbuff, size);
}

static unsigned __int64 __init_start_pos(int *n)
{
	Stn *ns = (Stn *)__malloc(sizeof(Stn));
	int16_t	n2 = 0;
	char buff[512];
	memset(buff, 0, 512);
	s_li.QuadPart = 17408;
	SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	s_bRet = ReadFile(s_f_IMG, buff, 512, &s_dwReadSize, NULL);
	unsigned __int64 nPos = buff[56] * 4096 + buff[57] * 1048576 + buff[58] * 65536 + buff[59] * 16777216 +
		buff[72] * 1 + buff[73] * 256 + buff[74] * 65536;
	memcpy((void *)ns, buff, 74);

	if (n)
		*n = ns->n;

	free(ns);
	ns = NULL;
	return nPos * 512;
}

static void __set_file(StImdh *file, dhFrameHeader *header)
{
	file->nStartDate = header->date;
	//	file->nSize += header->size;
	file->nChannelNO = header->channel;
	file->nStartAddress = s_i * 512;
	file->next = NULL;
}

static bool __count_freespace_b(unsigned __int64 nStartAddress)
{
	if (m_szFragment[0] <= nStartAddress)
	{
		for (unsigned __int64 i = 0; i < m_nCountFragment; i++)
		{
			if (m_szFragment[i] == nStartAddress)
				return true;
		}
#if 0
		unsigned __int64 n = m_nCountFragment / 2;
		if (m_szFragment[n] > nStartAddress)
		{
			for (unsigned __int64 i = 0; i < n; i++)
			{
				if (m_szFragment[i] == nStartAddress)
					return true;
			}
		}
		else
		{
			for (unsigned __int64 i = n; i < m_nCountFragment; i++)
			{
				if (m_szFragment[i] == nStartAddress)
					return true;
			}
		}
#endif
	}
	return false;
}

static void __count_freespace(unsigned __int64 nStartAddress)
{
	m_szFragment[m_nCountFragment++] = nStartAddress / 512;
#if 0
	//n2M:2M的扇区数
	unsigned __int64 nStartAddress_sec = nStartAddress / 512, i = 0;
	for (; i < 4096; i++)
	{
		init_bitmap_set(nStartAddress_sec + i);
	}
#endif
}

static void __tranverse(dhFrameHeader *header, dhFrameTail *tail)
{
	if (header)
	{
		header->header = Tranverse32(header->header);
		header->unknown0 = Tranverse16(header->unknown0);
		header->channel = Tranverse16(header->channel);
		header->frameNO = Tranverse32(header->frameNO);
		header->size = Tranverse32(header->size);
		header->date = Tranverse32(header->date);
		header->timepass = Tranverse16(header->timepass);
	}
	if (tail)
	{
		tail->tail = Tranverse32(tail->tail);
		tail->size = Tranverse32(tail->size);
	}
}

static bool __check(char *szFrame, dhFrameHeader *header, dhFrameTail *tail, unsigned __int64 j)
{
	int n = sizeof(dhFrameTail);
	char buff[8];
	memset(buff, 0, n);

	memcpy(header, szFrame, sizeof(dhFrameHeader));
	s_li.QuadPart = 512 * s_i + j + header->size - sizeof(dhFrameTail);
	SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
	s_bRet = ReadFile(s_f_IMG, buff, n, &s_dwReadSize, NULL);

	memcpy(tail, buff, n);

	if (header->size <= sizeof(dhFrameHeader))
		return false;

	if (tail->tail != END_HEADER || header->size != tail->size)
		return false;

	return true;
}

//大华的年月日，时分秒进制
UINT32 DHDateNums[6] = {
	67108864,4194304,131072,4096,64,1
};
static void __get_data(UINT32 nDate, int8_t *date)
{
	byte innerIndex = 0;
	for (int index = 0; index < 6; index++)
	{
		UINT32 innerDateNum = nDate;
		for (innerIndex = 0; innerIndex < index; innerIndex++) {
			innerDateNum %= DHDateNums[innerIndex];
		}
		date[index] = (int8_t)(innerDateNum / DHDateNums[innerIndex]);
	}
}
#if 0
unsigned __int64 m_nAllSize = 12;
void __search(unsigned __int64 nStartAddress)
{
	unsigned __int64 n1 = m_nAllSize, n2 = m_nAllSize / 2;
	for (; ; i++)
	{
	cc:
		if (m_nSearch[n2] > nStartAddress)
		{
			if (m_nSearch[n2 - 1] < nStartAddress)
				return false;
			n1 = n2;
			n2 = n1 / 2;
			goto cc;
		}
		else if (m_nSearch[n2] < nStartAddress)
		{
			n2 = (n1 - n2) / 2 + n2;
			goto cc;
		}
		else
		{
		}
	}
}
#endif
#if 0
static void __get_data(UINT32 nDate, int8_t *date)
{
	time_t t1;
	struct tm *p;
	unsigned __int64 n = 1100000000;
	//这里要加上1100692704 + 365125988/364462556		1087722988 + 369893236
	if (nDate >= n)
	{
		if (m_nTimePos == 0)
		{
			t1 = nDate + 365125988;
			if (m_nType == 3)
				t1 = nDate + 364462556;
		}
		else
			t1 = nDate + m_nTimePos;
	}
	else
	{
		if (m_nTimePos == 0)
			t1 = nDate + 369893236;
		else
			t1 = nDate + m_nTimePos;
	}

	p = localtime(&t1);
	if (p <= 0)	//错误的时间写死一个时间
	{
		t1 = 1457616224;
		p = localtime(&t1);
	}
	char s[100];
	strftime(s, sizeof(s), "%Y-%m-%d %H:%M:%S", p);
	int year = atoi(s + 2);
	int mon = atoi(s + 5);
	int day = atoi(s + 8);
	int time = atoi(s + 11);
	int mini = atoi(s + 14);
	int seco = atoi(s + 17);

	date[0] = year;
	date[1] = mon;
	date[2] = day;
	date[3] = time;
	date[4] = mini;
	date[5] = seco;
}
#endif

#if 0
//printf(" %d-%d-%d %d:%d:%d\n",date[0],date[1],date[2],date[3],date[4],date[5]);
static void __get_data(UINT32 nDate, int8_t *date)
{
	UINT32 n = nDate;
	int x0 = n >> 28;						//第0位3
	int x1 = (n >> 24) & 0x0F;			//第1位D
	int x2 = ((n >> 16) & 0x00FF) >> 4;	//第2位9
	int x3 = (n >> 16) & 0x000F;
	int x23 = (n >> 16) & 0x00FF;
	int x4 = ((n >> 8) & 0x0000FF) >> 4;
	int x5 = (n >> 8) & 0x00000F;
	int x6 = (n >> 4) & 0x000000F;
	int x7 = n & 0x0000000F;
	int x67 = n & 0x000000FF;

	//int year = 2000 + x0 * 4 + ( x1 * 4 / 12 - 1 );
	int year = x0 * 4 + (x1 * 4 / 12 - 1);
	if ((x1 * 4 / 12) == 0)
		year = x0 * 4;

	int mon = x1 * 4 / 12 + x2 / 4;
	if (x1 % 2 == 0 && x1 != 0)
		mon = mon + 4;
	if (mon == 0)
		mon = 4;
	if (x1 == 15)
	{
		year -= 1;
		mon += 7;
	}
	//	if( mon <= 8 )
	//		mon += 4;

	int day = (x23 - x2 / 4 * 0x40) / 2;
	int time = 0;
	if (x3 % 2 != 0)
		time = x4 + x5 * 4 / 60 + 16;
	else
		time = x4 + x5 * 4 / 60;
	int mini = x5 * 4 + x6 / 4;

	int b = x6 / 4;
	int b1 = b * 0x40;
	int seco = x67 - b1;

	date[0] = year;
	date[1] = mon;
	date[2] = day;
	date[3] = time;
	date[4] = mini;
	date[5] = seco;
#if 0
	UINT32 n = nDate;
	int x0 = n >> 28;						//第0位3
	int x1 = (n >> 24) & 0x0F;			//第1位D
	int x2 = ((n >> 16) & 0x00FF) >> 4;	//第2位9
	int x3 = (n >> 16) & 0x000F;
	int x23 = (n >> 16) & 0x00FF;
	int x4 = ((n >> 8) & 0x0000FF) >> 4;
	int x5 = (n >> 8) & 0x00000F;
	int x6 = (n >> 4) & 0x000000F;
	int x7 = n & 0x0000000F;
	int x67 = n & 0x000000FF;

	//int year = 2000 + x0 * 4 + ( x1 * 4 / 12 - 1 );
	int year = x0 * 4 + (x1 * 4 / 12 - 1);
	int mon = x1 * 4 / 12 + x2 / 4;
	if (x1 % 2 == 0)
		mon = mon + 4;
	if (x1 == 15)
	{
		year -= 1;
		mon += 7;
	}

	int day = (x23 - x2 / 4 * 0x40) / 2;
	int time = 0;
	if (x3 % 2 != 0)
		time = x4 + x5 * 4 / 60 + 16;
	else
		time = x4 + x5 * 4 / 60;
	int mini = x5 * 4 + x6 / 4;

	int b = x6 / 4;
	int b1 = b * 0x40;
	int seco = x67 - b1;

	date[0] = year;
	date[1] = mon;
	date[2] = day;
	date[3] = time;
	date[4] = mini;
	date[5] = seco;
#endif
#if 0
	date[0] = (nDate << 2) >> 28;
	date[1] = (nDate << 6) >> 28;
	date[2] = (nDate << 11) >> 28;
	date[3] = (nDate << 16) >> 28;
	date[4] = (nDate << 20) >> 27;
	date[5] = (nDate << 26) >> 26;
#endif
}
#endif

static  LPCWSTR FromCharPinToLPTSTR(char *ch) {
	CString strFile = CString(ch);
	int length = strFile.GetLength();
	LPCWSTR wChFile = strFile.GetBuffer();
	strFile.ReleaseBuffer();
	return wChFile;
}

static HANDLE __dh_openflie(WCHAR *szFile)
{
	//  	wchar_t wstr[128];
	//  	memset( wstr, 128, 0 );
	//  	MultiByteToWideChar( CP_UTF8, 0, szFile, -1, wstr, 128 );
	LPCWSTR fileTSTR = szFile;
	HANDLE hFile = CreateFile(fileTSTR,
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL);
	if (hFile == INVALID_HANDLE_VALUE)
		return NULL;

	return hFile;
}

static unsigned __int64 __dh_get_file_size(HANDLE hFile)
{
	ULARGE_INTEGER f_l;
	f_l.LowPart = GetFileSize(hFile, &f_l.HighPart);
	unsigned __int64 tFileSize1 = f_l.QuadPart;

	return tFileSize1;
}

static HANDLE __dh_create_file(char *szFile)
{
	HANDLE hFile = CreateFile(FromCharPinToLPTSTR(szFile),    //创建文件的名称。
		GENERIC_WRITE | GENERIC_READ,          // 写和读文件。
		0,                      // 不共享读写。
		NULL,                  // 缺省安全属性。
		CREATE_ALWAYS,          // 如果文件存在，也创建。
		FILE_ATTRIBUTE_NORMAL, // 一般的文件。     
		NULL);                // 模板文件为空。

	if (hFile == INVALID_HANDLE_VALUE)
		return NULL;

	return hFile;
}

static void *__malloc(size_t size)
{
	void *mem = NULL;
	mem = (void *)malloc(size);
	memset(mem, 0, size);
	if (mem == NULL)
	{
		MessageBox(NULL, (LPWSTR) "内存不足!", (LPWSTR)"Error", NULL);
		exit(1);
		return NULL;
	}
	return mem;
}
#if 0
void __en_char(char *buffer)
{
	for (int i = 0; i < 144; i++)
	{
		if (buffer[i] == '0')
			buffer[i] = 'p';
		else if (buffer[i] == '1')
			buffer[i] = 'e';
		else if (buffer[i] == '2')
			buffer[i] = 't';
		else if (buffer[i] == '3')
			buffer[i] = 'f';
		else if (buffer[i] == '4')
			buffer[i] = 'e';
		else if (buffer[i] == '5')
			buffer[i] = 'q';
		else if (buffer[i] == '6')
			buffer[i] = 'u';
		else if (buffer[i] == '7')
			buffer[i] = 'b';
		else if (buffer[i] == '8')
			buffer[i] = 'v';
		else if (buffer[i] == '9')
			buffer[i] = 'z';
	}
}

void __de_char(char *buffer)
{
	for (int i = 0; i < 144; i++)
	{
		if (buffer[i] == 'p')
			buffer[i] = '0';
		else if (buffer[i] == 'e')
			buffer[i] = '1';
		else if (buffer[i] == 't')
			buffer[i] = '2';
		else if (buffer[i] == 'f')
			buffer[i] = '3';
		else if (buffer[i] == 'e')
			buffer[i] = '4';
		else if (buffer[i] == 'q')
			buffer[i] = '5';
		else if (buffer[i] == 'u')
			buffer[i] = '6';
		else if (buffer[i] == 'b')
			buffer[i] = '7';
		else if (buffer[i] == 'v')
			buffer[i] = '8';
		else if (buffer[i] == 'z')
			buffer[i] = '9';
	}
}
#endif
StDate *__recovery_searchfile_2(HANDLE hDisk, int nType)
{
	StLog *stLogo1 = (StLog *)__malloc(sizeof(StLog));
	int8_t date1[6], date2[6], date3[6];
	unsigned __int64 time1 = 0, time2 = 0, time3 = 0, i = 0, nCuSize, j3 = 0;
	dhFrameHeader *header = (dhFrameHeader *)malloc(sizeof(dhFrameHeader));
	dhFrameTail *tail = (dhFrameTail *)malloc(sizeof(dhFrameTail));
	stFrame_dh *frame = NULL;
	StImdh *stTmpFile = NULL, *stTmpFileNext = NULL;
	StImdh *stFile1 = NULL, *stFile2 = NULL;
	StDate *stData1 = NULL, *stData2 = NULL;
	StStartAdd *stStartAdd1 = NULL, *stStartAdd2 = NULL;
	s_f_IMG = hDisk;
	unsigned __int64 nBufferSize = 512 * 204800, nReadSize = 0;	//nReadSize统计100M中已经读取的大小,超过100M的处理
	char *szSearchBuff = (char *)__malloc(nBufferSize);

	bool b1 = false;
	for (s_bStop = true; s_i < (size_sec - 1) && s_bStop; s_i = (s_li.QuadPart + i) / 512 + 1)
	{
		//每次都读取100M数据
		s_li.QuadPart = s_i * 512;
		SetFilePointer(s_f_IMG, s_li.LowPart, &s_li.HighPart, FILE_BEGIN);
		s_bRet = ReadFile(s_f_IMG, szSearchBuff, nBufferSize, &s_dwReadSize, NULL);
		if (nBufferSize != s_dwReadSize)
			nBufferSize = s_dwReadSize;
		if (s_dwReadSize <= 0)
			break;
		//开始解析这100M数据
		for (i = 0; i < (nBufferSize - 4); i++)
		{
			s_i = (s_li.QuadPart + i) / 512;
			j3 = (s_li.QuadPart + i) % 512;
			if (j3 == 0 && m_bFS_Free)
			{
				if (__count_freespace_b(s_i))
				{
					i = i + 2097151;	//如果这个扇区有被占用，那么跳过2MB
					continue;
				}
			}

			//找到帧的标识位44 48 41 56
			if (szSearchBuff[0 + i] == 0x44 && szSearchBuff[1 + i] == 0x48 && szSearchBuff[2 + i] == 0x41 && szSearchBuff[3 + i] == 0x56 && szSearchBuff[4 + i] == 0xFFFFFFFD)
			{
				if (i > nBufferSize)		//处理最后一个帧在100M以外
				{
					i = i - frame->Size;	//把大小还原到之前没越界前
					s_i = (s_li.QuadPart + i) / 512;
					break;
				}

				memcpy(header, szSearchBuff + i, sizeof(dhFrameHeader));		//判断帧的合法性
				if (header->size > sizeof(dhFrameHeader))
				{
					if ((i + header->size) > nBufferSize)		//检测在尾部检查的时候是否尾部越界
					{
						s_i = (s_li.QuadPart + i) / 512;
						break;
					}
					//验证帧的尾部是否是这个帧的
					memcpy(tail, szSearchBuff + i + header->size - sizeof(dhFrameTail), sizeof(dhFrameTail));
					if (tail->tail == END_HEADER && header->size == tail->size)
					{
						if (frame == NULL)		//到这一步说明帧是合法帧,组碎片
						{
							b1 = true;
							frame = (stFrame_dh *)__malloc(sizeof(stFrame_dh));
							frame->StartDate = frame->EndDate = header->date;
							frame->Channel = header->channel;
							frame->Size = header->size;
							//frame->nStartAddress = ( s_i * 512 + i ) / 512 * 512;
							frame->nStartAddress = s_i * 512;
							i += header->size - 1;		//-1是因为循环那里还会加1所以先减掉这个1
							continue;
						}
						else
						{
							if (frame->Channel == header->channel)
							{
								__get_data(frame->EndDate, date1);
								__get_data(header->date, date2);
								time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
								time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
								if (time2 >= time1)
								{
									if ((time2 - time1) <= 10)
									{
										b1 = true;
										frame->EndDate = header->date;
										frame->Size += header->size;
										i += header->size - 1;
										continue;
									}
								}
							}
						}
					}
					else
						continue;
				}
				else
					continue;
			ff:
				//组文件
				if (!m_stFile)
				{
					m_stFile = (StDate *)__malloc(sizeof(StDate));
					m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
					m_stFile->nDate = frame->StartDate;
					m_stFile->File->nSize = frame->Size;
					m_stFile->File->nChannelNO = frame->Channel;
					m_stFile->File->nStartDate = frame->StartDate;
					m_stFile->File->nEndDate = frame->EndDate;

					m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
					m_stFile->File->stStAdd->nStartAddress = frame->nStartAddress;
					m_stFile->File->stStAdd->nSize = frame->Size;
				}
				else
				{
					for (stData1 = m_stFile; stData1; stData1 = stData1->next)
					{
						stData2 = stData1;
						//日期判断，定位到某个日期列表中
						__get_data(stData1->nDate, date1);
						__get_data(frame->StartDate, date2);
						if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
						{
							for (stFile1 = stData1->File; stFile1; stFile1 = stFile1->next)
							{
								stFile2 = stFile1;
								//对于超过1G的文件不再往后添加,跳过
								if (stFile1->nSize >= 1073741824)
									continue;

								if (stFile1->nChannelNO == frame->Channel)
								{
									//如果新的帧的起始时间与老文件的结束时间差4秒就是一个文件
									__get_data(stFile1->nEndDate, date1);
									time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
									time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];
									//如果两个文件是同一通道号,同一日期,相同的起始时间,说明不对,跳过这个
									if (frame->StartDate >= stFile1->nStartDate && frame->StartDate < stFile1->nEndDate)
										goto f;

									if (time2 >= time1)
									{
										if ((time2 - time1) <= 10)
										{
											for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next);
											stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
											stStartAdd1->next->nStartAddress = frame->nStartAddress;
											stStartAdd1->next->nSize = frame->Size;

											stFile1->nSize += frame->Size;
											stFile1->nEndDate = frame->EndDate;
											goto f;
										}
									}
								}
							}
							//没有找到就新建一个
							if (stFile1 == NULL)
							{
								stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
								stFile2->next->nSize = frame->Size;
								stFile2->next->nStartDate = frame->StartDate;
								stFile2->next->nEndDate = frame->EndDate;
								stFile2->next->nChannelNO = frame->Channel;

								stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
								stFile2->next->stStAdd->nStartAddress = frame->nStartAddress;
								stFile2->next->stStAdd->nSize = frame->Size;
								goto f;
							}
						}
					}
					if (stData1 == NULL)
					{
						stData2->next = (StDate *)__malloc(sizeof(StDate));
						stData2->next->nDate = frame->StartDate;
						stData2->next->File = (StImdh *)__malloc(sizeof(StImdh));
						stData2->next->File->nSize = frame->Size;
						stData2->next->File->nStartDate = frame->StartDate;
						stData2->next->File->nEndDate = frame->EndDate;
						stData2->next->File->nChannelNO = frame->Channel;

						stData2->next->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
						stData2->next->File->stStAdd->nStartAddress = frame->nStartAddress;
						stData2->next->File->stStAdd->nSize = frame->Size;
					}
				}
			f:
				if (frame)
				{
					b1 = false;
					free(frame);
					frame = NULL;
					i -= 1;
				}
			}
			else
			{
				//判断是没有找到帧的还是已经有帧的
				if (b1)
				{
					b1 = false;
					goto ff;
				}
			}
		}
	}
	if (header)
		free(header);
	if (header)
		free(tail);
	if (header)
		free(szSearchBuff);
	if (header)
		free(frame);
	header = NULL;
	tail = NULL;
	szSearchBuff = NULL;
	frame = NULL;
	return m_stFile;
}



#if 0
ff:
  //组文件
  if (!m_stFile)
  {
	  m_stFile = (StDate *)__malloc(sizeof(StDate));
	  m_stFile->File = (StImdh *)__malloc(sizeof(StImdh));
	  m_stFile->nDate = frame->StartDate;
	  m_stFile->File->nSize = frame->Size;
	  m_stFile->File->nChannelNO = frame->Channel;
	  m_stFile->File->nStartDate = frame->StartDate;
	  m_stFile->File->nEndDate = frame->EndDate;
	  m_stFile->File->nFrameNO = frame->FrameNOEnd;

	  m_stFile->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
	  m_stFile->File->stStAdd->nStartAddress = frame->nStartAddress;
	  m_stFile->File->stStAdd->nSize = frame->Size;
	  m_stFile->File->stStAdd->nStartDate = frame->StartDate;
	  m_stFile->File->stStAdd->nEndDate = frame->EndDate;
	  m_stFile->File->stStAdd->nChannelNO = frame->Channel;
	  nFCount++;
  }
  else
  {
	  for (stData1 = m_stFile; stData1; stData1 = stData1->next)
	  {
		  stData2 = stData1;
		  //日期判断，定位到某个日期列表中
		  __get_data(stData1->nDate, date1);
		  __get_data(frame->StartDate, date2);
		  if (date1[0] == date2[0] && date1[1] == date2[1] && date1[2] == date2[2])
		  {
			  for (stFile1 = stData1->File; stFile1; stFile1 = stFile1->next)
			  {
				  stFile2 = stFile1;
				  //对于超过1G的文件不再往后添加,跳过
				  if (stFile1->nSize >= 1073741824)
					  continue;

				  if (stFile1->nChannelNO == frame->Channel)
				  {
					  //如果新的帧的起始时间与老文件的结束时间差4秒就是一个文件
					  __get_data(stFile1->nEndDate, date1);
					  time1 = date1[3] * 60 * 60 + date1[4] * 60 + date1[5];
					  time2 = date2[3] * 60 * 60 + date2[4] * 60 + date2[5];

					  int nt = 0;
					  if (time2 >= time1)
						  nt = time2 - time1;
					  else
						  nt = time1 - time2;

					  //if( time2 >= time1 )
					  {
						  if (nt <= 10)
						  {
							  if (frame->FrameNOStart >= stFile1->nFrameNO)
							  {
								  int nnnn = frame->FrameNOStart - stFile1->nFrameNO;
								  if ((nnnn <= (nFrameNumCount + 5) && nnnn >= (nFrameNumCount)) || nnnn == 0 || nnnn == 1 || nnnn == 2)	//帧的第一个号与文件的最后一个帧
								  {
									  for (stStartAdd1 = stFile1->stStAdd; stStartAdd1->next; stStartAdd1 = stStartAdd1->next);
									  stStartAdd1->next = (StStartAdd *)__malloc(sizeof(StStartAdd));
									  stStartAdd1->next->nStartAddress = frame->nStartAddress;
									  stStartAdd1->next->nSize = frame->Size;
									  stStartAdd1->next->nStartDate = frame->StartDate;
									  stStartAdd1->next->nEndDate = frame->EndDate;
									  stStartAdd1->next->nChannelNO = frame->Channel;

									  stFile1->nSize += frame->Size;
									  stFile1->nEndDate = frame->EndDate;
									  stFile1->nFrameNO = frame->FrameNOEnd;

									  goto f;
								  }
							  }
						  }
					  }
				  }
			  }
			  //没有找到就新建一个
			  if (stFile1 == NULL)
			  {
				  stFile2->next = (StImdh *)__malloc(sizeof(StImdh));
				  stFile2->next->nSize = frame->Size;
				  stFile2->next->nStartDate = frame->StartDate;
				  stFile2->next->nEndDate = frame->EndDate;
				  stFile2->next->nChannelNO = frame->Channel;
				  stFile2->next->nFrameNO = frame->FrameNOEnd;

				  stFile2->next->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
				  stFile2->next->stStAdd->nStartAddress = frame->nStartAddress;
				  stFile2->next->stStAdd->nSize = frame->Size;
				  stFile2->next->stStAdd->nStartDate = frame->StartDate;
				  stFile2->next->stStAdd->nEndDate = frame->EndDate;
				  stFile2->next->stStAdd->nChannelNO = frame->Channel;
				  nFCount++;
				  goto f;
			  }
		  }
	  }
	  if (stData1 == NULL)
	  {
		  stData2->next = (StDate *)__malloc(sizeof(StDate));
		  stData2->next->nDate = frame->StartDate;
		  stData2->next->File = (StImdh *)__malloc(sizeof(StImdh));
		  stData2->next->File->nSize = frame->Size;
		  stData2->next->File->nStartDate = frame->StartDate;
		  stData2->next->File->nEndDate = frame->EndDate;
		  stData2->next->File->nChannelNO = frame->Channel;
		  stData2->next->File->nFrameNO = frame->FrameNOEnd;

		  stData2->next->File->stStAdd = (StStartAdd *)__malloc(sizeof(StStartAdd));
		  stData2->next->File->stStAdd->nStartAddress = frame->nStartAddress;
		  stData2->next->File->stStAdd->nSize = frame->Size;
		  stData2->next->File->nStartDate = frame->StartDate;
		  stData2->next->File->nEndDate = frame->EndDate;
		  stData2->next->File->nChannelNO = frame->Channel;
		  nFCount++;
	  }
  }
f:
  //nFrameNumCount = 0;
  if (frame)
  {
	  b1 = false;
	  free(frame);
	  frame = NULL;
	  i -= 1;
  }
#endif