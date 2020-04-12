#pragma once
#include "Common.h"
#include "WFSRecovery.h"
#include "IO.h"
#include <vector>
#include <time.h>
#include "Common_Recovery.h"

using namespace std;

StDateCategory * StDCHeader = nullptr;
HANDLE hFile;							//��ª��WinApi;

UINT64 startSec;					//��ʼ������;
UINT64 endSec;						//��ֹ������;
UINT64 timePos;						//ʱ��ƫ��;
UINT64 lbaPos;						//ƫ��lba;
int secSize;						//������С;

INT64 pos;						//��ǰɨ��ƫ��;
bool stopped;					//�Ƿ�ֹͣ��;
UINT64 previewSize = 0;			//Ԥ����С;

typedef class WFSVideo;

//WFS��Ƭ�ṹ(��ʱʹ��);
class WFSFrag {
public:
	WFSVideo* Video;
	byte LastFrameNo;
	StFileFrag* StFrag;
};

class WFSVideo {
public:
	StVideo* StVideo;
	StDateCategory* StDTGory;
};

UINT64 getFrameNoFunc(WFSFrag* frag) {
	return frag->LastFrameNo;
}

void cdfc_wfs_date_converter(UINT64 date, UINT32* resDate) {
	auto tm = get_dh_time(date);
//	localtime((time_t) date);
	resDate[0] == tm.tm_year;
	resDate[1] == tm.tm_mon;
	resDate[2] == tm.tm_mday;
	resDate[3] == tm.tm_hour;
	resDate[4] == tm.tm_min;
	resDate[5] == tm.tm_sec;
	
}

bool cdfc_wfs_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk) {
	startSec = nStartSec;
	endSec = nEndSec;
	timePos = nTimePos;
	lbaPos = nLBAPos;
	secSize = nSecSize;

	hFile = hDisk;
	pos = nStartSec * secSize;

	stopped = false;

	if (secSize <= 0 || secSize % 512 != 0) {
		return false;
	}
	if (hFile == nullptr) {
		return false;
	}
	return true;
}

UINT64 cdfc_wfs_current_sector() {
	return pos / secSize;
}

StDateCategory* cdfc_wfs_filelist() {
	return StDCHeader;
}

void cdfc_wfs_stop() {
	stopped = true;
}

