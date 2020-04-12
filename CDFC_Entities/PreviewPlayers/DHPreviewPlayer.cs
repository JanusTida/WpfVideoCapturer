using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.PreviewPlayers {
    public partial class DHPreviewPlayer {
        [DllImport("CDFCSnapshoter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DH_MemPLAY_Init(uint port, IntPtr containerHandle, IntPtr stream, uint streamSize);
        [DllImport("CDFCSnapshoter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DH_MemPLAY_SnapAt(uint nPort, uint milliSeconds, IntPtr bmpBuffer, uint bufferSize, IntPtr actualSize, IntPtr error);
        [DllImport("CDFCSnapshoter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DH_MemPLAY_PLAY(uint nPort, IntPtr error);
        [DllImport("CDFCSnapshoter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DH_MemPLAY_Free(uint nPort);
        [DllImport("CDFCSnapshoter.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DH_MemPLAY_Stop(uint nPort, IntPtr error);

    }
    public partial class DHPreviewPlayer {
        public DHPreviewPlayer(IntPtr buffer) {

        }
    }
}
