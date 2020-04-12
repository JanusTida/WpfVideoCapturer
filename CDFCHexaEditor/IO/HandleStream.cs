using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EventLogger;

namespace WPFHexaEditor.Control.IO {
    public class ObjectDeviceStream : Stream {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_common_read")]
        private extern static bool cdfc_common_read(IntPtr hDisk, ulong nPos, IntPtr szBuffer, ulong nSize, IntPtr nDwSize, bool Position);
        [DllImport("kernel32.dll",SetLastError =true)]
        private extern static bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, IntPtr lpNumberOfBytesRead, IntPtr lpOverlapped);
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SetFilePointer(IntPtr handle, int lDistanceToMove, out int lpDistanceToMoveHigh, uint dwMoveMethod);
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        public IObjectDevice ObjectDevice { get; private set; }
        private long Seek(IntPtr handle, long offset, SeekOrigin origin) {
            uint moveMethod = 0;

            switch (origin) {
                case SeekOrigin.Begin:
                    moveMethod = 0;
                    break;

                case SeekOrigin.Current:
                    moveMethod = 1;
                    break;

                case SeekOrigin.End:
                    moveMethod = 2;
                    break;
            }

            int lo = (int)(offset & 0xffffffff);
            int hi = (int)(offset >> 32);

            lo = SetFilePointer(handle, lo, out hi, moveMethod);

            if (lo == -1) {
                var s = GetLastError();
                if (GetLastError() != 0) {
                    Logger.WriteLine($"{nameof(ObjectDeviceStream)}:{nameof(Seek)}->:Failed to set file pointer!");
                    return 0;
                    //throw new Exception("INVALID_SET_FILE_POINTER");
                }
            }

            return (((long)hi << 32) | (uint)lo);
        }
        public ObjectDeviceStream(IObjectDevice objectDevice,long length, int sectorSize = 512, long startPos = 0) {
            this.ObjectDevice = objectDevice;
            this.length = length;
            this.SectorSize = sectorSize;
            this.startPos = startPos;
        }
        
        public int SectorSize { get; private set; }
        public override bool CanRead {
            get {
                return true;
            }
        }

        public override bool CanSeek {
            get {
                return true;
            }
        }

        public override bool CanWrite {
            get {
                return false;
            }
        }

        private long length;
        public override long Length {
            get {
                return length;
            }
        }

        private long startPos;
        private long position = 0;
        public override long Position {
            get {
                return position;
            }

            set {
                //if(value > length) {
                //    throw new ArgumentOutOfRangeException(nameof(Position));
                //}
                position = value;
            }
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 在对象中读取某一个位置;
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="index"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public override int Read(byte[] destination, int offset, int byteCount) {
            return ObjectDevice.Read(destination, offset, byteCount,(long) Position + (long) startPos);
            ////获取能够被扇区数目整除的字节数;
            //var byteCountForSector = byteCount % SectorSize == 0 ? byteCount : byteCount / SectorSize * SectorSize + SectorSize;
            //var PositionForSector = Position % SectorSize == 0 ? Position : Position / SectorSize * SectorSize;
            ////两个位置的差;
            //var subForPos = (int)(Position - PositionForSector);
            ////临时缓冲区,以方便合法调用;
            //byte[] bufferByteArray = null;

            //if (PositionForSector + byteCountForSector < byteCount + Position) {
            //    byteCountForSector += SectorSize;
            //}
            //bufferByteArray = new byte[byteCountForSector];

            //IntPtr ptrSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            //IntPtr ptrBuffer = Marshal.AllocHGlobal(byteCountForSector * Marshal.SizeOf(typeof(byte)));

            //int readSize = 0;
            //bool res = false;

            //Func<IntPtr, ulong, IntPtr, ulong, IntPtr, bool, bool> readFunc = cdfc_common_read;
            //Seek(Handle, PositionForSector, SeekOrigin.Begin);
            //res = ReadFile(Handle, ptrBuffer,(uint) byteCountForSector, ptrSize, IntPtr.Zero);
            //    //readFunc(Handle, (ulong)PositionForSector, ptrBuffer, (ulong)byteCountForSector, ptrSize, true);

            //if (res) {
            //    readSize = Marshal.ReadInt32(ptrSize);
            //    Marshal.Copy(ptrBuffer, bufferByteArray, 0, byteCountForSector);
            //    for (int i = subForPos; i < byteCount + subForPos; i++) {
            //        destination[offset + i - subForPos] = bufferByteArray[i];
            //    }
            //}
            //else {
            //    readSize = Marshal.ReadInt32(ptrSize);
            //}
            //Marshal.FreeHGlobal(ptrBuffer);
            //Marshal.FreeHGlobal(ptrSize);

            //Position += byteCount;
            //return readSize;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }
    }
}
