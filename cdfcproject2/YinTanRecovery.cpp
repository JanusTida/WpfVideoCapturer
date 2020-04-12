#include "YinTanRecovery.h"
#include "Common.h"
#include "IO.h"
#include "Common_Recovery.h"

using namespace std;

//ӥ̶��Ƭ�ṹ(��ʱʹ��);
class YinTanFrag {
public:
	TempVideo* Video;
	UINT32 LastFrameNo;					//��һ��֡��;
	UINT32 LastDateNum;					//��һ������;
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

		//ί��A�ĳ���;
		StVideo *stVideo = nullptr;
		TempVideo *wfsVideo = nullptr;
		YinTanFrag *yinTanFrag = nullptr;

		auto val = 2;

		auto headerLen = getArrayLen(YinTanFrameHeader);		//֡ͷ��ʶ����;

		auto readLen = 0;

		INT64 bufferLen = 1024 * 1024 * 16 + sizeof(StYinTanFrame) - 1;		//��������СΪԤ��������С+(pinLen - 1);
		auto buffer = (byte *)Mmalloc(bufferLen);
		Mmemset(buffer + sizeof(StYinTanFrame) - 1, 0, bufferLen - sizeof(StYinTanFrame) + 1);				//���䲢����;
																//Mmemset(buffer, 0,bl);				//���䲢����;

		//�ҵ���һ���ڴ��в�ƥ����ֽ�;
		auto unMatched = findFirstNotIn(YinTanFrameHeader, headerLen);
		//ȷ����һ�μ���ʱ����Խ������Ӱ��;
		Mmemset(buffer, unMatched , sizeof(StYinTanFrame) - 1);

		//ͨ��һ��֡���������ļ���ί��A;
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

		//����һ������Ƭ�ڶ����е�ί��;
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

		//��ȫ���һ�����ļ���ί��;
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
			insertFrag(yinTanFrag);
		};

		//������Ӧ�����ļ���ί��;
		auto createAndApply = [&applyNew, &creatNewFile](StYinTanFrame * frame, UINT64 nPos,UINT64 nSize) {
			//���ǹؼ�֡�ṹ,�����½��ļ�;
			if (!frame->IsImportFrame()) {
				return;
			}

			creatNewFile(frame, nPos,nSize);
			applyNew();
		};

		//����һ��֡�ṹ��ί��;
		auto dealWithFrame = [&frags, &createAndApply,&insertFrag,this](StYinTanFrame * frame, UINT64 nPos, UINT64 nSize) {
			UINT32 findVal = frame->GetFrameNum() - 1;
			//������һ���������ڵ�֡;
			auto index = findIndex(frags, getFrameNoFunc, findVal,nullptr, nullptr);
			//�����ڣ��ж�Ϊ������֮ǰ��ĳһ���ļ�;
			if (index != -1) {
				auto min = index;
				auto max = index;

				auto minDisIndex = -1;
				auto minDis = 0xffffffff;

				//��ǰ,��ȡ����������֡��������֡��λ��;
				while (min > 0 && frags[min - 1]->LastFrameNo == findVal) {
					min--;
				}
				while (max < frags.size() - 1 && frags[max + 1]->LastFrameNo == findVal) {
					max++;
				}
				
				//��Ϊ�ؼ�֡,�����ڶ�������е�֡����֡�ŵ�����,�����ɸѡ,��ȡʱ����Ĺؼ�֡��Ϊ׼;;
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
				//����,ֻ����һ��֡��������;
				else if(min == max){
					minDisIndex = index;
				}
				else {
					auto err = 0;
				}

				if (minDisIndex != -1) {
					auto frag = frags[minDisIndex];
					//�������֡��������,��ֱ���ӳ�Size;
					if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
						frag->StFrag->Size += nSize;
						frag->Video->StVideo->Size += nSize;
						frag->LastFrameNo = frame->GetFrameNum();

						//��Ϊ�ؼ�֡,�����ӳ�ʱ��;
						if (frame->IsImportFrame()) {
							frag->LastDateNum = frame->GetDateNum();
							frag->StFrag->EndDate = frame->GetDateNum();
							frag->Video->StVideo->EndDate = frame->GetDateNum();
						}
					}
					//�������һ���µ��ļ���Ƭ�ڵ�;
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
			//�����½�һ���ļ�;
			else {
				createAndApply(frame, nPos,nSize);
				count2++;
			}
		};

		MSetFilePointer(hDisk, startSec * secSize, FILE_BEGIN);

		//�ӻ�������frameLen - 1����ʼд��;
		while ((readLen = MReadFile(hDisk, buffer + sizeof(StYinTanFrame) - 1, bufferLen - sizeof(StYinTanFrame) + 1)) != 0 && pos <= endSec * secSize) {
			//��ѯ������pinʣ�೤���Ժ������;
			for (size_t i = 0; i < readLen; i++) {
				//ȷ����ͷ����������;
				if (*(buffer + i) == YinTanFrameHeader[0]) {
					//ȷ��֡ͷHeader�Ƿ�����;
					if (Mmemcmp(buffer + i + 1, YinTanFrameHeader + 1, headerLen - 1) == 0) {
						//ȷ���Ƿ�Ϊ�ؼ�֡;(YinTanFrameHeader = "0x5410F006")
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
			//����δ�����ļ�λ��ǰ��������������;
			Mmemcpy(buffer, buffer + readLen, sizeof(StYinTanFrame) - 1);
			pos += readLen;
			if (stopped) {
				break;
			}
			
		}

		//�������һ����Ƭ;
		if (lastFound != -1 && pos - lastFound > sizeof(StYinTanFrame) && !stopped) {
			auto lastSec = lastFound / secSize;
			auto dis = lastFound - lastSec * 512;
			MSetFilePointer(hDisk, lastSec * 512, FILE_BEGIN);
			readLen = MReadFile(hDisk, buffer, 1024);
			Mmemcpy(&frame, buffer + dis, sizeof(StYinTanFrame));
			dealWithFrame(&frame, lastFound,pos - lastFound);
		}

		delete buffer;
				
		//�ͷ���ʱ�ڴ�(YinTanFrag,YinTanVideo);
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