#include "Common.h"
//template <class T>
//T findFirst(T node, T(*next) (T), bool(*func)(T))
 
using namespace std;


void* __cdecl Mmalloc(
	_In_ _CRT_GUARDOVERFLOW size_t _Size
) {
	return malloc(_Size);
}


void* __cdecl Mmemset(
	_Out_writes_bytes_all_(_Size) void*  _Dst,
	_In_                          int    _Val,
	_In_                          size_t _Size
) {
	return memset(_Dst, _Val, _Size);
}
void* __cdecl Mmemcpy(
	_Out_writes_bytes_all_(_Size) void* _Dst,
	_In_reads_bytes_(_Size)       void const* _Src,
	_In_                          size_t      _Size
) {
	return memcpy(
		_Out_writes_bytes_all_(_Size) _Dst,
		_In_reads_bytes_(_Size)       _Src,
		_In_                          _Size
	);
}


int Mmemcmp(const byte* sZbuffer1,const byte* sZbuffer2, int nComSize) {
	//return memcmp(sZbuffer1, sZbuffer2, nComSize);
	auto index = 0;
	
	while (index < nComSize) {
		if (*(sZbuffer1 + index) != *(sZbuffer2 + index)) {
			return 1;
		}
		index++;
	}
	return 0;
}

//找到第一个内存中不匹配的字节;
byte findFirstNotIn(const byte *mem, int len) {
	byte unMatched = 0xff;
	//遍历所有字节,找到不在Header中的字节;
	for (size_t j = 0; j < 0xff; j++)
	{
		//是否找到相等的结果;
		auto findMatch = false;
		for (size_t pinIndex = 0; pinIndex < len; pinIndex++)
		{
			if (mem[pinIndex] == j) {
				findMatch = true;
				break;
			}
		}

		if (!findMatch) {
			unMatched = j;
			break;
		}
	}
	return unMatched;
}