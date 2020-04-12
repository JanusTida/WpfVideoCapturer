#include "Common_Recovery.h"
#include "IO.h"
#include <atltime.h>

const UINT32 DHHexNum[]{
	67108864,4194304,131072,4096,64,1
};



UINT64 get_file_count(StDateCategory *cate) {
	UINT64 fCount = 0;
	auto cate2 = cate;
	while (cate2 != nullptr) {
		auto fileNode = cate2->File;
		while (fileNode != nullptr) {
			fCount++;
			fileNode = fileNode->Next;
		}
		cate2 = cate2->Next;
	}
	return fCount;
}

/// <summary>
/// 获得大华的时间
/// </summary>
tm get_dh_time(UINT32 dateNum) {
	short dateNums[6];
	tm dt;

	for (byte index = 0; index < 6; index++) {
		int innerDateNum = dateNum;
		for (byte innerIndex = 0; innerIndex < index; innerIndex++) {
			innerDateNum %= DHHexNum[innerIndex];
		}
		if (innerDateNum != 0) {
			dateNums[index] = innerDateNum / DHHexNum[index];
		}
	}

	//大华起始时间为2000年初始时间;
	dt.tm_year = dateNums[0] + 2000;
	dt.tm_mon = dateNums[1];
	dt.tm_mday = dateNums[2];
	dt.tm_hour = dateNums[3];
	dt.tm_min = dateNums[4];
	dt.tm_sec = dateNums[5];
	//dt = new DateTime(, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
	return dt;
}

//获得Unix时间;从1970/01/01 00:00:00起始;
tm get_unix_time(UINT32 dateNum) {
	auto ct = new CTime(dateNum - 8 * 3600);
	tm dt;
	dt.tm_year = ct->GetYear();
	dt.tm_mon = ct->GetMonth();
	dt.tm_mday = ct->GetDay();
	dt.tm_hour = ct->GetHour();
	dt.tm_min = ct->GetMinute();
	dt.tm_sec = ct->GetSecond();
	return dt;
}

//静态取时委托;
tm (*GetTime)(UINT32);

//设定取时委托;
void set_com_func(tm (*getTime)(UINT32)) {
	GetTime = getTime;
}

//对比时间;
tm ComTm;
//设定对比时间;
void set_com_tm(tm comTm) {
	ComTm = comTm;
}

//是否是同一天;
bool is_cate_same_day(StDateCategory* cate) {
	if (GetTime == nullptr) {
		return false;
	}

	auto t_m2 = GetTime(cate->Date);

	bool res = t_m2.tm_year == ComTm.tm_year
			&& t_m2.tm_mday == ComTm.tm_mday
			&& t_m2.tm_mon  == ComTm.tm_mon;
	
	return res;
}

StDateCategory* get_next_category(StDateCategory * cate) {
	return cate->Next;
}
StVideo* get_next_video(StVideo * cate) {
	return cate->Next;
}

eSearchType convert_type_to_searchtype(int type) {
	/*eSearchType_FULL,
		eSearchType_FREE,
		eSearchType_FS,
		eSearchType_QUICK*/
	switch (type) {
		case 0:
			return eSearchType_FULL;
		case 1:
			return eSearchType_FS;
		case 3:
			return eSearchType_QUICK;
		case 4:
			eSearchType_FREE;
	}

	return eSearchType_FULL;
}

//从标准时间转化为UINT8数组;
void convert_tm_to_uint(tm tm, UINT8* resDate) {
	if (resDate == nullptr) {
		printf_s("res Date can't be null.");
		return;
	}

	resDate[0] = tm.tm_year - 2000;
	resDate[1] = tm.tm_mon;
	resDate[2] = tm.tm_mday;
	resDate[3] = tm.tm_hour;
	resDate[4] = tm.tm_min;
	resDate[5] = tm.tm_sec;
}

