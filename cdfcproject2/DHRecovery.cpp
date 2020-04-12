#include "DHRecovery.h"
#include "Common_Recovery.h"
#include "Common.h"

//碎片结构(临时使用);
class DHTempFrag {
public:
	TempVideo * Video;
	UINT16 LastFrameNo1;					//上一个帧号1;(间距为1)
	UINT32 LastDateNum;					//上一个日期;
	StFileFrag* StFrag;					//上一个碎片;
};

UINT64 getFrameNoFunc(DHTempFrag* frag) {
	return frag->LastFrameNo1;
}

class DHSearcher : public NormalDavSearcher {
public:
	DHSearcher()
	{
		get_time = get_dh_time;
	}
	StDateCategory * cdfc_search_start(HANDLE hDisk, int type, int* error) {
		StVideo * file = nullptr;
		StDateCategory * StDCHeader = nullptr;
		vector<DHTempFrag *> frags;

		INT64 lastFound = -1;
		StDHFrame frame;

		//委托A的出参;
		StVideo *stVideo = nullptr;
		TempVideo *wfsVideo = nullptr;
		DHTempFrag *yinTanFrag = nullptr;

		auto val = 2;

		auto headerLen = getArrayLen(DHFrameHeader);		//帧头标识长度;

		auto readLen = 0;

		const UINT32 MaxFrameLength = 1048576;

		INT64 bufferLen = 1024 * 1024 * 16 + sizeof(StDHFrame) - 1;		//缓冲区大小为预缓冲区大小+(pinLen - 1);
		auto buffer = (byte *)Mmalloc(bufferLen);
		Mmemset(buffer + sizeof(StDHFrame) - 1, 0, bufferLen - sizeof(StDHFrame) + 1);				//分配并置零;
																									//Mmemset(buffer, 0,bl);				//分配并置零;

																									//找到第一个内存中不匹配的字节;
		auto unMatched = findFirstNotIn(DHFrameHeader, headerLen);
		//确保第一次检索时不会对结果产生影响;
		Mmemset(buffer, unMatched, sizeof(StDHFrame) - 1);

		//通过一个帧创建完整文件的委托A;
		auto creatNewFile = [&stVideo, &wfsVideo, &yinTanFrag](StDHFrame * frame, UINT64 nPos, UINT64 nSize) {
			auto stFrag = new StFileFrag();
			stFrag->StartDate = frame->GetDateNum();
			stFrag->EndDate = frame->GetDateNum();
			stFrag->StartAddress = nPos;
			stFrag->Size = nSize;

			yinTanFrag = new DHTempFrag();
			yinTanFrag->StFrag = stFrag;
			yinTanFrag->LastFrameNo1 = frame->GetFrameNum();
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
		auto insertFrag = [&frags](DHTempFrag *frag) {
			auto left = 0, right = 0;
			auto index = findIndex(frags, getFrameNoFunc, frag->LastFrameNo1, &left, &right);
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
		auto applyNew = [&frags, &stVideo, &wfsVideo, &yinTanFrag, &insertFrag, this,&StDCHeader]() {
			if (StDCHeader == nullptr) {
				StDCHeader = new StDateCategory();
				StDCHeader->Date = stVideo->StartDate;
				StDCHeader->File = stVideo;

				wfsVideo->StDTGory = StDCHeader;
			}
			else {
				auto dc = StDCHeader;
				set_com_tm(get_time(stVideo->StartDate));
				set_com_func(get_time);
				/*auto func2 = &HBSearcher::cdfc_convert_int_to_tm;
				set_com_func(*func2);*/
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
		auto createAndApply = [&applyNew, &creatNewFile](StDHFrame * frame, UINT64 nPos, UINT64 nSize) {
			//若非关键帧结构,则不能新建文件;
			if (!frame->IsImportFrame()) {
				return;
			}

			creatNewFile(frame, nPos, nSize);
			applyNew();
		};

		//处理一个帧结构的委托;
		auto dealWithFrame = [&frags, &createAndApply, &insertFrag, this](StDHFrame * frame, UINT64 nPos, UINT64 nSize) {
			UINT32 findVal = frame->GetPreFrameNum();
#if _DEBUG
			if (nPos == 607345) {
				auto sss = 0;
			}
#endif
			
			//查找上一个可能相邻的帧;(二分)
			auto index = findIndex(frags, getFrameNoFunc, findVal, nullptr, nullptr);

			//若存在，判定为归属于之前的某一个文件;
			if (index != -1) {
				auto min = index;
				auto max = index;

				auto minDisIndex = -1;
				auto minDis = 0xffffffff;

				//向前,后取得所有满足帧号条件的帧的位置;
				while (min > 0 && frags[min - 1]->LastFrameNo1 == findVal) {
					min--;
				}
				while (max < frags.size() - 1 && frags[max + 1]->LastFrameNo1 == findVal) {
					max++;
				}

				//若为关键帧,若存在多个队列中的帧满足帧号的条件,则进行筛选,则取时间近的关键帧点为准;;
				//if (frame->IsImportFrame()) {
				for (size_t i = min; i <= max; i++)
				{

					auto frag = frags[i];

					if ( frame->GetDateNum() >= frag->StFrag->EndDate &&
						 frame->GetDateNum() - frag->StFrag->EndDate < minDis) {
#if _DEBUG
						if (minDisIndex != -1) {
							auto err = 1;
						}
#endif
						minDisIndex = i;
						minDis = frame->GetDateNum() - frag->StFrag->EndDate;
					}

				}
				//}


				//若找到最佳项;
				if (minDisIndex != -1) {
					auto frag = frags[minDisIndex];
					frag->LastFrameNo1 = frame->GetFrameNum();
					frag->LastDateNum = frame->GetDateNum();
					frag->Video->StVideo->EndDate = frame->GetDateNum();
					frag->StFrag->EndDate = frame->GetDateNum();

					frag->Video->StVideo->Size += nSize;

					//如果两个帧是连续的,则直接延长碎片的Size;
					if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
						frag->StFrag->Size += nSize;
						////若为关键帧,还将延长时间;
						//if (frame->IsImportFrame()) {
						//	
						//}
					}
					//否则添加一个新的文件碎片节点;
					else {
						auto stFrag = new StFileFrag();

						stFrag->StartAddress = nPos;
						stFrag->Size = nSize;
						stFrag->StartDate = frame->GetDateNum();
						stFrag->EndDate = frame->GetDateNum();

						frag->StFrag->Next = stFrag;

						frag->StFrag = stFrag;
					}
					frags.erase(frags.begin() + minDisIndex);
					insertFrag(frag);
				}
				//否则,只考虑一个帧满足的情况;
				else {
					createAndApply(frame, nPos, nSize);
					count2++;
				}

			}
			////可能为反常的关键帧,直接修改至"合法"后插入之前最后一个节点;
			//else if (frame->IsImportFrame() && frags.size() != 0) {
			//	auto minDesIndex = -1;
			//	UINT64 minDes = 0 - 1;
			//	for (size_t i = 0; i < frags.size(); i++){
			//		if (nPos - frags[i]->StFrag->StartAddress < minDes) {
			//			minDes = nPos - frags[i]->StFrag->StartAddress;
			//			minDesIndex = i;
			//		}
			//	}
			//}
			//否则新建一个文件;
			else {
				createAndApply(frame, nPos, nSize);
				count2++;
			}
		};

		MSetFilePointer(hDisk, startSec * secSize, FILE_BEGIN);
		
		auto err = GetLastError();
		//从缓冲区的frameLen - 1处开始写入;
		while ((readLen = MReadFile(hDisk, buffer + sizeof(StDHFrame) - 1, bufferLen - sizeof(StDHFrame) + 1)) != 0 && pos <= endSec * secSize) {
			//轮询缓冲区pin剩余长度以后的内容;
			for (size_t i = 0; i < readLen; i++) {
				//确定枕头满足条件否;
				if (*(buffer + i) == DHFrameHeader[0]) {
					//确定帧头Header是否满足;
					if (Mmemcmp(buffer + i + 1, DHFrameHeader + 1, headerLen - 1) == 0
						&&(* (buffer + i + 4) == NormalDHPin ||
						 *(buffer + i + 4) == BigDHPin)) {

						//确定是否为关键帧;
						//if (Mmemcmp(buffer + i + 36, YinTanFramePin, pinLen) == 0) {
						auto thisPosition = pos + i - sizeof(StDHFrame) + 1;
						if (lastFound != -1) {
							auto lastLength = thisPosition - lastFound;
							//确定和之前的帧间距
							if (lastLength > sizeof(StDHFrame) && lastLength <= MaxFrameLength) {
#if _DEBUG
								if (lastFound == 607345) {
									auto sss = 0;
								}
#endif
								dealWithFrame(&frame, lastFound, lastLength);
							}
						}

						Mmemcpy(&frame, buffer + i, sizeof(StDHFrame));
						lastFound = thisPosition;
						
						
						/*if (frame.FrameLength > 6) {
						i += frame.FrameLength - 1;
						}*/
						//}
					}
				}

			}
			//后面未检索的几位向前推移至缓冲区首;
			Mmemcpy(buffer, buffer + readLen, sizeof(StDHFrame) - 1);
			pos += readLen;
			if (stopped) {
				break;
			}

		}


		//处理最后一个碎片;
		if (lastFound != -1 && pos - lastFound > sizeof(StDHFrame) && !stopped) {
			auto lastSec = lastFound / secSize;
			auto dis = lastFound - lastSec * secSize;
			MSetFilePointer(hDisk, lastSec * secSize, FILE_BEGIN);
			readLen = MReadFile(hDisk, buffer, 1024);
			Mmemcpy(&frame, buffer + dis, sizeof(StDHFrame));
			dealWithFrame(&frame, lastFound, pos - lastFound);
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
auto searcher = new DHSearcher();

StDateCategory* dahua_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError) {
	searcher->cdfc_init(nStartSec, nEndSec, nSecSize, 0, nLBAPos, hDisk);
	if (eType == eSearchType_FULL) {
		return searcher->cdfc_search_start(hDisk, 0, nError);
	}
	return nullptr;
}

bool dahua_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError) {
	return searcher->cdfc_filesave(stFile, hDisk, hFileOut, nCurrSizeDW, nError);
}

void dahua_exit(void* stFile) {
	searcher->cdfc_exit((StDateCategory *)stFile);
}

void dahua_stop() {
	searcher->cdfc_stop();
}

void dahua_get_data(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount) {
	searcher->get_data(nOffsetSec, nFileCount);
}

bool dahua_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize) {
	return searcher->cdfc_readbuffer(szFile, hDisk, szBuffer, nBuffSize);
}

void dahua_date_converter(UINT32 nDate, int8_t *date) {
	searcher->cdfc_date_converter(nDate, (UINT8 *)date);
}


StDateCategory* cdfc_dahua_search_start(HANDLE handle, int type, int* error);

StDateCategory* cdfc_dahua_search_start_f(HANDLE handle, int type, int* error);

UINT64 cdfc_dahua_current_sector();

StDateCategory* cdfc_dahua_filelist();

void cdfc_dahua_exit();

void cdfc_dahua_stop();

void cdfc_dahua_date_converter(UINT64 date, UINT32* resDate);

void cdfc_dahua_set_regionsize(UINT64 regionSize);

StDateCategory* cdfc_dahua_search_start(HANDLE handle, int type, int* error);

StDateCategory* cdfc_dahua_filelist_system();

void cdfc_dahua_set_preview(UINT64 nSize);

bool cdfc_dahua_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

bool cdfc_dahua_filesave_f(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError);

bool cdfc_dahua_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);

bool cdfc_dahua_readbuffer_f(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize);



//楚河汉界;
bool cdfc_dahua_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk) {
	return searcher->cdfc_init(nStartSec, nEndSec, nSecSize, 0, nLBAPos, hDisk);
}

StDateCategory* cdfc_dahua_search_start_free(HANDLE handle, int type, int* error) {
	/*auto sType = FromTypeToSearchType(type);
	if (sType == eSearchType_FULL) {

	}*/
	return searcher->cdfc_search_start(handle, 0, error);
	//return nullptr;
}

UINT64 cdfc_dahua_current_sector() {
	return searcher->cdfc_current_sector();
}

StDateCategory* cdfc_dahua_filelist_2() {
	return searcher->cdfc_filelist();
}

StDateCategory* cdfc_dahua_filelist() {
	return searcher->cdfc_filelist();
}

void cdfc_dahua_exit() {
	searcher->cdfc_exit(nullptr);
}

void cdfc_dahua_stop() {
	searcher->cdfc_stop();
}

void cdfc_dahua_date_converter(UINT32 date, UINT8* resDate) {
	if (resDate == nullptr) {
		printf_s("date can't be null");
		return;
	}

	searcher->cdfc_date_converter(date, resDate);
}

bool cdfc_dahua_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
	return searcher->cdfc_filesave(szFile, hDisk, saveFileHandle, nCurrSizeDW, nError);
}

bool cdfc_dahua_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
	return searcher->cdfc_readbuffer(szFile, hDisk, szBuffer, nBuffSize);
}




