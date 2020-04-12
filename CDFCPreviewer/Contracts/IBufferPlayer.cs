using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCPreviewer.Contracts {
    //内存播放器的契约;
    public interface IBufferPlayer {
        bool Play();                        //播放接口;
        bool Pause();                       //暂停接口;
        bool Resume();                      //继续接口;
        bool Stop();                        //停止接口;
    }
}