int count2 = 0;
StDateCategory* cdfc_wfs_search_start_free(HANDLE handle, int type, int* error) {
	StVideo * file = nullptr;
	vector<WFSFrag *> frags;

	auto val = 2;

	auto pinLen = getArrayLen(WFSFramePin);						//��ñ�ʶ����;

	auto readLen = 0;

	INT64 bufferLen = 1024 * 1024 * 16 + pinLen - 1;		//��������СΪԤ��������С+(pinLen - 1);
	auto buffer = (byte *)Mmalloc(bufferLen);
	Mmemset(buffer, 0, bufferLen - pinLen + 1);				//���䲢����;
															//Mmemset(buffer, 0,bl);				//���䲢����;

	//��ǰpinLen - 1λ��Ϊff,ȷ����һ��ѭ��ʱǰ��λ����Ӱ����;
	for (size_t i = 0; i < pinLen - 1; i++)
	{
		//����pin[i],���������ֽ�,�ҵ��������;
		for (size_t j = 0; j < 0xff; j++)
		{
			//�Ƿ��ҵ���ȵĽ��;
			auto findMatch = false;
			for (size_t pinIndex = 0; pinIndex < pinLen - 1; pinIndex++)
			{
				if (WFSFramePin[pinIndex] == j) {
					findMatch = true;
					break;
				}
			}
			
			if (!findMatch) {
				buffer[i] = j;
			}
		}
	}

	INT64 lastFound = -1;
	StWFSFrame frame;

	//ί��A�ĳ���;
	StVideo *stVideo = nullptr;
	WFSVideo *wfsVideo = nullptr;
	WFSFrag *wfsFrag = nullptr;

	//ͨ��һ��֡�õ������ļ���ί��A;
	auto creatNewFile = [&stVideo, &wfsVideo, &wfsFrag](StWFSFrame * frame, UINT64 nPos, UINT64 nSize) {
		auto stFrag = new StFileFrag();
		stFrag->StartDate = frame->DateNum;
		stFrag->EndDate = frame->DateNum;
		stFrag->StartAddress = nPos;
		stFrag->Size = nSize;

		wfsFrag = new WFSFrag();
		wfsFrag->StFrag = stFrag;
		wfsFrag->LastFrameNo = frame->FrameNo;

		stVideo = new StVideo();
		stVideo->StartDate = frame->DateNum;
		stVideo->EndDate = frame->DateNum;
		stVideo->StartAddress = nPos;
		stVideo->Size = nSize;
		stVideo->stStAdd = stFrag;

		wfsVideo = new WFSVideo();
		wfsVideo->StVideo = stVideo;

		wfsFrag->Video = wfsVideo;
	};

	//����һ������Ƭ�ڶ����е�ί��;
	auto insertFrag = [&frags](WFSFrag *frag) {
		auto left = 0, right = 0;
		auto index = findIndex(frags, getFrameNoFunc, frag->LastFrameNo, &left, &right);
		if (index != -1) {
			frags.insert(frags.begin() + index, frag);
		}
		else {
			if (right == -1) {
				frags.insert(frags.begin(), frag);
			}
			else if (left == frags.size()) {
				frags.push_back(frag);
			}
			else if (left > right) {
				frags.insert(frags.begin() + left, frag);
			}
		}
	};

	//��ȫ���һ�����ļ���ί��;
	auto applyNew = [&frags, &stVideo, &wfsVideo, &wfsFrag, &insertFrag]() {
		if (StDCHeader == nullptr) {
			StDCHeader = new StDateCategory();
			StDCHeader->Date = stVideo->StartDate;
			StDCHeader->File = stVideo;

			wfsVideo->StDTGory = StDCHeader;
		}
		else {
			auto dc = StDCHeader;
			set_com_tm(get_dh_time(stVideo->StartDate));
			//Ѱ��ͬһ��ķ���ڵ�;
			auto preDc = findFirst(dc, get_next_category, is_cate_same_day);
			//���ҵ�������ԭ�ڵ�������;
			if (preDc != nullptr) {
				auto file = preDc->File;
				auto lastFile = getLast(file, get_next_video);

				wfsVideo->StDTGory = preDc;

				lastFile->Next = stVideo;
			}
			//���򴴽�һ���ڵ�;
			else {
				auto newDc = new StDateCategory();
				newDc->Date = stVideo->StartDate;
				newDc->File = stVideo;

				wfsVideo->StDTGory = newDc;

				auto lastDc = getLast(dc, get_next_category);
				lastDc->Next = newDc;
			}
		}
		insertFrag(wfsFrag);
	};

	//����һ��֡�ṹ��ί��;
	auto dealWithFrame = [&frags, &applyNew, &creatNewFile, &insertFrag](StWFSFrame * frame, UINT64 nPos, UINT64 nSize) {
		/*if (frame->FrameNo == 44) {
			auto s = 0;
		}*/

		int left = 0, right = 0;
		byte findVal = frame->FrameNo - 2;
		/*if (frame->FrameNo < 2) {
			auto s = 0;
		}*/

		//������һ���������ڵ�֡;
		auto index = findIndex(frags, getFrameNoFunc, findVal, &left, &right);
		//�����ڣ��ж�Ϊ������֮ǰ��ĳһ���ļ�;
		if (index != -1) {
			auto min = index;
			auto max = index;

			auto minDisIndex = -1;
			auto minDis = 0;

			while (min > 0 && frags[min - 1]->LastFrameNo == frame->FrameNo - 2) {
				min--;
			}
			while (max < frags.size() - 1 && frags[max + 1]->LastFrameNo == frame->FrameNo - 2) {
				max++;
			}

			auto aMin = 0;
			auto aMax = 0;
			bool found = false;
			auto in = 0;
			for (auto i = frags.begin(); i < frags.end(); i++)
			{
				if ((*i)->LastFrameNo == frame->FrameNo - 2) {
					if (!found) {
						aMin = in;
						aMax = in;
						found = true;
					}
				}
				else {
					if (found) {
						aMax = in - 1;
						break;
					}
				}
				in++;
			}
			if (aMax != max || aMin != min) {
				auto sda = 0;
			}
			for (size_t i = min; i <= max; i++)
			{
				if (frags[i]->StFrag->EndDate < frame->DateNum &&
					((minDisIndex == -1) || (frame->DateNum - frags[i]->StFrag->EndDate) < minDis)) {
					minDisIndex = i;
					minDis = frame->DateNum - frags[i]->StFrag->EndDate ;
				}
			}
			
			if (minDisIndex != -1) {
				auto frag = frags[minDisIndex];
				//�������֡��������,��ֱ���ӳ�Size,EndDate;
				if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
					frag->StFrag->Size += nSize;
					frag->StFrag->EndDate = frame->DateNum;

					frag->Video->StVideo->Size += nSize;
					frag->Video->StVideo->EndDate = frame->DateNum;

					frag->LastFrameNo = frame->FrameNo;

					frags.erase(frags.begin() + minDisIndex);

					insertFrag(frag);
				}
				//�������һ���µ��ļ���Ƭ�ڵ�;���Ƴ����ͷ�֮ǰ�����ڵ�;
				else {
					auto stFrag = new StFileFrag();
					auto wfsFrag = new WFSFrag();
					stFrag->StartAddress = nPos;
					stFrag->Size = nSize;
					stFrag->StartDate = frame->DateNum;
					stFrag->EndDate = frame->DateNum;

					wfsFrag->LastFrameNo = frame->FrameNo;
					wfsFrag->Video = frag->Video;
					wfsFrag->StFrag = stFrag;

					frag->Video->StVideo->Size += nSize;
					frag->Video->StVideo->EndDate = frame->DateNum;
					frag->StFrag->Next = stFrag;

					delete *(frags.begin() + minDisIndex);
					frags.erase(frags.begin() + minDisIndex);
					insertFrag(wfsFrag);
				}
			}
			else {
				creatNewFile(frame, nPos, nSize);
				applyNew();
				count2++;
			}
		}
		//�����½�һ���ļ�;
		else {
			creatNewFile(frame, nPos, nSize);
			applyNew();
			count2++;
		}
	};

	MSetFilePointer(hFile, startSec * secSize, FILE_BEGIN);
	while ((readLen = MReadFile(hFile, buffer + pinLen - 1, bufferLen - pinLen + 1)) != 0) {
		//��ѯ������pinʣ�೤���Ժ������;
		for (size_t i = 0; i < readLen; i++) {
			if (*(buffer + i) == WFSFramePin[0]) {
				if (Mmemcmp(buffer + i + 1, WFSFramePin + 1, pinLen - 1) == 0) {
					if (lastFound != -1) {
						auto lastLength = pos + i - pinLen + 1 - lastFound;
						if (lastLength > sizeof(StWFSFrame)) {
							dealWithFrame(&frame, lastFound, lastLength);
						}
					}

					lastFound = pos + i - pinLen + 1;
					Mmemcpy(&frame, buffer + i, sizeof(StWFSFrame));
					i += pinLen;
				}
			}
		}
		//����δ�����ļ�λ��ǰ��������������;
		Mmemcpy(buffer, buffer + readLen, pinLen - 1);	
		pos += readLen;
		if (stopped) {
			break;
		}
	}

	//�������һ����Ƭ;
	if (lastFound != -1 && pos - lastFound > sizeof(StWFSFrame) && !stopped) {
		auto lastSec = lastFound / secSize;
		auto dis = lastFound - lastSec * 512;
		MSetFilePointer(hFile, lastSec * 512, FILE_BEGIN);
		readLen = MReadFile(hFile, buffer, 1024);
		Mmemcpy(&frame, buffer + dis, sizeof(StWFSFrame));
		dealWithFrame(&frame, lastFound, pos - lastFound);
	}

	delete buffer;
	auto sz = 0;
	auto s = StDCHeader;

	//�ͷ���ʱ�ڴ�(WFSFrag,WFSVideo);
	for (auto i = frags.begin(); i < frags.end(); i++) {
		auto vi = (*i)->Video;
		//printf("%d\n", (*i)->LastFrameNo);
		if (vi != nullptr) {
			delete vi;
		}
		delete (*i);
	}

	frags.clear();
	frags.shrink_to_fit();
	
	return StDCHeader;
}

