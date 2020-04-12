#pragma once
#include <Windows.h>
#include <vector>
#include <functional>
using namespace std;
template <class T>
UINT getArrayLen(T& array);
//从某个有序的泛型vector中寻找针对val的一个范围;
//template <class T>
//UINT64 calcMaxValue(T x, T y);
//UINT64(*func) (T),
template <class T>
UINT getArrayLen(T& array)
{
	return (sizeof(array) / sizeof(array[0]));
}
//从某个有序的泛型vector中二分寻找针对val的一个范围;
//template <class T>
//UINT64 calcMaxValue(T x, T y);
//UINT64(*func) (T),
template <class T>
int findIndex(vector<T> vec, UINT64(*func) (T),
	UINT64 val, int *lpMinIndex, int *lpMaxIndex)
{
	int size = vec.size();
	int det = -1;//二分法搜索 
	int left = 0;//定义left整数变量
	int right = size - 1;//定义right

	while (left <= right) {//在while循环至直到有一个条件结束搜索 
		int mid = (left + right) / 2;
		if (func(vec[mid]) < val) {
			left = mid + 1;
		}
		else if (func(vec[mid]) >val) {
			right = mid - 1;
		}
		else {
			det = mid;
			break;
		}
	}
	if (lpMinIndex != nullptr) {
		*lpMinIndex = left;
	}
	if (lpMaxIndex != nullptr) {
		*lpMaxIndex = right;
	}

	return det;
}

template <class T>
T findFirst(T node, T(*next) (T), bool(*func)(T))
{
	while (node != nullptr) {
		if (func(node)) {
			return node;
		}
		node = next(node);
	}
	return nullptr;
}


template <class T>
T getLast(T node, T(*next)(T)) {
	while (next(node) != nullptr) {
		node = next(node);
	}
	return node;
}

extern "C" _declspec(dllexport) void* __cdecl Mmalloc(
	_In_ _CRT_GUARDOVERFLOW size_t _Size
);
extern "C" _declspec(dllexport) void* __cdecl Mmemset(
	_Out_writes_bytes_all_(_Size) void*  _Dst,
	_In_                          int    _Val,
	_In_                          size_t _Size
);

extern "C" _declspec(dllexport) void* __cdecl Mmemcpy(
	_Out_writes_bytes_all_(_Size) void* _Dst,
	_In_reads_bytes_(_Size)       void const* _Src,
	_In_                          size_t      _Size
);

extern "C" _declspec(dllexport) int Mmemcmp(const byte* sZbuffer1,const byte* sZbuffer2, int nComSize);
extern "C" _declspec(dllexport) byte findFirstNotIn(const byte *mem, int len);