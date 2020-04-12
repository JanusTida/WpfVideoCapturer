#include "DH_NVR_Recovery.h"
#include "Common.h"
#include "IO.h"
#include "Common_Recovery.h"
using namespace std;

//��NVR��Ƭ�ṹ(��ʱʹ��);
class DHNVRFrag {
public:
	TempVideo* Video;
	UINT32 LastFrameNo;					//��һ��֡��;
	UINT32 LastDateNum;					//��һ������;
	UINT16 ChannelNo;					//ͨ����;
	StFileFrag* StFrag;
};

UINT64 getFrameNumFunc(DHNVRFrag* frag) {
	return frag->LastFrameNo;
}

class DHNVRSearcher : public NormalDavSearcher {
public:
	StDateCategory* cdfc_search_start(HANDLE hDisk, int type, int* error) {
		StVideo * file = nullptr;
		vector<DHNVRFrag *> frags;
		StDateCategory * StDCHeader = nullptr;

		INT64 lastFound = -1;
		StDHNVRFrame frame;

		//ί��A�ĳ���;
		StVideo *stVideo = nullptr;
		TempVideo *tempVideo = nullptr;
		DHNVRFrag *dhNVRFrag = nullptr;

		auto val = 2;

		auto headerLen = getArrayLen(DaHuaFrameHeader);		//֡ͷ��ʶ����;
		
		auto readLen = 0;

		INT64 bufferLen = 1024 * 1024 * 16 + sizeof(StDHNVRFrame) - 1;		//��������СΪԤ��������С+(pinLen - 1);
		auto buffer = (byte *)Mmalloc(bufferLen);
		Mmemset(buffer + sizeof(StDHNVRFrame) - 1, 0, bufferLen - sizeof(StDHNVRFrame) + 1);				//���䲢����;
											
		
		//ȷ����һ�μ���ʱ����Խ������Ӱ��;
		auto unMatched = findFirstNotIn(DaHuaFrameHeader, headerLen);
		Mmemset(buffer, unMatched, sizeof(StDHNVRFrame) - 1);


		//ͨ��һ��֡���������ļ���ί��A;
		auto creatNewFile = [&stVideo, &tempVideo, &dhNVRFrag](StDHNVRFrame * frame, UINT64 nPos, UINT64 nSize) {
			auto stFrag = new StFileFrag();
			stFrag->StartDate = frame->GetDateNum();
			stFrag->EndDate = frame->GetDateNum();
			stFrag->StartAddress = nPos;
			stFrag->Size = nSize;
			stFrag->ChannelNO = frame->GetChannelNo();

			dhNVRFrag = new DHNVRFrag();
			dhNVRFrag->StFrag = stFrag;
			dhNVRFrag->LastFrameNo = frame->GetFrameNum();
			dhNVRFrag->LastDateNum = frame->GetDateNum();
			dhNVRFrag->ChannelNo = frame->GetChannelNo();

			stVideo = new StVideo();
			stVideo->StartDate = frame->GetDateNum();
			stVideo->EndDate = frame->GetDateNum();
			stVideo->StartAddress = nPos;
			stVideo->Size = nSize;
			stVideo->stStAdd = stFrag;
			stVideo->ChannelNO = frame->GetChannelNo();

			tempVideo = new TempVideo();
			tempVideo->StVideo = stVideo;

			dhNVRFrag->Video = tempVideo;

		};

		//����һ������Ƭ�ڶ����е�ί��;
		auto insertFrag = [&frags](DHNVRFrag *frag) {
			auto left = 0, right = 0;
			auto index = findIndex(frags, getFrameNumFunc, frag->LastFrameNo, &left, &right);
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
		auto applyNew = [&frags, &stVideo, &tempVideo, &dhNVRFrag, &insertFrag, this,&StDCHeader]() {
			if (StDCHeader == nullptr) {
				StDCHeader = new StDateCategory();
				StDCHeader->Date = stVideo->StartDate;
				StDCHeader->File = stVideo;

				tempVideo->StDTGory = StDCHeader;
			}
			else {
				auto dc = StDCHeader;
				set_com_tm(get_time(stVideo->StartDate));
				set_com_func(get_time);
				
				//Ѱ��ͬһ��ķ���ڵ�;
				auto preDc = findFirst(dc, get_next_category, is_cate_same_day);
				//���ҵ�������ԭ�ڵ�������;
				if (preDc != nullptr) {
					auto file = preDc->File;
					auto lastFile = getLast(file, get_next_video);

					tempVideo->StDTGory = preDc;

					lastFile->Next = stVideo;
				}
				//���򴴽�һ���ڵ�;
				else {
					auto newDc = new StDateCategory();
					newDc->Date = stVideo->StartDate;
					newDc->File = stVideo;

					tempVideo->StDTGory = newDc;

					auto lastDc = getLast(dc, get_next_category);
					lastDc->Next = newDc;
				}
			}
			insertFrag(dhNVRFrag);
		};

		//������Ӧ�����ļ���ί��;
		auto createAndApply = [&applyNew, &creatNewFile](StDHNVRFrame * frame, UINT64 nPos, UINT64 nSize) {
			//���ǹؼ�֡�ṹ,�����½��ļ�;
			if (!frame->IsImportFrame()) {
				return;
			}

			creatNewFile(frame, nPos, nSize);
			applyNew();
		};

		//����һ��֡�ṹ��ί��;
		auto dealWithFrame = [&frags, &createAndApply, &insertFrag, this](StDHNVRFrame * frame, UINT64 nPos, UINT64 nSize) {
			UINT32 findVal = frame->GetFrameNum() - 1;
			//������һ���������ڵ�֡;
			auto index = findIndex(frags, getFrameNumFunc, findVal, nullptr, nullptr);
			if (nPos / 512 == 328782) {
				auto sss = 0;
			}

			//�����ڣ��ж�Ϊ������֮ǰ��ĳһ���ļ�;
			if (index != -1) {
				auto min = index;
				auto max = index;

				auto foundIndex = -1;
				auto minDis = 0xffffffff;

				//��ǰ,��ȡ����������֡��������֡��λ��;
				while (min > 0 && getFrameNumFunc( frags[min - 1] ) == findVal) {
					min--;
				}
				while (max < frags.size() - 1 && getFrameNumFunc(frags[max + 1]) == findVal) {
					max++;
				}

				//��Ϊ�ؼ�֡,�����ڶ�������е�֡����֡�ŵ�����,�����ɸѡ,��ͨ������ͬ,��ʱ������Ĺؼ�֡��Ϊ׼;
				if (min != max) {
					auto s = 0;
				}
				if (frame->IsImportFrame()) {
					for (size_t i = min; i <= max; i++)
					{
						if (frags[i]->ChannelNo == frame->GetChannelNo() 
							&& frags[i]->LastFrameNo < frame->GetFrameNum()
							&& (frame->GetFrameNum() - frags[i]->LastFrameNo) < minDis) {
							foundIndex = i;
							minDis = frame->GetDateNum() - frags[i]->StFrag->EndDate;
						}
					}
				}
				else if (min == max) {
					foundIndex = index;
				}
				else {
					for (size_t i = min; i <= max; i++)
					{
						if (frags[i]->LastFrameNo < frame->GetFrameNum()
							&& (frame->GetFrameNum() - frags[i]->LastFrameNo) < minDis) {
							foundIndex = i;
							minDis = frame->GetDateNum() - frags[i]->StFrag->EndDate;
						}
					}
					
					auto err = 0;
				}
				

				if (foundIndex != -1) {
					auto frag = frags[foundIndex];
					//�������֡��������,��ֱ���ӳ�Size;
					if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
						frag->StFrag->Size += nSize;
						frag->Video->StVideo->Size += nSize;
						frag->LastFrameNo = frame->GetFrameNum();

						//��Ϊ�ؼ�֡,�����ӳ�ʱ��;
						//if (frame->IsImportFrame()) {
							frag->LastDateNum = frame->GetDateNum();
							frag->StFrag->EndDate = frame->GetDateNum();
							frag->Video->StVideo->EndDate = frame->GetDateNum();
						//}
					}
					//�������һ���µ��ļ���Ƭ�ڵ�;
					else {
						frag->LastFrameNo = frame->GetFrameNum();

						auto stFrag = new StFileFrag();

						stFrag->StartAddress = nPos;
						stFrag->Size = nSize;

						frag->StFrag->Next = stFrag;

						stFrag->StartDate = frame->GetDateNum();
						stFrag->EndDate = frame->GetDateNum();
						frag->Video->StVideo->EndDate = frame->GetDateNum();

						frag->Video->StVideo->Size += nSize;

						frag->StFrag = stFrag;
					}
					frags.erase(frags.begin() + foundIndex);
					insertFrag(frag);
				}
				else if(frame->IsImportFrame()){
					createAndApply(frame, nPos, nSize);
				}
				else {
					auto err = 0;
				}
			}
			//������Ϊ�ؼ�֡,�½�һ���ļ�;
			else if(frame ->IsImportFrame()) {
				createAndApply(frame, nPos, nSize);
				count2++;
			}
		};

		MSetFilePointer(hDisk, startSec * secSize, FILE_BEGIN);

		//�ӻ�������frameLen - 1����ʼд��;
		while ((readLen = MReadFile(hDisk, buffer + sizeof(StDHNVRFrame) - 1, bufferLen - sizeof(StDHNVRFrame) + 1)) != 0 && pos <= endSec * secSize) {
			//��ѯ������pinʣ�೤���Ժ������;
			for (size_t i = 0; i < readLen; i++) {
				//ȷ����ͷ����������;
				if (*(buffer + i) == DaHuaFrameHeader[0]) {
					//ȷ��֡ͷHeader�Ƿ�����;
					if (Mmemcmp(buffer + i + 1, DaHuaFrameHeader + 1, headerLen - 1) == 0) {
						if (buffer[i + headerLen] == DHNVRImportFramePin
							|| buffer[i + headerLen] == DHNVRNormalFramePin) {
							if (lastFound != -1) {
								auto lastLength = pos + i - sizeof(StDHNVRFrame) + 1 - lastFound;
								if (lastLength > sizeof(StDHNVRFrame)) {
									dealWithFrame(&frame, lastFound, lastLength);
								}
							}
							lastFound = pos + i - sizeof(StDHNVRFrame) + 1;
							Mmemcpy(&frame, buffer + i, sizeof(StDHNVRFrame));
							i += sizeof(StDHNVRFrame);
						}
					}
				}

			}
			//����δ�����ļ�λ��ǰ��������������;
			Mmemcpy(buffer, buffer + readLen, sizeof(StDHNVRFrame) - 1);
			pos += readLen;
			if (stopped) {
				break;
			}

		}

		//�������һ����Ƭ;
		if (lastFound != -1 && pos - lastFound > sizeof(StDHNVRFrame) && !stopped) {
			auto lastSec = lastFound / secSize;
			auto dis = lastFound - lastSec * 512;
			MSetFilePointer(hDisk, lastSec * 512, FILE_BEGIN);
			readLen = MReadFile(hDisk, buffer, 1024);
			Mmemcpy(&frame, buffer + dis, sizeof(StDHNVRFrame));
			dealWithFrame(&frame, lastFound, pos - lastFound);
		}

		delete buffer;

		//�ͷ���ʱ�ڴ�(DHNVRFrag,YinTanVideo);
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

DHNVRSearcher* searcher = new DHNVRSearcher();

bool cdfc_dh_nvr_init(UINT64 nStartSec, UINT64 nEndSec, int nSecSize,
	UINT64 nTimePos, UINT64 nLBAPos, HANDLE hDisk) {
	return searcher->cdfc_init(nStartSec, nEndSec, nSecSize, nTimePos, nLBAPos, hDisk);

}

StDateCategory* cdfc_dh_nvr_search_start(HANDLE handle, int type, int* error) {
	return searcher->cdfc_search_start(handle, type, error);
}

bool cdfc_dh_nvr_filesave(StVideo * szFile, HANDLE hDisk, HANDLE saveFileHandle, UINT64 * nCurrSizeDW, int* nError) {
	return searcher->cdfc_filesave(szFile, hDisk, saveFileHandle, nCurrSizeDW, nError);
}

void cdfc_dh_nvr_exit() {
	searcher->cdfc_exit(nullptr);
}

StDateCategory* cdfc_dh_nvr_filelist() {
	return searcher->cdfc_filelist();
}

void cdfc_dh_nvr_stop() {
	searcher->cdfc_stop();
}

void cdfc_dh_nvr_set_regionsize(UINT64 regionSize) {
	searcher->cdfc_set_regionsize(regionSize);
}

bool cdfc_dh_nvr_readbuffer(StVideo* szFile, HANDLE hDisk, byte* szBuffer, UINT64 nBuffSize) {
	return searcher->cdfc_readbuffer(szFile, hDisk, szBuffer, nBuffSize);
}

UINT64 cdfc_dh_nvr_current_sector() {
	return searcher->cdfc_current_sector();
}

void cdfc_dh_nvr_date_converter(UINT64 date, UINT8* resDate) {
	return searcher->cdfc_date_converter(date, resDate);
}

void cdfc_dh_nvr_set_preview(UINT64 regionSize) {
	return searcher->cdfc_set_regionsize(regionSize);
}