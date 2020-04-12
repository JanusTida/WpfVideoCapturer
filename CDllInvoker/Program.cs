using CDFCEntities.DeviceObjects;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using EventLogger;
using System.Diagnostics;
using System.Linq;
using CDFCEntities.Structs;
using System.Threading;
using CDFCStatic.CMethods;
using CDFCEntities.Files;
using CDFC.Util.PInvoke;
using Win32;
using Microsoft.Win32.SafeHandles;

namespace CDFCEntities {
    
    class Program {
        #region decomplier
        //var imgFile = ImgFile.GetImgFile("D://DHTest2.img");

        //var scanner = new Scanners.DHScanner(imgFile);

        ////FileStream fs = new FileStream("D://DHTest2.img", FileMode.Open);
        //ComObjectMethods.cdfc_devices_init();
        //ComObjectMethods.cdfc_devices_devicelist();
        //ComObjectMethods.cdfc_devices_patitionlist();
        //scanner.Init(Enums.ScanMethod.FileSystem, 0, imgFile.Size / 512, 512, 0, 0);
        ////scanner.SearchStart(2);
        ////var cs = scanner.CurCategories;
        ////scanner.Dispose();
        ////DHScanMethods.cdfc_object_init(0, imgFile.Size / 512, 512, 0, 0, imgFile.Handle);

        ////ThreadPool.QueueUserWorkItem(callBack => {
        //scanner.SearchStart(1);
        //var categeries = scanner.CurCategories;
        //StreamWriter sw = new StreamWriter("D://Test.txt");
        //categeries.ForEach(p => {
        //    p.Videos.ForEach(q => {
        //        sw.WriteLine("StartAddress:"+q.StartAddress.ToString("X8")+"/tStartDate:"+q.StartDate.ToString("X8") + "/tEndDate:" + q.EndDate.ToString("X8") + "/tChannelNo:"+q.ChannelNO+"/tSize:"+q.Size.ToString("X8"));
        //        sw.WriteLine("Frags:");
        //        q.FileFragments.ForEach(t => {
        //            sw.WriteLine("StartAddress"+t.StartAddress.ToString("X8")+"/tStartDate"+ t.StartDate.ToString("X8") + "/tEndDate:" + t.EndDate.ToString("X8")+ "/tChannelNo:"+t.ChannelNO+"/tSize:" +t.Size.ToString("X8"));
        //        });
        //        sw.WriteLine("--------------------------------------------------");
        //    });

