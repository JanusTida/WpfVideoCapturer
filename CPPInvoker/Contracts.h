#pragma once

//数据交互开发规范;
//以下所有规范相关的代码将会定义在一个单独的项目中,以便多个项目引用;

//数据源规范(RAW,VHD等);
__interface Stream {
	//获取长度
	long long GetLength();
	//获取位置;
	long long GetPosition();
	//设定位置;(Seek);
	void SetPosition(long long position);

	bool CanRead();

	//读取;
	//参数lpBuffer:缓冲区;
	//参数nNumberOfBytesToRead:读取大小;
	//返回:实际读取大小;
	unsigned int Read(void * lpBuffer,
		unsigned int nNumberOfBytesToRead);

	//是否可写;
	bool CanWrite();

	//写入数据;
	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite);

	//关闭流;
	void Close() {

	}
};

static int _error = 0;
extern "C" _declspec(dllexport) int get_last_error() {
	return _error;
}

extern "C" _declspec(dllexport) void set_last_error(int errCode) {
	_error = errCode;
}
//
//////分区设备规范;
////__interface IDevice{
////
////}
//
//
////缓冲区规范;
////struct StBuffer {
////	//缓冲区;
////	void * lpBuffer;
////	//缓冲区长度;
////	int nLength;
////};
//
////文件规范;
////__interface IFile {
////	StBuffer* GetName();
////	//大小;
////	long long GetSize();
////	//获取流;
////	Stream *GetStream();
////};
//
//
////__interface IFileSystemParser {
////
////};
//
//
////文件系统解析提供者;
////__interface IFileSystemParserProvider {
////	//是否能够解析;
////	bool CanParse(Stream* stream);
////
////	
////};
//
//
////原则:只有一项(比如Dos的EFIInfo)的数据无需放到链表中,不同的文件系统/分区表尽量分开处理,不在同一种结构中容纳多种不同系统的特性,设计不冗余;
////分区表规范;
//
////1.Dos相关;
//typedef struct StEFIInfo
//{
//	unsigned __int64 EFIPART;						//EFI签名(EFI PART)		8 byte
//	UINT32 Version;									//版本					4 byte
//	UINT32 EFISize;									//EFI信息大小字节数		4 byte
//	UINT32 CRC;										//EFI信息 CRC校验和		4 byte
//	UINT32 Unknown;									//保留					4 byte
//	unsigned __int64 EFICurrSecNum;					//当前EFI LBA扇区号		8 byte
//	unsigned __int64 EFIBackupSecNum;				//备份EFI LBA扇区号		8 byte
//	unsigned __int64 GPTStartLBA;					//GPT分区区域起始LBA	8 byte
//	unsigned __int64 GPTEndLBA;						//GPT分区区域结束LBA	8 byte
//	UCHAR DiskGUID[16];								//磁盘GUID				16 byte
//	unsigned __int64 GPTPartTabStartLBA;			//备份EFI LBA扇区号		8 byte
//	UINT32 PartTabCount;							//分区表项数			4 byte
//	UINT32 PartTabCRC;								//分区表CRC校验和		4 byte
//};
//
//#pragma pack(1)
//typedef struct StEFIPTable
//{
//	UCHAR PartTabType[16];					//分区类型GUID		16 byte
//	UCHAR PartTabOnly[16];					//分区唯一GUID		16 byte
//	unsigned __int64 PartTabStartLBA;		//分区起始LAB		8 byte
//	unsigned __int64 PartTabEndLBA;			//分区结束LAB		8 byte
//	unsigned __int64 PartTabProp;			//分区属性			8 byte
//	UCHAR PartTabNameUnicode[72];			//分区名unicode码	72 byte
//};
//#pragma pack()
//
//typedef	enum ePTableType
//{
//	e_nknown,	//未知类型
//	e_gpt,
//	e_dos,
//	e_apple,
//
//
//	e_fat,
//	e_ntfs
//};
//
//#pragma pack(1)
//typedef struct InFoDisk
//{
//	UCHAR     BootID;					//80h表示可启动分区，否则为0；对主分区有用；
//	UCHAR     SartHead;				//分区的起始磁头号； 
//	USHORT    SartSectorTrack;			//分区的起始扇区和磁道号
//	UCHAR     FileSystemID;				//05H或0FH为扩展分区，06H或0EH为FAT16，0BH或0CH为FAT32 ,07为NTFS；
//	UCHAR     EndHead;					//分区结束磁头号；
//	USHORT    EndSectorTrack;			//分区结束扇区和磁道号
//	ULONG     HeadSecor;				//分区前的扇区； 
//	ULONG     AllSector;				//分区的总扇区； 
//}InFoDisk, *PInFoDisk;
//#pragma pack()
//
//#pragma pack(1)
//typedef struct StPTable
//{
//	//是否为拓展分区;
//	//bool IsExtended;
//	ePTableType eType;
//
//	unsigned __int64 nOffset;
//	InFoDisk *InFo;
//	//唯一项的应拿到向外一级;
//	//StEFIInfo *EFIInfo;
//	StEFIPTable *EFIPTable;
//};
//#pragma pack()
//
////
//struct StDosPtInfo {
//	unsigned __int64 nOffset;
//	InFoDisk *InFo;
//	StDosPtInfo next;
//};
//
//
////Dos管理者实例;
////本结构无需被上层使用,外部只使用本结构的指针(void *);
//typedef struct StDosManager {
//	//Dos信息;
//	StDosPtInfo *StDosInfo;
//
//	/*
//	此处保存一些私有的实例相关的状态信息，以便管理,释放等
//	例如
//	Stream* Stream;
//	
//	*/
//
//	//释放接口;
//	//Stream不应在这个方法内部释放;
//	void Exit() {
//
//	}
//};
//
////是否是dos分区表设备;
//bool device_tab_dos_available(Stream* stream);
//
//StDosManager *DosManager_Get(Stream* stream);//////获得Dos信息;//StDosPtInfo *GetDosInfo(StDosManager* manager) {//	return manager->StDosInfo;//}//////退出;//void DosManager_Exit(StDosManager* stDosManager) {
//	stDosManager->Exit();
//}
//
////2.Gpt相关;
////是否是gpt分区表设备;
//bool device_tab_gpt_available(Stream* stream) {
//
//}
//
////如法炮制;
//StGPTManager* GPTManager_Get(Stream *stream);
//
//void StGptManager_Exit(StGptManager* manager);
////....
//
//
//
//
////文件系统规范;
//
////1.ext4相关;
//struct StNtfsFile {
//
//};
//struct StNtfsDirect {
//
//};
//
////ext4信息;
//struct StExt4Info {
//	StNtfsDirect* rootDirect;
//	Stream* stream;
//};
//
////ext4管理器单元;
//struct STExt4Manager {
//	StExt4Info *info;
//	//...
//};
//
////是否是Ext4分区;;
//bool part_ext4_avaiable(Stream* stream);
//
//STExt4Manager* Ext4Manager_Get(Stream * stream);
//
//StExt4Info* Ext4Manager_GetInfo(STExt4Manager* manager);
//
////解析文件项;
//
////由于所有方法都加入了一个实例参数,设定实例将无需使用;
////void Cflabqd_Partition_Init(IntPtr ST_PartTabInfo, SafeFileHandle handle);
//
//void* Cflabqd_Get_InodeInfo(STExt4Manager* manager,UINT N_Inode);
//
//void* Cflabqd_Get_BlockList(STExt4Manager* manager,void* ST_Ext4Inode);
//
//void* Cflabqd_Parse_Dir(STExt4Manager* manager, void* stBlockList);
//
//void* Cflabqd_InodeInfo_Free(STExt4Manager* manager, void* ST_Ext4Inode);
//
//void* Cflabqd_BlockList_Free(STExt4Manager* manager, void* ST_BlockList);
//
//void* Cflabqd_Dir_Free(STExt4Manager* manager, void* ST_DirDntry);