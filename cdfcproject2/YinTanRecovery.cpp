#include "YinTanRecovery.h"
#include "Common.h"
#include "IO.h"
#include "Common_Recovery.h"

using namespace std;

//鹰潭碎片结构(临时使用);
class YinTanFrag {
public:
	TempVideo* Video;
	UINT32 LastFrameNo;					//上一个帧号;
	UINT32 LastDateNum;					//上一个日期;
	StFileFrag* StFrag;
};

UINT64 getFrameNoFunc(YinTanFrag* frag) {
	return frag->LastFrameNo;
}

class YinTanSearcher : public NormalDavSearcher {
public:
	StDateCategory* cdfc_search_start(HANDLE hDisk, int type, int* error) {
		StVideo * file = nullptr;
		StDateCategory * StDCHeader = nullptr;
		vector<YinTanFrag *> frags;

		INT64 lastFound = -1;
		StYinTanFrame frame;

		//委托A的出参;
		StVideo *stVideo = nullptr;
		TempVideo *wfsVideo = nullptr;
		YinTanFrag *yinTanFrag = nullptr;

		auto val = 2;

		auto headerLen = getArrayLen(YinTanFrameHeader);		//帧头标识长度;

		auto readLen = 0;

		INT64 bufferLen = 1024 * 1024 * 16 + sizeof(StYinTanFrame) - 1;		//缓冲区大小为预缓冲区大小+(pinLen - 1);
		auto buffer = (byte *)Mmalloc(bufferLen);
		Mmemset(buffer + sizeof(StYinTanFrame) - 1, 0, bufferLen - sizeof(StYinTanFrame) + 1);				//分配并置零;
																//Mmemset(buffer, 0,bl);				//分配并置零;

		//找到第一个内存中不匹配的字节;
		auto unMatched = findFirstNotIn(YinTanFrameHeader, headerLen);
		//确保第一次检索时不会对结果产生影响;
		Mmemset(buffer, unMatched , sizeof(StYinTanFrame) - 1);

		//通过一个帧创建完整文件的委托A;
		auto creatNewFile = [&stVideo, &wfsVideo, &yinTanFrag](StYinTanFrame * frame, UINT64 nPos,UINT64 nSize) {
			auto stFrag = new StFileFrag();
			stFrag->StartDate = frame->GetDateNum();
			stFrag->EndDate = frame->GetDateNum();
			stFrag->StartAddress = nPos;
			stFrag->Size = nSize;

			yinTanFrag = new YinTanFrag();
			yinTanFrag->StFrag = stFrag;
			yinTanFrag->LastFrameNo = frame->GetFrameNum();
			yinTanFrag->LastDateNum = frame->GetDateNum();

			stVideo = new StVideo();
			stVideo->StartDate = frame->GetDateNum();
			stVideo->EndDate = frame->GetDateNum();
			stVideo->StartAddress = nPos;
			stVideo->Size = nSize;
			stVideo->stStAdd = stFrag;

			wfsVideo = new TempVideo();
			wfsVideo->StVideo = stVideo;

			yinTanFrag->Video = wfsVideo;
		};

		//插入一个新碎片在队列中的委托;
		auto insertFrag = [&frags](YinTanFrag *frag) {
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

		//完全添加一个新文件的委托;
		auto applyNew = [&frags, &stVideo, &wfsVideo, &yinTanFrag, &insertFrag,this,&StDCHeader]() {
			if (StDCHeader == nullptr) {
				StDCHeader = new StDateCategory();
				StDCHeader->Date = stVideo->StartDate;
				StDCHeader->File = stVideo;

				wfsVideo->StDTGory = StDCHeader;
			}
			else {
				auto dc = StDCHeader;
				set_com_tm(get_dh_time(stVideo->StartDate));
				//寻找同一天的分类节点;
				auto preDc = findFirst(dc, get_next_category, is_cate_same_day);
				//若找到，则在原节点上扩充;
				if (preDc != nullptr) {
					auto file = preDc->File;
					auto lastFile = getLast(file, get_next_video);

					wfsVideo->StDTGory = preDc;

					lastFile->Next = stVideo;
				}
				//否则创建一个节点;
				else {
					auto newDc = new StDateCategory();
					newDc->Date = stVideo->StartDate;
					newDc->File = stVideo;

					wfsVideo->StDTGory = newDc;

					auto lastDc = getLast(dc, get_next_category);
					lastDc->Next = newDc;
				}
			}
			insertFrag(yinTanFrag);
		};

		//创建并应用新文件的委托;
		auto createAndApply = [&applyNew, &creatNewFile](StYinTanFrame * frame, UINT64 nPos,UINT64 nSize) {
			//若非关键帧结构,则不能新建文件;
			if (!frame->IsImportFrame()) {
				return;
			}

			creatNewFile(frame, nPos,nSize);
			applyNew();
		};

		//处理一个帧结构的委托;
		auto dealWithFrame = [&frags, &createAndApply,&insertFrag,this](StYinTanFrame * frame, UINT64 nPos, UINT64 nSize) {
			UINT32 findVal = frame->GetFrameNum() - 1;
			//查找上一个可能相邻的帧;
			auto index = findIndex(frags, getFrameNoFunc, findVal,nullptr, nullptr);
			//若存在，判定为归属于之前的某一个文件;
			if (index != -1) {
				auto min = index;
				auto max = index;

				auto minDisIndex = -1;
				auto minDis = 0xffffffff;

				//向前,后取得所有满足帧号条件的帧的位置;
				while (min > 0 && frags[min - 1]->LastFrameNo == findVal) {
					min--;
				}
				while (max < frags.size() - 1 && frags[max + 1]->LastFrameNo == findVal) {
					max++;
				}
				
				//若为关键帧,若存在多个队列中的帧满足帧号的条件,则进行筛选,则取时间近的关键帧点为准;;
				if (frame->IsImportFrame()) {
					for (size_t i = min; i <= max; i++)
					{
						if (frags[i]->LastDateNum < frame->GetDateNum() &&
							(frame->GetDateNum() - frags[i] -> LastDateNum) < minDis) {
							minDisIndex = i;
							minDis = frame->GetDateNum() - frags[i]->StFrag->EndDate;
						}
					}
				}
				//否则,只考虑一个帧满足的情况;
				else if(min == max){
					minDisIndex = index;
				}
				else {
					auto err = 0;
				}

				if (minDisIndex != -1) {
					auto frag = frags[minDisIndex];
					//如果两个帧是连续的,则直接延长Size;
					if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
						frag->StFrag->Size += nSize;
						frag->Video->StVideo->Size += nSize;
						frag->LastFrameNo = frame->GetFrameNum();

						//若为关键帧,还将延长时间;
						if (frame->IsImportFrame()) {
							frag->LastDateNum = frame->GetDateNum();
							frag->StFrag->EndDate = frame->GetDateNum();
							frag->Video->StVideo->EndDate = frame->GetDateNum();
						}
					}
					//否则添加一个新的文件碎片节点;
					else {
						frag->LastFrameNo = frame->GetFrameNum();

						auto stFrag = new StFileFrag();

						stFrag->StartAddress = nPos;
						stFrag->Size = nSize;

						frag->StFrag->Next = stFrag;

						if (frame->IsImportFrame()) {
							stFrag->StartDate = frame->GetDateNum();
							stFrag->EndDate = frame->GetDateNum();
							frag->Video->StVideo->EndDate = frame->GetDateNum();
						}
						frag->Video->StVideo->Size += nSize;
						
						frag->StFrag = stFrag;
					}
					frags.erase(frags.begin() + minDisIndex);
					insertFrag(frag);
				}
				
				
			}
			//否则新建一个文件;
			else {
				createAndApply(frame, nPos,nSize);
				count2++;
			}
		};

		MSetFilePointer(hDisk, startSec * secSize, FILE_BEGIN);

		//从缓冲区的frameLen - 1处开始写入;
		while ((readLen = MReadFile(hDisk, buffer + sizeof(StYinTanFrame) - 1, bufferLen - sizeof(StYinTanFrame) + 1)) != 0 && pos <= endSec * secSize) {
			//轮询缓冲区pin剩余长度以后的内容;
			for (size_t i = 0; i < readLen; i++) {
				//确定枕头满足条件否;
				if (*(buffer + i) == YinTanFrameHeader[0]) {
					//确定帧头Header是否满足;
					if (Mmemcmp(buffer + i + 1, YinTanFrameHeader + 1, headerLen - 1) == 0) {
						//确定是否为关键帧;(YinTanFrameHeader = "0x5410F006")
						//if (Mmemcmp(buffer + i + 36, YinTanFramePin, pinLen) == 0) {
						auto thisPosition = pos + i - sizeof(StYinTanFrame) + 1;
						if (lastFound != -1) {
							auto lastLength =  thisPosition - lastFound;
							if (lastLength > sizeof(StYinTanFrame)) {
								dealWithFrame(&frame, lastFound, lastLength);
							}
						}
						lastFound = thisPosition;
						Mmemcpy(&frame, buffer + i, sizeof(StYinTanFrame));
						if (frame.FrameLength > 6) {
							i += frame.FrameLength - 1;
						}
						//}
					}
				}
				
			}
			//后面未检索的几位向前推移至缓冲区首;
			Mmemcpy(buffer, buffer + readLen, sizeof(StYinTanFrame) - 1);
			pos += readLen;
			if (stopped) {
				break;
			}
			
		}

		//处理最后一个碎片;
		if (lastFound != -1 && pos - lastFound > sizeof(StYinTanFrame) && !stopped) {
			auto lastSec = lastFound / secSize;
			auto dis = lastFound - lastSec * 512;
			MSetFilePointer(hDisk, lastSec * 512, FILE_BEGIN);
			readLen = MReadFile(hDisk, buffer, 1024);
			Mmemcpy(&frame, buffer + dis, sizeof(StYinTanFrame));
			dealWithFrame(&frame, lastFound,pos - lastFound);
		}

		delete buffer;
				
		//释放临时内存(YinTanFrag,YinTanVideo);
		for (auto i = frags.begin(); i < frags.end(); i++) {
			auto vi = (*i)->Video;
			//printf("%d\n", (*i)->LastFrameNo);
			if (vi != nullptr) {
				delete vi;
			}	
			(*i)->Video = nullptr;
			delete (*i);
		}

		frags.clear();
		frags.shrink_to_fit();

		return StDCHeader;
	}
};

