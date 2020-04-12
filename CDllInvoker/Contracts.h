#include<Windows.h>

//数据交互开发规范;
//以下所有规范相关的代码将会定义在一个单独的项目中,以便多个项目引用;

//数据源规范(RAW,VHD等);
__interface IStream {
	//获取长度
	long long GetLength();
	//获取位置;
	long long GetPosition();
	//设定位置;(Seek);
	void SetPosition(long long position);

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
};

//Demo,镜像原始流
struct ImgRawStream:IStream{
	ImgRawStream(HANDLE handle,bool canWrite) {
		this->_canWrite = canWrite;
	}

	//获取长度
	long long GetLength() {
		//GETFIleSize()...
		return 0;
	}

	//获取位置;
	long long GetPosition() {
		//Seek.
		return 0;
	}

	//设定位置;(Seek);
	void SetPosition(long long position) {
		//SetFilePointer

	}

	//是否可读;
	bool CanRead() {
		return true;
	}

	//读取;
	//参数lpBuffer:缓冲区;
	//参数nNumberOfBytesToRead:读取大小;
	//返回:实际读取大小;
	unsigned int Read(void * lpBuffer,
		unsigned int nNumberOfBytesToRead) {
		//ReadFile
		return 0;
	}

private:
	bool _canWrite;

public:
	//是否可写;
	bool CanWrite() {
		return _canWrite;
	}

	//写入数据;
	void Write(void* lpBuffer, unsigned int nNumberOfBytesToWrite) {

	}

};

static ImgRawStream* CrateImgRawStream(HANDLE handle,bool canWrite) {
	return new ImgRawStream(handle,canWrite);
}

struct PhysicalDeviceStream:IStream {

};

static int _error = 0;
static int GetLastError() {
	return _error;
}

static void SetLastError(int errCode) {
	_error = errCode;
}

////分区设备规范;
//__interface IDevice{
//
//}

//缓冲区规范;
struct StBuffer {
	//缓冲区;
	void * lpBuffer;
	//缓冲区长度;
	int nLength;
};

//文件规范;
struct File {
	StBuffer *Name;
	//大小;
	long long Size;
};


//文件系统解析提供者;
__interface IFileSystemParserProvider {
	//是否能够解析;
	bool CanParse(IStream* stream);

};

//文件系统规范;
//__interface IFileStream {
//	void
//}

