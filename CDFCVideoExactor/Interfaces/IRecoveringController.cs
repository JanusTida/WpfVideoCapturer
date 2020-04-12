namespace CDFCVideoExactor.Interfaces {
    //恢复文件的控制器接口
    public interface IRecoveringController {
        void Start();
        string RecoveringPath { get; }
        bool IsRecovering { get; }
    }
}
