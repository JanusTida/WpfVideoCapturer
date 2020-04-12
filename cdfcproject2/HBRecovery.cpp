#include "HBRecovery.h"
#include "Common.h"

//��Ƭ�ṹ(��ʱʹ��);
class HBTempFrag {
public:
	TempVideo * Video;
	UINT8 LastFrameNo1;					//��һ��֡��1;(���Ϊ1)
	UINT8 LastFrameNo2;					//��һ��֡��2;(���Ϊ2)
	UINT32 LastPin1;					//��һ��Pin1;
	UINT32 LastDateNum;					//��һ������;
	StFileFrag* StFrag;
};

UINT64 getFrameNoFunc(HBTempFrag* frag) {
	return frag->LastFrameNo1;
}



class HBSearcher : public NormalDavSearcher {
public:
	HBSearcher()
	{
		get_time = get_unix_time;
	}

	StDateCategory * cdfc_search_start(HANDLE hDisk, int type, int* error) {
		StDateCategory * StDCHeader = nullptr;
		pos = 0;
		StVideo * file = nullptr;
		vector<HBTempFrag *> frags;

		INT64 lastFound = -1;
		StHBFrame frame;

		//ί��A�ĳ���;
		StVideo *stVideo = nullptr;
		TempVideo *wfsVideo = nullptr;
		HBTempFrag *yinTanFrag = nullptr;

		auto val = 2;

		auto headerLen = getArrayLen(HBFrameHeader);		//֡ͷ��ʶ����;

		auto readLen = 0;

		const UINT32 MaxFrameLength = 1048576;

		INT64 bufferLen = 1024 * 1024 * 16 + sizeof(StHBFrame) - 1;		//��������СΪԤ��������С+(pinLen - 1);
		auto buffer = (byte *)Mmalloc(bufferLen);
		Mmemset(buffer + sizeof(StHBFrame) - 1, 0, bufferLen - sizeof(StHBFrame) + 1);				//���䲢����;
																										//Mmemset(buffer, 0,bl);				//���䲢����;

																										//�ҵ���һ���ڴ��в�ƥ����ֽ�;
		auto unMatched = findFirstNotIn(HBFrameHeader, headerLen);
		//ȷ����һ�μ���ʱ����Խ������Ӱ��;
		Mmemset(buffer, unMatched, sizeof(StHBFrame) - 1);

		//ͨ��һ��֡���������ļ���ί��A;
		auto creatNewFile = [&stVideo, &wfsVideo, &yinTanFrag](StHBFrame * frame, UINT64 nPos, UINT64 nSize) {
			auto stFrag = new StFileFrag();
			stFrag->StartDate = frame->GetDateNum();
			stFrag->EndDate = frame->GetDateNum();
			stFrag->StartAddress = nPos;
			stFrag->Size = nSize;

			yinTanFrag = new HBTempFrag();
			yinTanFrag->StFrag = stFrag;
			yinTanFrag->LastFrameNo1 = frame->GetFrameNum();
			yinTanFrag->LastFrameNo2 = frame->GetFrameNum2();
			yinTanFrag->LastDateNum = frame->GetDateNum();
			yinTanFrag->LastPin1 = frame->GetPin1();

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
		auto insertFrag = [&frags](HBTempFrag *frag) {
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

		//��ȫ���һ�����ļ���ί��;
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
		auto createAndApply = [&applyNew, &creatNewFile](StHBFrame * frame, UINT64 nPos, UINT64 nSize) {
			//���ǹؼ�֡�ṹ,�����½��ļ�;
			if (!frame->IsImportFrame()) {
				return;
			}

			creatNewFile(frame, nPos, nSize);
			applyNew();
		};

		//����һ��֡�ṹ��ί��;
		auto dealWithFrame = [&frags, &createAndApply, &insertFrag, this](StHBFrame * frame, UINT64 nPos, UINT64 nSize) {
			UINT32 findVal = frame->GetPreFrameNum1();
			#if _DEBUG
			if (nPos == 237302412) {
				auto sss = 0;
			}
			#endif

			//������һ���������ڵ�֡;(����)
			auto index = findIndex(frags, getFrameNoFunc, findVal, nullptr, nullptr);

			//�����ڣ��ж�Ϊ������֮ǰ��ĳһ���ļ�;
			if (index != -1) {
				auto min = index;
				auto max = index;

				auto minDisIndex = -1;
				auto minDis = 0xffffffff;

				//��ǰ,��ȡ����������֡��������֡��λ��;
				while (min > 0 && frags[min - 1]->LastFrameNo1 == findVal) {
					min--;
				}
				while (max < frags.size() - 1 && frags[max + 1]->LastFrameNo1 == findVal) {
					max++;
				}

				//��Ϊ�ؼ�֡,�����ڶ�������е�֡����֡�ŵ�����,�����ɸѡ,��ȡʱ����Ĺؼ�֡��Ϊ׼;;
				//if (frame->IsImportFrame()) {
				for (size_t i = min; i <= max; i++)
				{
					
					auto frag = frags[i];
					
					if ((frame->CheckIsValidPreFrameNum2(frag->LastFrameNo2)
						|| frag->LastPin1 == BigHBPin1
						|| frame->GetPin1() == BigHBPin1)
						&& frame->GetDateNum() >= frag->StFrag->EndDate
						&& frame->GetDateNum() - frag->StFrag->EndDate < minDis) {
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


				//���ҵ������;
				if (minDisIndex != -1) {
					auto frag = frags[minDisIndex];
					frag->LastFrameNo1 = frame->GetFrameNum();
					frag->LastFrameNo2 = frame->GetFrameNum2();
					frag->LastPin1 = frame->GetPin1();
					frag->LastDateNum = frame->GetDateNum();
					frag->Video->StVideo->EndDate = frame->GetDateNum();
					frag->StFrag->EndDate = frame->GetDateNum();

					frag->Video->StVideo->Size += nSize;

					//�������֡��������,��ֱ���ӳ���Ƭ��Size;
					if (frag->StFrag->StartAddress + frag->StFrag->Size == nPos) {
						frag->StFrag->Size += nSize;
					}
					//�������һ���µ��ļ���Ƭ�ڵ�;
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
				//����,ֻ����һ��֡��������;
				else {
					createAndApply(frame, nPos, nSize);
					count2++;
				}

			}
			//�����½�һ���ļ�;
			else {
				createAndApply(frame, nPos, nSize);
				count2++;
			}
		};

		MSetFilePointer(hDisk, startSec * secSize, FILE_BEGIN);

		//�ӻ�������frameLen - 1����ʼд��;
		while ((readLen = MReadFile(hDisk, buffer + sizeof(StHBFrame) - 1, bufferLen - sizeof(StHBFrame) + 1)) != 0 && pos <= endSec * secSize) {
			
			//��ѯ������pinʣ�೤���Ժ������;
			for (size_t i = 0; i < readLen; i++) {
				//ȷ����ͷ����������;
				if (*(buffer + i) == HBFrameHeader[0]) {
					//ȷ��֡ͷHeader�Ƿ�����;
					if (Mmemcmp(buffer + i + 1, HBFrameHeader + 1, headerLen - 1) == 0) {
						//ȷ���Ƿ�Ϊ�ؼ�֡;(YinTanFrameHeader = "0x5410F006")
						//if (Mmemcmp(buffer + i + 36, YinTanFramePin, pinLen) == 0) {
						auto thisPosition = pos + i - sizeof(StHBFrame) + 1;
						if (lastFound != -1) {
							auto lastLength = thisPosition - lastFound;
							//ȷ����֮ǰ��֡���
							if (lastLength > sizeof(StHBFrame) && lastLength <= MaxFrameLength) {
								if (frame.GetPin1() == NormalHBPin1
									||frame.GetPin1() == BigHBPin1) {

									dealWithFrame(&frame, lastFound, lastLength);

									
								}
							}
						}

						lastFound = thisPosition;
						Mmemcpy(&frame, buffer + i, sizeof(StHBFrame));
						/*if (frame.FrameLength > 6) {
						i += frame.FrameLength - 1;
						}*/
						//}
					}
				}

			}
			//����δ�����ļ�λ��ǰ��������������;
			Mmemcpy(buffer, buffer + readLen, sizeof(StHBFrame) - 1);
			pos += readLen;
			MSetFilePointer(hDisk, pos, FILE_BEGIN);
			if (stopped) {
				break;
			}
			_nFileCount = get_file_count(StDCHeader);
		}


		//�������һ����Ƭ;
		if (lastFound != -1 && pos - lastFound > sizeof(StHBFrame) && !stopped) {
			auto lastSec = lastFound / secSize;
			auto dis = lastFound - lastSec * secSize;
			MSetFilePointer(hDisk, lastSec * secSize, FILE_BEGIN);
			readLen = MReadFile(hDisk, buffer, 1024);
			Mmemcpy(&frame, buffer + dis, sizeof(StHBFrame));
			dealWithFrame(&frame, lastFound, pos - lastFound);
			
		}

		_nFileCount = get_file_count(StDCHeader);

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
auto searcher = new HBSearcher();

StDateCategory* hb_searchstart(HANDLE hDisk, eSearchType eType, unsigned __int64 nStartSec, unsigned __int64 nEndSec,
	int nSecSize, unsigned __int64 nAreaSize, int nClusterSize, unsigned __int64 nLBAPos, bool bJournal, int *nError) {
	searcher->cdfc_init(nStartSec, nEndSec, nSecSize, 0, nLBAPos, hDisk);
	if (eType == eSearchType_FULL) {
		return searcher->cdfc_search_start(hDisk, 0, nError);
	}
	return nullptr;
}

bool hb_recover(HANDLE hDisk, eSearchType eType, StVideo *stFile, HANDLE hFileOut, unsigned __int64 *nCurrSizeDW, int *nError) {
	return searcher->cdfc_filesave(stFile, hDisk, hFileOut, nCurrSizeDW, nError);
}

void hb_exit(StDateCategory *stFile) {
	searcher->cdfc_exit( stFile);
}

void hb_stop() {
	searcher->cdfc_stop();
}

void hb_get_date(unsigned __int64 *nOffsetSec, unsigned __int64 *nFileCount) {
	searcher->get_data(nOffsetSec, nFileCount);
}

bool hb_preview(StVideo *szFile, eSearchType eType, HANDLE hDisk, byte *szBuffer, unsigned __int64 nBuffSize) {
	return searcher->cdfc_readbuffer(szFile, hDisk, szBuffer, nBuffSize);
}

void hb_date_converter(UINT32 nDate, int8_t *date) {
	searcher->cdfc_date_converter(nDate, (UINT8 *)date);
}