//#pragma once
//#include<Windows.h>
//#include "Contracts.h"
//
////Demo,����ԭʼ��;
//struct ImageRawStream :public Stream {
//	ImageRawStream(LPCWSTR path,int fileAccess,int fileShare) {
//		//this->_canWrite = canWrite;
//	}
//
//	//��ȡ����
//	long long GetLength() {
//		//GETFIleSize()...
//		return 0;
//	}
//
//	//��ȡλ��;
//	long long GetPosition() {
//		//Seek.
//		return 0;
//	}
//
//	//�趨λ��;(Seek);
//	void SetPosition(long long position) {
//		//SetFilePointer
//
//	}
//
//	//�Ƿ�ɶ�;
//	bool CanRead() {
//		return true;
//	}
//
//	//��ȡ;
//	//����lpBuffer:������;
//	//����nNumberOfBytesToRead:��ȡ��С;
//	//����:ʵ�ʶ�ȡ��С;
//	unsigned int Read(void * lpBuffer,
//		unsigned int nNumberOfBytesToRead) {
//		//ReadFile
//		return 0;
//	}
//
//private:
//	bool _canWrite;
//
//public:
//	//�Ƿ��д;
//	bool CanWrite() {
//		return _canWrite;
//	}
//
//	//д������;
//	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite) {
//
//	}
//
//	void Close() {
//
//	}
//};
//
//struct OnePeridStream:Stream {
//public:
//	OnePeridStream(Stream* stream)
//	{
//
//	}
//			
//};
//
//
//
//extern "C" _declspec(dllexport) ImageRawStream * create_raw_img_stream(LPCWSTR buffer, bool canWrite) {
//	return new ImageRawStream(buffer, canWrite);
//}
//
//
//
///// <summary>
///// �����豸�Ľṹ;
///// </summary>
//struct PhysicsDeviceStruct {
//	int ObjectID;                     //�豸����(���Ϊ16�����������ƴ�)
//	//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
//	byte Lable[64];
//	//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
//	byte DevName[20];                //��������
//	ULONG DevSize;                 //�豸��С
//	//CHSStruct DevCHS;                    //�豸����
//	//uint DevMomd;                    //����ģʽ
//	//uint DevType;                 //�豸����
//	//IntPtr Handle;
//	//int SectorSize;              //�����ֽ�
//	//IntPtr Buffer;                   //��д����
//	//IntPtr Partiton;                  //�����ṹ��������
//	//IntPtr Arch;                 //����ָ��
//	//IntPtr DevRW;                    //�豸��д
//	//bool DevState;                    //�Ƿ�ʹ��
//											 //public IntPtr Handle;
//};
//
//
//
////�����豸��;
//struct PhysicalDeviceStream : Stream {
//	//����ʱ��ʹ��һ���豸�ṹ�Ի�ȡ��Ϣ;
//	PhysicalDeviceStream(PhysicsDeviceStruct* phyStruct,bool canWrite)
//	{
//		
//	}
//
//	//��ȡ����
//	long long GetLength();
//	//��ȡλ��;
//	long long GetPosition();
//
//	//�趨λ��;(Seek);
//	void SetPosition(long long position);
//
//	//��ȡ;
//	//����lpBuffer:������;
//	//����nNumberOfBytesToRead:��ȡ��С;
//	//����:ʵ�ʶ�ȡ��С;
//	unsigned int Read(void * lpBuffer,
//		unsigned int nNumberOfBytesToRead);
//
//	//�Ƿ��д;
//	bool CanWrite();
//
//	//д������;
//	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite);
//
//	//�ر���;
//	void Close() {
//
//	}
//
//};
//
////extern "C" PhysicalDeviceStream* 
//
//struct PartitionStream : Stream {
//
//};
//
//