        //});
        //sw.Close();
        #endregion
        [DllImport("cdfc_device.dll")]
        private static extern void cdfc_get_diskinfo([MarshalAs(UnmanagedType.LPTStr)]string lpFileName, [MarshalAs(UnmanagedType.LPTStr)]string dirvename, IntPtr freeState);
        [DllImport("CDFCVideoPreviewer.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IniMemoryPreviewer(IntPtr buf, ulong bufSize,IntPtr error);
        [DllImport("CDFCVideoPreviewer.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Play();
        [DllImport("CDFCVideoPreviewer.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Stop();

        [DllImport("VideoRecover.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern int videorecover_fstype(IntPtr handle);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_searchstart(IntPtr hDisk, int eType, ulong nStartSec, ulong nEndSec,
                    int nSecSize, ulong nAreaSize, int nClusterSize, ulong nLBAPos, bool bJournal, IntPtr nError);
        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_get_date(ref ulong nOffsetSec, ref ulong nFileCount);

        [DllImport("kernel32.dll")]
        static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

        [DllImport("kernel32.dll")]
        static extern bool SetFilePointerEx(
          SafeFileHandle         hFile,
          LARGE_INTEGER  liDistanceToMove,
          ref LARGE_INTEGER lpNewFilePointer,
          uint          dwMoveMethod
        );
        


        static long GetFilePointerEx(SafeFileHandle hFile) {
            LARGE_INTEGER liOfs = new LARGE_INTEGER();
            LARGE_INTEGER liNew = new LARGE_INTEGER();

            SetFilePointerEx(hFile, liOfs, ref liNew, 1);
            return (long)liNew.highpart << 32 | (long) liNew.lowpart;
        }

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto,CallingConvention = CallingConvention.StdCall,SetLastError = true)]
        //private static extern uint GetFileSize(
        //    SafeFileHandle handle,
        //        ref uint lpFileSizeHigh
        //);

        //[DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        //private static extern MOpenFile([MarshalAs(UnmanagedType.LPWStr) string fileName,)
        /// <summary>
        /// 大华系统的年月日，分，进制数字规则；
        /// </summary>
        private static readonly uint[] DHHexNum = new uint[] {
            67108864,4194304,131072,4096,64,1
        };

        

        #region 各系统的时间获得方法
        /// <summary>
        /// 获得大华的时间
        /// </summary>
        /// <param name="dateNum">从起始时间的偏移量</param>
        /// <returns>用短整型数组表示的年月日，时分秒</returns>
        private static DateTime? GetDHTime(uint dateNum) {
            short[] dateNums = new short[6];
            DateTime dt;

            for (byte index = 0; index < 6; index++) {
                var innerDateNum = dateNum;
                for (byte innerIndex = 0; innerIndex < index; innerIndex++) {
                    innerDateNum %= DHHexNum[innerIndex];
                }
                if (innerDateNum != 0) {
                    dateNums[index] = (short)(innerDateNum / DHHexNum[index]);
                }
            }

            try {
                //大华起始时间为2000年初始时间;
                dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt;
            }
            //若时间构造失败;
            catch {
                return null;
            }
        }
#endregion
        [STAThread]
        public static void Main(string[] args) {
            var handle2 = SharpLib.Win32.Function.CreateFile("\\\\.\\PhysicalDrive0",
                  SharpLib.Win32.FileAccess.FILE_GENERIC_READ,
                (SharpLib.Win32.FileShare.FILE_SHARE_WRITE | SharpLib.Win32.FileShare.FILE_SHARE_READ),
                 IntPtr.Zero, SharpLib.Win32.CreationDisposition.OPEN_EXISTING,
                  SharpLib.Win32.FileFlagsAttributes.FILE_ATTRIBUTE_NORMAL,IntPtr.Zero);
            
            long hg = 0;
            var size = GetFileSizeEx(handle2,out hg);
            var fs2 = new FileStream(handle2, FileAccess.Read );

            fs2.Position = 123;
            var pos = GetFilePointerEx(handle2);
            ComObject co = ComObject.LocalObject;
            uint s22 = 67108864 * 17 + 4194304 * 9 + 131072 * 06 + 4096 * 1;
            var dt2 = GetDHTime(0x464CC2B5);

            //var file = ImgFile.GetImgFile("C://DHTest2 - Copy.img");
            var fs = File.OpenRead("H://3a6900e8fb3f3ef7495fa9f2252f217ec51725b9.rar");
            var handle = fs.SafeFileHandle.DangerousGetHandle();
            var sz = fs.Length / 512;
            var tp = videorecover_fstype(handle);

            var errPtr = Marshal.AllocHGlobal(4);
            var done = false;
            ThreadPool.QueueUserWorkItem(cb => {
                var s = hb_searchstart(handle, 0, 0, (ulong) sz, 512, 0, 0, 0, true, errPtr);
                done = true;
            });
            
            //var offsetSec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
            //var nFileCount = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
            ulong offsetSec = 0;
            ulong nFileCount = 0;
            //Marshal.WriteInt64(offsetSec, 16);
            //Marshal.WriteInt64(nFileCount, 16);


            while (!done) {
                hb_get_date(ref offsetSec, ref nFileCount);
                Console.WriteLine($"{offsetSec}-{nFileCount}");
                //Console.WriteLine($"{Marshal.ReadInt64(offsetSec)}-{Marshal.ReadInt64(nFileCount)}");
                Thread.Sleep(1000);
            }
            //while (s != IntPtr.Zero) {
            //    var st = s.GetStructure<DateCategoryStruct>();


            //    var file2 = st.File;
            //    while (file2 != IntPtr.Zero) {
            //        var video = file2.GetStructure<VideoStruct>();
            //        Console.WriteLine(video.Size);
            //        file2 = video.Next;
            //    }
            //    s = st.Next;
            //}

        }

        private static string BuildrDtEnString(DateTime dt) {
            var ts = dt - DateTime.Parse("1970/01/01");
            var tSeconds = (int) ts.TotalSeconds;
            var bs = FillEndo(tSeconds.ToString());
            File.WriteAllBytes("D://sda.txt", bs);
            var bsString = Encoding.ASCII.GetString(bs);
            return bsString;
        }

        private static DateTime? GetDtByEn(string en) {
            try {
                var endo = GetEndoByFileBytes(Encoding.ASCII.GetBytes(en));
                var s = int.Parse(endo);
                return DateTime.Parse("1970/01/01").AddSeconds(s);
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(GetDtByEn)}:{ex.Message}{nameof(en)}:{en}");
                return null;
            }
        }

        ///// <summary>
        ///// 通过文件字节生成endo;
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <returns></returns>
        public static string GetEndoByFileBytes(byte[] buffer) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var sb = new StringBuilder();
            try {
                var index = 0;
                while (true) {
                    var skipCount = buffer[index];
                    if (buffer[index + skipCount] == 0) {
                        break;
                    }
                    sb.Append((char)buffer[skipCount + index + 1]);
                    index = index + skipCount;
                }
                return sb.ToString();
            }
            catch {
                return null;
            }

        }
        ///// <summary>
        ///// 通过endo生成文件字节;
        ///// </summary>
        ///// <param name = "endo" ></ param >
        ///// < returns ></ returns >
        //public static byte[] FillEndo(string endo) {
        //    var arr = Encoding.ASCII.GetBytes(endo);
        //    try {
        //        var curIndex = 0;
        //        var buffer = new byte[20 * 1024];
        //        var rand = new Random();

            //        for (int i = 0; i < buffer.Length; i++) {
            //            //buffer[i] = (byte)rand.Next(255);
            //        }

            //        foreach (var item in arr) {
            //            var nextPosition = (byte)rand.Next(2, 50);
            //            buffer[curIndex] = nextPosition;
            //            buffer[curIndex + nextPosition + 1] = item;
            //            curIndex += nextPosition;
            //        }

            //        buffer[curIndex] = 0;
            //        return buffer;
            //    }
            //    catch {
            //        return null;
            //    }
            //}

            //var fs = new FileStream("J://1.dav", FileMode.Open);
            //var bArr = new byte[fs.Length];
            //fs.Read(bArr, 0, (int)fs.Length);
            //    var length = fs.Length;
            //fs.Close();

            //    var buffer = Marshal.AllocHGlobal((int)length);
            //Marshal.Copy(bArr, 0, buffer, (int) length);
            //    fs.Close();
            //    var errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            //    IniMemoryPreviewer(buffer,(ulong)length, errorPtr);
            //    Play();
            //    Stop();

            //DateTime dt = new DateTime();
            //hs.GetRtc(ref dt);
            //string s = Hasp.KeyInfo;
            //    hs.Logout();

        private static SmartUkeyApp smartUkeyWorker = new SmartUkeyApp();
        private static int[] keyHandles = new int[8];
        private static int[] keyNum = new int[1];
        private static bool DongleOK {
            get {
                if (smartUkeyWorker == null) {
                    smartUkeyWorker = new SmartUkeyApp();
                }
                var rCode = smartUkeyWorker.SmartUKeyFind("CDFCVDExactor", keyHandles, keyNum);
                return rCode == 0;
            }
        }
        
        /// <summary>
        /// 填充某个字符串至新文件;
        /// </summary>
        /// <param name="endo">需加密的字符</param>
        /// <param name="fileName">文件名(相对或绝对)</param>
        private static void FillFile(string endo,string fileName) {
            if (endo == null)
                throw new ArgumentNullException(nameof(endo));
            try {
                var endoLength = endo.Length;
                var newFs = File.Create(fileName);
                var random = new Random();

                //随机生成128位字节;
                for (int i = 0; i < 128; i++) {
                    var randN = (byte)random.Next(0, 255);
                    newFs.WriteByte(randN);
                }

                //写入endo长度;占用4位;并做一些处理;
                for (int i = 0; i < 4; i++) {
                    var bt = (byte)(endoLength % 10 + 'X' + i);
                    newFs.WriteByte(bt);
                    endoLength /= 10;
                }

                endoLength = endo.Length;
                //读取endo至缓冲区(已经确定endo中每个字符大小在ASCII范围内，故生成的缓冲区大小必定等于endo长度);
                var buffer = Encoding.ASCII.GetBytes(endo);

                //随机生成一个字符,以为后来填充的endo字符串作为偏移量;
                var subByte = (byte)random.Next(0, 255);

                //增加endo偏移;
                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] += subByte;
                }

                //写入endo;
                newFs.Write(buffer, 0, buffer.Length);

                //写入subByte;占用4位;
                newFs.WriteByte((byte)(subByte + '&'));

                //获取填充之后的应继续填充的剩余长度;
                var leftSize = 4096 - newFs.Position;
                //随机填充剩余长度;
                for (int i = 0; i < leftSize; i++) {
                    var randN = (byte)random.Next(0, 255);
                    newFs.WriteByte(randN);
                }

                newFs.Close();
            }
            catch {

            }
        }

        /// <summary>
        /// 通过填充后
        /// </summary>
        /// <param name="filledString"></param>
        /// <returns></returns>
        private static string GetEndoByFilledString(byte[] buffer) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if(buffer.Length != 4096) {
                throw new ArgumentException(nameof(buffer));
            }

            try {
                var endoLength = 0;
                for (int i = 3; i >= 0; i--) {
                    endoLength *= 10;
                    endoLength += buffer[128 + i] - i - 'X';
                }
                if(endoLength > 0 && endoLength < 1024) {
                    
                }
                return null;
            }
            catch {
                return null;
            }
        }


