using CDFCPreviewer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EventLogger;

namespace CDFCPreviewer.Components {
    public partial class DHBufferPlayer : IBufferPlayer {
        private int? port;
        private IntPtr bufferPtr;
        private IntPtr handle;

        public DHBufferPlayer(IntPtr bufferPtr, IntPtr handle,long bufferSize) {
            if (bufferPtr != IntPtr.Zero) {
                this.bufferPtr = bufferPtr;
                this.BufferSize = bufferSize;
                this.handle = handle;
            }
        }

        public long BufferSize { get; }
        public bool Pause() {
            return PLAY_Pause(port.Value, 1);
        }
        [HandleProcessCorruptedStateExceptions]
        private bool Init() {
            try {
                if (port != null) {
                    PLAY_Stop(port.Value);
                    PLAY_CloseStream(port.Value);
                }
                var lpPort = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(lpPort, 0);
                PLAY_GetFreePort(lpPort);
                port = Marshal.ReadInt32(lpPort);
                var s = PLAY_OpenStream(port.Value, IntPtr.Zero, 0, 900 * 1024);
                s = PLAY_Play(port.Value, this.handle);
                s = PLAY_InputData(port.Value, bufferPtr,(uint) BufferSize);
                return s;
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(DHBufferPlayer)}->{nameof(Init)}:{ex.Message}");
                return false;
            }
            
        }

        public bool Play() {
            if (bufferPtr != IntPtr.Zero) {
                if(port == null) {
                    try {
                        return Init();
                    }
                    catch {
                        throw;
                    }
                }
            }
            return false;
        }
        public bool PlayOne() {
            if(bufferPtr != IntPtr.Zero) {
                if(port == null) {
                    Init();
                }
                return PLAY_OneByOne(port.Value);
            }
            return false;
        }
        public bool Resume() {
            return PLAY_Pause(port.Value, 0);
        }
        public bool Stop() {
            if (bufferPtr != IntPtr.Zero && port != null) {
                PLAY_Stop(port.Value);
                var val = PLAY_CloseStream(port.Value);
                bufferPtr = IntPtr.Zero;
                port = null;
                return val;
            }
            return false;
        }
    }

    public partial class DHBufferPlayer {
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_GetFreePort(IntPtr lpPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Stop(int port);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.ThisCall)]
        private static extern bool PLAY_GetPicBMP(int port, IntPtr pByte, ulong bufSize, IntPtr mapSize);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Play(int port, IntPtr Handle);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OpenFile(int port, [MarshalAs(UnmanagedType.LPStr)]string fileName);
        //"CDFCSnapshoter.dll",EntryPoint = "DH_PLAY_OpenStream",
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OpenStream(int port, IntPtr pByte, uint size, uint poolSize);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_InputData(int port, IntPtr pByte, uint size);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetPlayedTimeEx(int nPort, uint nTime);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetPlayedTimeEx(int port);
        [DllImport("CDFCSnapshoter.dll", EntryPoint = "DH_PLAY_GetLastError", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint PLAY_GetLastError(int port);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_GetPicJPEG(int port, IntPtr pByte, uint bufSize, IntPtr mapSize, int quality);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Pause(int nPort, uint nPause);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Fast(int nPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_Slow(int nPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_CloseStream(int nPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_SetPlaySpeed(int nPort, float fCoff);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool PLAY_OneByOne(int nPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern uint PLAY_GetCurrentFrameRate(int nPort);
        [DllImport("DHPreviewer/dhplay.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern float PLAY_GetCurrentFrameRateEx(int nPort);
    }
}
