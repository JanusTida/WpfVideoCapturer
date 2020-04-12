// CDFCSnapshoter.cpp : Defines the exported functions for the DLL application.
//
#include "stdafx.h"
#include "DHMemPLayer.h"
#include "dhplay.h"
#include "dhplayEx.h"
#include <vector>
//图像缓冲区结果;
typedef struct BitMapBuffer {
	PBYTE Buffer = nullptr;
	DWORD BufferSize = 0;
	DWORD ActualSize = 0;
	BitMapBuffer * Next = nullptr;
}BitMapBuffer,* PBitMapBuffer;

class DHMemPlayer {
private:
	HWND playerHandle = nullptr;
	UINT8 * stream = nullptr;
	UINT32 streamSize = 0;
	INT32 port = 0;
	bool playing = false;
	bool stopped = true;
	bool paused = false;
	float speed = 1.0;

	void HandleError(UINT32* error){
		if (!error) {
			*error = PLAY_GetLastError(port);
		}
	}
public:
	bool Set_Speed(float fCoff,UINT32* error) {
		if (fCoff >= 0.1 && fCoff <= 64.0) {
			speed = fCoff;
			return true;
		}
		else {
			return false;
		}
	}

	//由于原播放接口无法控制内存流的位置，故采用PLAY_OneByOne循环的方式。
	bool Play(UINT32* error) {
		double frameRate = 0.0;
		
		if (PLAY_OpenStream(port, nullptr, 0, streamSize)
			&& PLAY_Play(port,playerHandle)
			&& PLAY_InputData(port, stream, streamSize)) {
			//PLAY_Play(port,);
			frameRate = PLAY_GetCurrentFrameRateEx(port);
			auto frameDuration = 0.0;
			if (frameRate != 0) {
				frameDuration = 1000 / frameRate;
			}
			else {
				PLAY_CloseStream(port);
				return false;
			}
			playing = true;
			stopped = false;
			auto isFinished = false;
			auto curFrame = PLAY_GetCurrentFrameNum(port);
			while (!stopped && !isFinished) {
				if (!paused) {
					isFinished = !PLAY_OneByOne(port);
				}
				curFrame = PLAY_GetCurrentFrameNum(port);
				Sleep(frameDuration / (double)speed);
			}
			PLAY_CloseStream(port);
			stopped = true;
			playing = false;
			return true;
		}
		else {
			HandleError(error);
			return false;
		}
		
	}
	//取某个时间的帧图像;
	bool SnapAt(UINT32 milliSeconds,PBYTE bmpBuffer,UINT32 bufferSize,unsigned long * actualSize,UINT32* error) {
		//得到空闲端口;
		LONG freePort = 0;
		auto freePortOK = PLAY_GetFreePort(&freePort);
		if (!freePortOK) {
			return false;
		}

		if (PLAY_OpenStream(freePort, nullptr, 0, streamSize)
			&& PLAY_Play(freePort, nullptr)
			&& PLAY_InputData(freePort, stream, streamSize)) {
			BOOL succeed = false;
			auto frameRate = PLAY_GetCurrentFrameRateEx(freePort);

			if (frameRate != 0) {
				auto frameDuration = 1000 / frameRate; //帧的过渡时间;
				auto isFinished = false;	//是否已经完成;
				auto frameNo = 0;
				auto targetFrameNo = milliSeconds / frameDuration;//获得所需的帧号;
				auto s = PLAY_SetCurrentFrameNum(freePort, targetFrameNo);
				for (frameNo = 0; frameNo < targetFrameNo; frameNo++) {
					PLAY_OneByOne(freePort);
				}
				succeed = PLAY_GetPicJPEG(freePort, bmpBuffer, bufferSize, actualSize, 50);
			}

			PLAY_Stop(freePort);
			PLAY_DestroyStream(freePort);

			return succeed;
		}
		else {
			HandleError(error);
			return false;
		}
	}
	
	bool Stop(UINT32 *error) {
		stopped = true;
		while (playing) {
			Sleep(10);
		}
		if (PLAY_Stop(port)) {
			PLAY_CloseStream(0);
			return true;
		}
		else {
			HandleError(error);
			return false;
		}
	}
	bool Free() {
		PLAY_Stop(port);
		PLAY_DestroyStream(port);
		return true;
	}
	INT32 GetPort() {
		return port;
	}
	DHMemPlayer::DHMemPlayer(INT32 port,HWND containerHandle,UINT8 * stream,UINT32 streamSize) {
		playerHandle = containerHandle;
		this->stream = stream;
		this->streamSize = streamSize;
		this->port = port;
	}
};
std::vector<DHMemPlayer*> playerList;
DHMemPlayer * GetMemPlayer(UINT32 port ,int *res_index = nullptr) {
	auto size = playerList.size();
	for (size_t i = 0; i < size; i++)
	{
		auto node = playerList.at(i);
		if (node->GetPort() == port) {
			if (res_index != nullptr) {
				*res_index = i;
			}
			return node;
		}
	}
	if (res_index != nullptr) {
		*res_index = -1;
	}
	return nullptr;
}
BOOL DH_MemPLAY_OpenStream(LONG nPort, PBYTE pFileHeadBuf, UINT32 nSize, UINT32 nBufPoolSize) {
	return PLAY_OpenStream(nPort, pFileHeadBuf, nSize, nBufPoolSize);
}
BOOL DH_MemPLAY_InputData(LONG nPort, PBYTE pBuf, UINT32 nSize) {
	return PLAY_InputData(nPort, pBuf,  nSize);
}
unsigned long DH_PLAY_GetLastError(LONG nPort) {
	return PLAY_GetLastError(nPort);
}
BOOL DH_MemPLAY_Init(UINT32 port, HWND containerHandle, UINT8 * stream, UINT32 streamSize) {
	auto node = GetMemPlayer(port);
	if (node == nullptr) {
		node = new DHMemPlayer(port, containerHandle, stream, streamSize);
		playerList.push_back(node);
		return true;
	}
	else {
		return false;
	}	
}
BOOL DH_MemPLAY_SnapAt(UINT32 nPort, UINT32 milliSeconds, PBYTE bmpBuffer, UINT32 bufferSize, unsigned long * actualSize, UINT32* error) {
	auto node = GetMemPlayer(nPort);
	if (node != nullptr) {
		return  node->SnapAt(milliSeconds,bmpBuffer,bufferSize, actualSize, error);
	}
	else {
		return false;
	}
}
BOOL DH_MemPLAY_PLAY(UINT32 nPort, UINT32* error) {
	auto node = GetMemPlayer(nPort);
	if (node != nullptr) {
		return node->Play(error);
	}
	else {
		return false;
	}
}
BOOL DH_MemPLAY_Free(UINT32 nPort) {
	auto index = 0;
	auto node = GetMemPlayer(nPort,&index);
	if (node != nullptr) {
		node->Free();
		playerList.erase(playerList.begin() + index);
		delete node;
		return true;
	}
	else {
		return false;
	}
}
BOOL DH_MemPLAY_Stop(UINT32 nPort, UINT32* error) {
	auto node = GetMemPlayer(nPort);
	if (node != nullptr) {
		return node->Stop(error);
	}
	return false;
}