        /// <summary>
        /// 通过文件字节生成endo;
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static byte[] FillEndo(string endo) {
            var arr = Encoding.ASCII.GetBytes(endo);
            try {
                var curIndex = 0;
                var buffer = new byte[20 * 1024];
                var rand = new Random();
                
                for (int i = 0; i < buffer.Length; i++) {
                    //buffer[i] = (byte)rand.Next(255);
                }

                foreach (var item in arr) {
                    var nextPosition = (byte)rand.Next(2,50);
                    buffer[curIndex] = nextPosition;
                    buffer[curIndex + nextPosition + 1] = item;
                    curIndex += nextPosition;
                }
                
                buffer[curIndex] = 0;
                return buffer;
            }
            catch {
                return null;
            }
        }
    }
    public static class AES {
        public static string Encrypt(string toEncrypt, string keyCode) {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(keyCode);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string toDecrypt, string keyCode) {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(keyCode);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string AESDecrypt(this string toDecrypt, string keyCode) {
            return Decrypt(toDecrypt, keyCode);
        }
    }
    public class SmartUkeyApp {
        bool _is64ibt = false;
        public SmartUkeyApp() {
            if (IntPtr.Size == 8)
                _is64ibt = true;
        }


        public int SmartUKeyFind(string appID, int[] keyHandles, int[] keyNumber) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyFind(appID, keyHandles, keyNumber);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyFind(appID, keyHandles, keyNumber);
            }
        }