void cdfc_wfs_set_preview(UINT64 nSize) {
	previewSize = nSize;
}

bool cdfc_wfs_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
	auto bufferSize = 1024 * 1024 * 16;
	auto buffer = (byte *)Mmalloc(bufferSize);
	auto frag = szFile->stStAdd;
	*nCurrSizeDW = 0;
	
	while (frag != nullptr) {
		MSetFilePointer(hFile, frag -> StartAddress,FILE_BEGIN);
		
		UINT64 readIndex = 0;
		while (readIndex < frag -> Size) {
			auto thisReadSize = (frag -> Size - readIndex) > bufferSize?bufferSize:frag -> Size % bufferSize;
			if (previewSize != 0) {
				thisReadSize = min(previewSize - *nCurrSizeDW, thisReadSize);
			}
			
			MReadFileInSector(hDisk, buffer, thisReadSize, secSize, frag->StartAddress + readIndex);
			MWriteFile(saveFileHandle, buffer, thisReadSize);

			readIndex += thisReadSize;
			*nCurrSizeDW += thisReadSize;
			if (*nCurrSizeDW >= previewSize && previewSize != 0) {
				break;
			}
		}

		if (*nCurrSizeDW >= previewSize && previewSize != 0) {
			break;
		}
		frag = frag->Next;
	}
	delete buffer;
	return true;
}

bool cdfc_wfs_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
	auto frag = szFile->stStAdd;

	UINT64 recSize = 0;

	while (frag != nullptr) {
		MSetFilePointer(hFile, frag->StartAddress, FILE_BEGIN);
		
		auto thisReadSize = min(frag->Size, nBuffSize - recSize);
		
		MReadFileInSector(hDisk, szBuffer + recSize, thisReadSize, secSize, frag->StartAddress);
		frag = frag->Next;
		recSize += thisReadSize;

		if (recSize >= nBuffSize) {
			break;
		}
	}

	return true;
}
//wfs�˳���
void cdfc_wfs_exit() {
	//�ͷ��ļ��б�;
	auto cateNode = StDCHeader;
	while (cateNode != nullptr) {
		auto vNode = cateNode->File;
		while (vNode != nullptr) {
			auto fragNode = vNode->stStAdd;
			while (fragNode != nullptr) {
				auto temNode = fragNode;
				fragNode = fragNode->Next;
				delete temNode;
			}
			auto temNode = vNode;
			vNode = vNode->Next;
			delete temNode;
		}
		auto temNode = cateNode;
		cateNode = cateNode->Next;
		delete temNode;
	}

	StDCHeader = nullptr;
}