YinTanSearcher* searcher = new YinTanSearcher();

bool cdfc_tdwy_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk) {
	searcher->cdfc_init(nStartSec, nEndSec, nSecSize, 0, nLBAPos, hDisk);
	return searcher->cdfc_init(nStartSec,nEndSec, nSecSize,nTimePos,nLBAPos, hDisk);
	
}

StDateCategory* cdfc_tdwy_search_start(HANDLE handle, int type, int* error) {
	return searcher->cdfc_search_start(handle, type, error);
}

bool cdfc_tdwy_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
	return searcher->cdfc_filesave(szFile,hDisk,saveFileHandle, nCurrSizeDW , nError);
}

void cdfc_tdwy_exit(StDateCategory *stFile) {
	searcher->cdfc_exit(stFile);
}

StDateCategory* cdfc_tdwy_filelist() {
	return searcher->cdfc_filelist();
}

void cdfc_tdwy_stop() {
	searcher->cdfc_stop();
}

void cdfc_tdwy_set_regionsize(UINT64 regionSize){
	searcher->cdfc_set_regionsize(regionSize);
}

bool cdfc_tdwy_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
	return searcher->cdfc_readbuffer(szFile, hDisk, szBuffer, nBuffSize);
}

UINT64 cdfc_tdwy_current_sector() {
	return searcher->cdfc_current_sector();
}

void cdfc_tdwy_date_converter(UINT64 date, UINT8* resDate) {
	return searcher->cdfc_date_converter(date,resDate);
}

void cdfc_tdwy_set_preview(UINT64 regionSize) {
	return searcher->cdfc_set_regionsize(regionSize);
}