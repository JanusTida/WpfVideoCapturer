using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Interfaces {
    //预览播放器接口;
    public interface IPreviewPlayer {
        void Play();
        void Pause();
        void Fast();
        void Slow();
        void SetSpeed();
        void Stop();
        void Free();
    }
}
