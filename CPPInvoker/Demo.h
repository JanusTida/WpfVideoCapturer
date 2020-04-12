//#pragma once
//#include<Windows.h>
//#include "Contracts.h"
//
////Demo,镜像原始流;
//struct ImageRawStream :public Stream {
//	ImageRawStream(LPCWSTR path,int fileAccess,int fileShare) {
//		//this->_canWrite = canWrite;
//	}
//
//	//获取长度
//	long long GetLength() {
//		//GETFIleSize()...
//		return 0;
//	}
//
//	//获取位置;
//	long long GetPosition() {
//		//Seek.
//		return 0;
//	}
//
//	//设定位置;(Seek);
//	void SetPosition(long long position) {
//		//SetFilePointer
//
//	}
//
//	//是否可读;
//	bool CanRead() {
//		return true;
//	}
//
//	//读取;
//	//参数lpBuffer:缓冲区;
//	//参数nNumberOfBytesToRead:读取大小;
//	//返回:实际读取大小;
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
//	//是否可写;
//	bool CanWrite() {
//		return _canWrite;
//	}
//
//	//写入数据;
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
///// 物理设备的结构;
///// </summary>
//struct PhysicsDeviceStruct {
//	int ObjectID;                     //设备标数(如果为16不以物理名称打开)
//	//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
//	byte Lable[64];
//	//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
//	byte DevName[20];                //驱动名称
//	ULONG DevSize;                 //设备大小
//	//CHSStruct DevCHS;                    //设备几何
//	//uint DevMomd;                    //访问模式
//	//uint DevType;                 //设备类型
//	//IntPtr Handle;
//	//int SectorSize;              //扇区字节
//	//IntPtr Buffer;                   //读写缓存
//	//IntPtr Partiton;                  //分区结构（废弃）
//	//IntPtr Arch;                 //调用指针
//	//IntPtr DevRW;                    //设备读写
//	//bool DevState;                    //是否使用
//											 //public IntPtr Handle;
//};
//
//
//
////本地设备流;
//struct PhysicalDeviceStream : Stream {
//	//构造时需使用一个设备结构以获取信息;
//	PhysicalDeviceStream(PhysicsDeviceStruct* phyStruct,bool canWrite)
//	{
//		
//	}
//
//	//获取长度
//	long long GetLength();
//	//获取位置;
//	long long GetPosition();
//
//	//设定位置;(Seek);
//	void SetPosition(long long position);
//
//	//读取;
//	//参数lpBuffer:缓冲区;
//	//参数nNumberOfBytesToRead:读取大小;
//	//返回:实际读取大小;
//	unsigned int Read(void * lpBuffer,
//		unsigned int nNumberOfBytesToRead);
//
//	//是否可写;
//	bool CanWrite();
//
//	//写入数据;
//	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite);
//
//	//关闭流;
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
