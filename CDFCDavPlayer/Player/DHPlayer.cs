using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;
using EventLogger;

namespace CDFCDavPlayer.Player {
    public interface IPlayer:IDisposable {
        bool Play();                        //播放接口;
        bool Pause();                       //暂停接口;
        bool Resume();                      //继续接口;
        bool Stop();                        //停止接口;
        bool EscapeToTimeSpan(TimeSpan ts); //跳转指令(ts目标进度);
        TimeSpan? CurrentTimeSpan { get; }      //当前进度;
        TimeSpan? TotalTimeSpan { get; }        //总长度;
        ImageSource GetImageSource(TimeSpan ts);    //获得某个时刻的图片流;
        string FileName { get; }
        bool SetVolume(uint val);                //设定音量大小;满值为100;
    }


    /// <summary>
    /// 大华播放器契约;
    /// </summary>
    public partial class DHPlayer : IPlayer {
        public IntPtr PlayerHandle { get; set; }
        public int Port { get; }
        /// <summary>
        /// 大华播放器构造方法;
        /// </summary>
        /// <param name="davName">dav文件名</param>
        public DHPlayer(string fileName) {
            var portPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            Marshal.WriteInt32(portPtr,-1);
            try {
                var succeed = PLAY_GetFreePort(portPtr);
                if (succeed) {
                    this.Port = Marshal.ReadInt32(portPtr);
                    this.FileName = fileName;
                }
                else {
                    throw new Exception($"{nameof(DHPlayer)}:" + "Failed to get freeport");
                }
            }
            catch(Exception ex) {
                throw new Exception($"{nameof(DHPlayer)} Unknown Error:{ex.Message}");
            }
            finally {
                Marshal.FreeHGlobal(portPtr);
            }
        }

        public string FileName { get; }

        public TimeSpan? CurrentTimeSpan {
            get {
                try {
                    var tsPlayed = PLAY_GetPlayedTimeEx(Port);
                    return TimeSpan.FromMilliseconds(tsPlayed);
                }
                catch {
                    return null;
                }
            }
        }

        public TimeSpan? TotalTimeSpan {
            get {
                try {
                    var s = PLAY_GetFileTotalFrames(Port);
                    return TimeSpan.FromSeconds(PLAY_GetFileTime(Port));
                }
                catch(Exception ex) {
                    Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(TotalTimeSpan)}:{ex.Message}");
                    return null;
                }
            }
        }

        public bool EscapeToTimeSpan(TimeSpan ts) {
            try {
                return PLAY_SetPlayedTimeEx(Port, (uint) ts.TotalMilliseconds);
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(EscapeToTimeSpan)}:{ex.Message}");
                return false;
            }
        }

        public bool Pause() {
            return PLAY_Pause(Port, 1);
        }

        public bool Play() {
            if(PlayerHandle == null) {
                return false;
            }
            if (!Init()) {
                return false;
            }

            try {
                var s = PLAY_PlaySound(Port);
                s = PLAY_Play(Port, PlayerHandle);
                return s;
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(Play)}:{ex.Message}");
                return false;
            }
        }

        public bool Resume() {
            try {
                return PLAY_Pause(Port, 0);
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(Resume)}:{ex.Message}");
                return false;
            }
        }

        public bool Stop() {
            try {
                return PLAY_Stop(Port);
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(Stop)}:{ex.Message}");
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private bool Init() {
            return PLAY_OpenFile(Port, FileName);
        }
        
        public void Dispose() {
            PLAY_Stop(Port);
            PLAY_CloseFile(Port);
        }

        /// <summary>
        /// 得到特定时刻的图像流;
        /// </summary>
        /// <param name="ts">指定的时刻</param>
        /// <returns></returns>
        public ImageSource GetImageSource(TimeSpan ts) {
            if (TotalTimeSpan == null || TotalTimeSpan.Value < ts) {
                return null;
            }

            try {
                var snapPath = $"{AppDomain.CurrentDomain.BaseDirectory}{SnapShoterPath}";
                var tmpDirec = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Tmp/";
                if (!Directory.Exists(tmpDirec)) {
                    Directory.CreateDirectory(tmpDirec);
                }
                var outFile = Path.GetRandomFileName();

                var stInfo = new ProcessStartInfo {
                    FileName = snapPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"-i \"{FileName}\" -y -ss 00:00:01 -t 00:00:03  -s  320*320  -f mjpeg -vframes 10 \"{tmpDirec}{outFile}\""
                };
                var pro = Process.Start(stInfo);
                pro.WaitForExit();
                if (File.Exists($"{tmpDirec}{outFile}")) {
                    using (var ms = File.OpenRead($"{tmpDirec}{outFile}")) {
                        var bitMapImage2 = new BitmapImage();
                        bitMapImage2.BeginInit();
                        bitMapImage2.CacheOption = BitmapCacheOption.OnLoad;
                        bitMapImage2.StreamSource = ms;
                        bitMapImage2.DecodePixelWidth = 400;
                        bitMapImage2.EndInit();
                        bitMapImage2.Freeze();
                        return bitMapImage2;
                    }
                }
            }
            catch(Exception ex) {

            }
            return null;
        }

        private const uint MaxVolume = 0xffff;
        public bool SetVolume(uint val) {
            try {
                return PLAY_SetVolume(Port,MaxVolume * val / 100);
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(DHPlayer)}->{nameof(SetVolume)}:{ex.Message}");
                return false;
            }
        }
    }
    public partial class DHPlayer {
        private const string SnapShoterPath = "FFPlayer/cdfcplayer2.exe";
        private const string DhPath = "dhplay.dll";
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetFileTotalFrames(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_GetFreePort(IntPtr lpPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetPlayedTime(int nPort, uint nTime);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetVolume(int nPort,uint nVolume);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Stop(int port);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetFileTime(int port);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.ThisCall)]
        private static extern bool PLAY_GetPicBMP(int port, IntPtr pByte, ulong bufSize, IntPtr mapSize);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Play(int port, IntPtr Handle);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OpenFile(int port, [MarshalAs(UnmanagedType.LPStr)]string fileName);
        //"CDFCSnapshoter.dll",EntryPoint = "DH_PLAY_OpenStream",
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OpenStream(int port, IntPtr pByte, uint size, uint poolSize);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_InputData(int port, IntPtr pByte, uint size);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_PlaySound(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetPlayedTimeEx(int nPort, uint nTime);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetPlayedTimeEx(int port);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetLastError(int port);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_GetPicJPEG(int port, IntPtr pByte, uint bufSize, IntPtr mapSize, int quality);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_GetPictureSize(int nPort,IntPtr pWidth,IntPtr pHeight);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Pause(int nPort, uint nPause);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Fast(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Slow(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_CloseStream(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_CloseFile(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetPlaySpeed(int nPort, float fCoff);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OneByOne(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetCurrentFrameRate(int nPort);
        [DllImport(DhPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern float PLAY_GetCurrentFrameRateEx(int nPort);
    }
}
