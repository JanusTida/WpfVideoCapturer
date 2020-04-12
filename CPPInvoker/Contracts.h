#pragma once

//���ݽ��������淶;
//�������й淶��صĴ��뽫�ᶨ����һ����������Ŀ��,�Ա�����Ŀ����;

//����Դ�淶(RAW,VHD��);
__interface Stream {
	//��ȡ����
	long long GetLength();
	//��ȡλ��;
	long long GetPosition();
	//�趨λ��;(Seek);
	void SetPosition(long long position);

	bool CanRead();

	//��ȡ;
	//����lpBuffer:������;
	//����nNumberOfBytesToRead:��ȡ��С;
	//����:ʵ�ʶ�ȡ��С;
	unsigned int Read(void * lpBuffer,
		unsigned int nNumberOfBytesToRead);

	//�Ƿ��д;
	bool CanWrite();

	//д������;
	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite);

	//�ر���;
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
//////�����豸�淶;
////__interface IDevice{
////
////}
//
//
////�������淶;
////struct StBuffer {
////	//������;
////	void * lpBuffer;
////	//����������;
////	int nLength;
////};
//
////�ļ��淶;
////__interface IFile {
////	StBuffer* GetName();
////	//��С;
////	long long GetSize();
////	//��ȡ��;
////	Stream *GetStream();
////};
//
//
////__interface IFileSystemParser {
////
////};
//
//
////�ļ�ϵͳ�����ṩ��;
////__interface IFileSystemParserProvider {
////	//�Ƿ��ܹ�����;
////	bool CanParse(Stream* stream);
////
////	
////};
//
//
////ԭ��:ֻ��һ��(����Dos��EFIInfo)����������ŵ�������,��ͬ���ļ�ϵͳ/���������ֿ�����,����ͬһ�ֽṹ�����ɶ��ֲ�ͬϵͳ������,��Ʋ�����;
////������淶;
//
////1.Dos���;
//typedef struct StEFIInfo
//{
//	unsigned __int64 EFIPART;						//EFIǩ��(EFI PART)		8 byte
//	UINT32 Version;									//�汾					4 byte
//	UINT32 EFISize;									//EFI��Ϣ��С�ֽ���		4 byte
//	UINT32 CRC;										//EFI��Ϣ CRCУ���		4 byte
//	UINT32 Unknown;									//����					4 byte
//	unsigned __int64 EFICurrSecNum;					//��ǰEFI LBA������		8 byte
//	unsigned __int64 EFIBackupSecNum;				//����EFI LBA������		8 byte
//	unsigned __int64 GPTStartLBA;					//GPT����������ʼLBA	8 byte
//	unsigned __int64 GPTEndLBA;						//GPT�����������LBA	8 byte
//	UCHAR DiskGUID[16];								//����GUID				16 byte
//	unsigned __int64 GPTPartTabStartLBA;			//����EFI LBA������		8 byte
//	UINT32 PartTabCount;							//����������			4 byte
//	UINT32 PartTabCRC;								//������CRCУ���		4 byte
//};
//
//#pragma pack(1)
//typedef struct StEFIPTable
//{
//	UCHAR PartTabType[16];					//��������GUID		16 byte
//	UCHAR PartTabOnly[16];					//����ΨһGUID		16 byte
//	unsigned __int64 PartTabStartLBA;		//������ʼLAB		8 byte
//	unsigned __int64 PartTabEndLBA;			//��������LAB		8 byte
//	unsigned __int64 PartTabProp;			//��������			8 byte
//	UCHAR PartTabNameUnicode[72];			//������unicode��	72 byte
//};
//#pragma pack()
//
//typedef	enum ePTableType
//{
//	e_nknown,	//δ֪����
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
//	UCHAR     BootID;					//80h��ʾ����������������Ϊ0�������������ã�
//	UCHAR     SartHead;				//��������ʼ��ͷ�ţ� 
//	USHORT    SartSectorTrack;			//��������ʼ�����ʹŵ���
//	UCHAR     FileSystemID;				//05H��0FHΪ��չ������06H��0EHΪFAT16��0BH��0CHΪFAT32 ,07ΪNTFS��
//	UCHAR     EndHead;					//����������ͷ�ţ�
//	USHORT    EndSectorTrack;			//�������������ʹŵ���
//	ULONG     HeadSecor;				//����ǰ�������� 
//	ULONG     AllSector;				//�������������� 
//}InFoDisk, *PInFoDisk;
//#pragma pack()
//
//#pragma pack(1)
//typedef struct StPTable
//{
//	//�Ƿ�Ϊ��չ����;
//	//bool IsExtended;
//	ePTableType eType;
//
//	unsigned __int64 nOffset;
//	InFoDisk *InFo;
//	//Ψһ���Ӧ�õ�����һ��;
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
////Dos������ʵ��;
////���ṹ���豻�ϲ�ʹ��,�ⲿֻʹ�ñ��ṹ��ָ��(void *);
//typedef struct StDosManager {
//	//Dos��Ϣ;
//	StDosPtInfo *StDosInfo;
//
//	/*
//	�˴�����һЩ˽�е�ʵ����ص�״̬��Ϣ���Ա����,�ͷŵ�
//	����
//	Stream* Stream;
//	
//	*/
//
//	//�ͷŽӿ�;
//	//Stream��Ӧ����������ڲ��ͷ�;
//	void Exit() {
//
//	}
//};
//
////�Ƿ���dos�������豸;
//bool device_tab_dos_available(Stream* stream);
//
//StDosManager *DosManager_Get(Stream* stream);//////���Dos��Ϣ;//StDosPtInfo *GetDosInfo(StDosManager* manager) {//	return manager->StDosInfo;//}//////�˳�;//void DosManager_Exit(StDosManager* stDosManager) {
//	stDosManager->Exit();
//}
//
////2.Gpt���;
////�Ƿ���gpt�������豸;
//bool device_tab_gpt_available(Stream* stream) {
//
//}
//
////�編����;
//StGPTManager* GPTManager_Get(Stream *stream);
//
//void StGptManager_Exit(StGptManager* manager);
////....
//
//
//
//
////�ļ�ϵͳ�淶;
//
////1.ext4���;
//struct StNtfsFile {
//
//};
//struct StNtfsDirect {
//
//};
//
////ext4��Ϣ;
//struct StExt4Info {
//	StNtfsDirect* rootDirect;
//	Stream* stream;
//};
//
////ext4��������Ԫ;
//struct STExt4Manager {
//	StExt4Info *info;
//	//...
//};
//
////�Ƿ���Ext4����;;
//bool part_ext4_avaiable(Stream* stream);
//
//STExt4Manager* Ext4Manager_Get(Stream * stream);
//
//StExt4Info* Ext4Manager_GetInfo(STExt4Manager* manager);
//
////�����ļ���;
//
////�������з�����������һ��ʵ������,�趨ʵ��������ʹ��;
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