        public int SmartUKeyGetLastError() {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyGetLastError();
            }
            else {
                return SmartUkeyAppX86.SmartUKeyGetLastError();
            }
        }

        public int SmartUKeyGetUid(int keyHandle, StringBuilder Uid) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyGetUid(keyHandle, Uid);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyGetUid(keyHandle, Uid);
            }
        }

        public int SmartUKeyCheckExist(int keyHandle) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyCheckExist(keyHandle);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyCheckExist(keyHandle);
            }
        }

        public int SmartUKeyOpen(int keyHandle, int password1, int password2, int password3, int password4, int[] requestFromKey) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyOpen(keyHandle, password1, password2, password3, password4, requestFromKey);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyOpen(keyHandle, password1, password2, password3, password4, requestFromKey);
            }
        }

        public int SmartUKeyVerify(int nKeyHandle, int response) {

            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyVerify(nKeyHandle, response);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyVerify(nKeyHandle, response);
            }
        }

        public int SmartUKeyReadMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyReadMemory(keyHandle, nStartAddr, nLen, pBuffer);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyReadMemory(keyHandle, nStartAddr, nLen, pBuffer);
            }
        }

        public int SmartUKeyWriteMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyWriteMemory(keyHandle, nStartAddr, nLen, pBuffer);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyWriteMemory(keyHandle, nStartAddr, nLen, pBuffer);
            }
        }

        public int SmartUKeyClose(int keyHandle) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyClose(keyHandle);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyClose(keyHandle);
            }
        }

        public int SmartUKeyTriDesEncrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyTriDesEncrypt(keyHandle, iv, dataLen, pDataBuffer);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyTriDesEncrypt(keyHandle, iv, dataLen, pDataBuffer);
            }
        }

        public int SmartUKeyTriDesDecrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyTriDesDecrypt(keyHandle, iv, dataLen, pDataBuffer);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyTriDesDecrypt(keyHandle, iv, dataLen, pDataBuffer);
            }
        }

        public int SmartUKeyResetPassRequest(int keyHandle, StringBuilder request) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyResetPassRequest(keyHandle, request);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyResetPassRequest(keyHandle, request);
            }
        }

        public int SmartUKeyResetPass(int keyHandle, StringBuilder response) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyResetPass(keyHandle, response);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyResetPass(keyHandle, response);
            }
        }

        public int SmartUKeyGetDrive(int keyHandle, int diskType, StringBuilder drv) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyGetDrive(keyHandle, diskType, drv);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyGetDrive(keyHandle, diskType, drv);
            }

        }

        public int SmartUKeyLoginSD(int keyHandle, string userPin, int[] isLocked, int[] leftNumber) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyLoginSD(keyHandle, userPin, isLocked, leftNumber);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyLoginSD(keyHandle, userPin, isLocked, leftNumber);
            }
        }

        public int SmartUKeyLogoutSD(long keyHandle) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyLogoutSD(keyHandle);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyLogoutSD(keyHandle);
            }
        }

        public int SmartUKeyChangePassword(int keyHandle, string oldPassword, string newPassword, int[] leftNumber) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartUKeyChangePassword(keyHandle, oldPassword, newPassword, leftNumber);
            }
            else {
                return SmartUkeyAppX86.SmartUKeyChangePassword(keyHandle, oldPassword, newPassword, leftNumber);
            }
        }

        public int SmartFS_SetCurrentDrive(int keyHandle) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_SetCurrentDrive(keyHandle);
            }
            else {
                return SmartUkeyAppX86.SmartFS_SetCurrentDrive(keyHandle);
            }
        }

        public int SmartFS_SetCurrentDirectory(int keyHandle, StringBuilder directory) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_SetCurrentDirectory(keyHandle, directory);
            }
            else {
                return SmartUkeyAppX86.SmartFS_SetCurrentDirectory(keyHandle, directory);
            }
        }

        public int SmartFS_GetCurrentDirectory(int keyHandle, StringBuilder directory) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_GetCurrentDirectory(keyHandle, directory);
            }
            else {
                return SmartUkeyAppX86.SmartFS_GetCurrentDirectory(keyHandle, directory);
            }
        }

        public int SmartFS_EnumDirectoryNext(StringBuilder fName, int[] fileSize, byte[] isDir) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_EnumDirectoryNext(fName, fileSize, isDir);
            }
            else {
                return SmartUkeyAppX86.SmartFS_EnumDirectoryNext(fName, fileSize, isDir);
            }
        }

        public int SmartFS_OpenFile(StringBuilder fileName, byte mode, int[] file_ptr) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_OpenFile(fileName, mode, file_ptr);
            }
            else {
                return SmartUkeyAppX86.SmartFS_OpenFile(fileName, mode, file_ptr);
            }
        }

        public int SmartFS_CloseFile(int file) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_CloseFile(file);
            }
            else {
                return SmartUkeyAppX86.SmartFS_CloseFile(file);
            }
        }

        public int SmartFS_ReadFile(int file, byte[] pBuffer, int BytesToRead, int[] ByteRead) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_ReadFile(file, pBuffer, BytesToRead, ByteRead);
            }
            else {
                return SmartUkeyAppX86.SmartFS_ReadFile(file, pBuffer, BytesToRead, ByteRead);
            }
        }

        public int SmartFS_WriteFile(int file, byte[] pBuffer, int BytesToWrite, int[] BytesWritten) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_WriteFile(file, pBuffer, BytesToWrite, BytesWritten);
            }
            else {
                return SmartUkeyAppX86.SmartFS_WriteFile(file, pBuffer, BytesToWrite, BytesWritten);
            }
        }

        public int SmartFS_FileSeek(int file, int offset) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_FileSeek(file, offset);
            }
            else {
                return SmartUkeyAppX86.SmartFS_FileSeek(file, offset);
            }
        }

        public int SmartFS_FileTruncate(int file) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_FileTruncate(file);
            }
            else {
                return SmartUkeyAppX86.SmartFS_FileTruncate(file);
            }
        }

        public int SmartFS_FileTell(int file, int[] position) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_FileTell(file, position);
            }
            else {
                return SmartUkeyAppX86.SmartFS_FileTell(file, position);
            }
        }

        public int SmartFS_GetSize(int file, int[] fileSize) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_GetSize(file, fileSize);
            }
            else {
                return SmartUkeyAppX86.SmartFS_GetSize(file, fileSize);
            }

        }

        public int SmartFS_FlushFile(int file) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_FlushFile(file);
            }
            else {
                return SmartUkeyAppX86.SmartFS_FlushFile(file);
            }
        }

        public int SmartFS_DeleteFile(string fileName) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_DeleteFile(fileName);
            }
            else {
                return SmartUkeyAppX86.SmartFS_DeleteFile(fileName);
            }
        }

        public int SmartFS_RenameFile(string oldName, string newName) {
            if (_is64ibt) {
                return SmartUkeyAppX64.SmartFS_RenameFile(oldName, newName);
            }
            else {
                return SmartUkeyAppX86.SmartFS_RenameFile(oldName, newName);
            }

        }

    }

    public class SmartUkeyAppX86 {

        //查找加密锁 
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyFind(string appID, int[] keyHandles, int[] keyNumber);

        //错误码
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetLastError();

        //获取加密锁序列号 
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetUid(int keyHandle, StringBuilder Uid);

        //检查KEYHANDLE所连接的加密锁是否存在
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyCheckExist(int keyHandle);

        //使用用户密码打开加密锁，用户密码通过管理工具使用超级密码和种子码生成
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyOpen(int keyHandle, int password1, int password2, int password3, int password4, int[] requestFromKey);

        //使用随机密钥对验证， response的值必须是对应于正确的requestFromKey（由OPEN函数返回）
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyVerify(int nKeyHandle, int response);

        //读取密锁内内存区数据
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyReadMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer);

        //读写密锁内内存区数据
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyWriteMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer);

        //关闭加密锁
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyClose(int keyHandle);

        //使用3DES加密算法及主密钥加密数据
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyTriDesEncrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer);

        //使用3DES加密算法及主密钥解密数据
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyTriDesDecrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer);

        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyResetPassRequest(int keyHandle, StringBuilder request);

        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyResetPass(int keyHandle, StringBuilder response);
        //安全U盘密码复位
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetDrive(int keyHandle, int diskType, StringBuilder drv);

        //登录UKEY安全磁盘
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyLoginSD(int keyHandle, string userPin, int[] isLocked, int[] leftNumber);

        //登出UKEY安全磁盘
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyLogoutSD(long keyHandle);

        //修改安全磁盘密码
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyChangePassword(int keyHandle, string oldPassword, string newPassword, int[] leftNumber);


        //设置当前加密锁
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_SetCurrentDrive(int keyHandle);

        //设置当前目录
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_SetCurrentDirectory(int keyHandle, StringBuilder directory);

        //获取当前目录
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_GetCurrentDirectory(int keyHandle, StringBuilder directory);
        //列出当前目录下的文件和目录  - 下一个文件目录
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_EnumDirectoryNext(StringBuilder fName, int[] fileSize, byte[] isDir);

        //打开新文件或目录 
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_OpenFile(StringBuilder fileName, byte mode, int[] file_ptr);
        //关闭文件和目录 
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_CloseFile(int file);

        //读文件
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_ReadFile(int file, byte[] pBuffer, int BytesToRead, int[] ByteRead);

        //写文件
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_WriteFile(int file, byte[] pBuffer, int BytesToWrite, int[] BytesWritten);

        //文件寻找地址
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileSeek(int file, int offset);

        //文件截止
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileTruncate(int file);

        //获取当前文件位置
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileTell(int file, int[] position);

        //判断当前文件大小
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_GetSize(int file, int[] fileSize);

        //SmartFS_FlushFile(byte[] file);
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FlushFile(int file);

        //删除文件
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_DeleteFile(string fileName);

        //改文件名
        [DllImport("SmartUKeyApp.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_RenameFile(string oldName, string newName);
    }

    public class SmartUkeyAppX64 {

        //查找加密锁 
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyFind(string appID, int[] keyHandles, int[] keyNumber);

        //错误码
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetLastError();

        //获取加密锁序列号 
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetUid(int keyHandle, StringBuilder Uid);

        //检查KEYHANDLE所连接的加密锁是否存在
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyCheckExist(int keyHandle);

        //使用用户密码打开加密锁，用户密码通过管理工具使用超级密码和种子码生成
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyOpen(int keyHandle, int password1, int password2, int password3, int password4, int[] requestFromKey);

        //使用随机密钥对验证， response的值必须是对应于正确的requestFromKey（由OPEN函数返回）
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyVerify(int nKeyHandle, int response);

        //读取密锁内内存区数据
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyReadMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer);

        //读写密锁内内存区数据
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyWriteMemory(int keyHandle, int nStartAddr, int nLen, byte[] pBuffer);

        //关闭加密锁
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyClose(int keyHandle);

        //使用3DES加密算法及主密钥加密数据
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyTriDesEncrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer);

        //使用3DES加密算法及主密钥解密数据
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyTriDesDecrypt(int keyHandle, byte[] iv, int dataLen, byte[] pDataBuffer);

        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyResetPassRequest(int keyHandle, StringBuilder request);

        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyResetPass(int keyHandle, StringBuilder response);
        //安全U盘密码复位
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyGetDrive(int keyHandle, int diskType, StringBuilder drv);

        //登录UKEY安全磁盘
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyLoginSD(int keyHandle, string userPin, int[] isLocked, int[] leftNumber);

        //登出UKEY安全磁盘
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyLogoutSD(long keyHandle);

        //修改安全磁盘密码
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartUKeyChangePassword(int keyHandle, string oldPassword, string newPassword, int[] leftNumber);


        //设置当前加密锁
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_SetCurrentDrive(int keyHandle);

        //设置当前目录
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_SetCurrentDirectory(int keyHandle, StringBuilder directory);

        //获取当前目录
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_GetCurrentDirectory(int keyHandle, StringBuilder directory);
        //列出当前目录下的文件和目录  - 下一个文件目录
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_EnumDirectoryNext(StringBuilder fName, int[] fileSize, byte[] isDir);

        //打开新文件或目录 
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_OpenFile(StringBuilder fileName, byte mode, int[] file_ptr);
        //关闭文件和目录 
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_CloseFile(int file);

        //读文件
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_ReadFile(int file, byte[] pBuffer, int BytesToRead, int[] ByteRead);

        //写文件
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_WriteFile(int file, byte[] pBuffer, int BytesToWrite, int[] BytesWritten);

        //文件寻找地址
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileSeek(int file, int offset);

        //文件截止
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileTruncate(int file);

        //获取当前文件位置
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FileTell(int file, int[] position);

        //判断当前文件大小
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_GetSize(int file, int[] fileSize);

        //SmartFS_FlushFile(byte[] file);
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_FlushFile(int file);

        //删除文件
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_DeleteFile(string fileName);

        //改文件名
        [DllImport("SmartUKeyApp.x64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SmartFS_RenameFile(string oldName, string newName);
    }

}

