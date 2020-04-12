using CDFCVideoExactor.Helpers;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using CDFCMessageBoxes.MessageBoxes;
using CDFCEntities.Enums;
using static CDFCCultures.Managers.LanguageHelper;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            DispatcherUnhandledException += (sender, e) => {
                EventLogger.Logger.WriteLine("主线程错误:" + e.Exception.Message);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                EventLogger.Logger.WriteLine("工作线程错误:" + ((Exception)e.ExceptionObject).Message);
                EventLogger.Logger.WriteLine("工作线程错误:" + ((Exception)e.ExceptionObject).StackTrace);
                var ex = e.ExceptionObject as Exception;
                if(ex != null && ex.InnerException != null) {
                    EventLogger.Logger.WriteLine("工作线程错误:" + ex.InnerException.StackTrace);
                    EventLogger.Logger.WriteLine("工作线程错误: " + ex.InnerException.Message);
                }
                var nullex = e.ExceptionObject as NullReferenceException;
                if(nullex != null) {
                    EventLogger.Logger.WriteLine("Source:" + nullex.Source);
                    
                    var enumrator = nullex.Data.GetEnumerator();
                    while (enumrator.MoveNext()) {
                        EventLogger.Logger.WriteLine("Object:" + enumrator.Current.ToString());
                    }
                }
            };
            var doc = CDFCVideoExactorUpdater.Helpers.VersionHelper.GetLatestDoc();
        }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            
            var args = e.Args;
            bool isOk = false;
            ParseArgs(args, out isOk);
            if (isOk) {
                this.Resources.MergedDictionaries.LoadLanguage("CDFCVideoExactor");
                this.Resources.MergedDictionaries.LoadLanguage("CDFCMessageBoxes");
            }
            else {
                Environment.Exit(0);
            }

        }

        private void ParseArgs(string[] args,out bool ok) {
            try {
                //CDFCMessageBox.Show(args.Length.ToString());
                //foreach(var arg in args) {
                //    CDFCMessageBox.Show(arg);
                //}
                if (args.Length >= 2) {
                    var etr = args[0];
                    var brand = args[1];
                    
                    switch (etr) {
                        case "CpAndMultiMedia":
                            ConfigState.EtrType = Enums.EntranceType.CPAndMultiMedia;
                            ok = true;
                            break;
                        case "Capturer":
                            ConfigState.EtrType = Enums.EntranceType.Capturer;
                            ok = true;
                            break;
                        case "MultiMedia":
                            ConfigState.EtrType = Enums.EntranceType.MultiMedia;
                            ok = true;
                            break;
                        case "CapturerSingle":
                            ConfigState.EtrType = Enums.EntranceType.CapturerSingle;
                            DeviceTypeEnum singleType = DeviceTypeEnum.Unknown;
                            if(Enum.TryParse(brand, out singleType)) {
                                if(singleType != DeviceTypeEnum.Unknown) {
                                    ConfigState.SingleType = singleType;
                                    ok = true;
                                }
                                else {
                                    ok = false;
                                }
                            }
                            else {
                                throw new Exception("Bad Brand Version!");
                            }
                            break;
                        default:
                            throw new Exception("Bad Brand Args");
                    }
                    if (args.Length > 2) {
                        var enWay = args[2];
                        switch (enWay) {
                            case "SoftKey":
                                ConfigState.EnWay = Enums.EncryptWay.SoftKey;
                                break;
                            default:
                                ok = true;
                                break;
                        }
                    }
                }
                else {
                    throw new Exception("Bad StartArgs Count!"+args.Length);
                }
                
            }
            catch (Exception ex) {
                CDFCMessageBox.Show($"{ex.Message}");
                ok = false;
            }
        }
        
    }
    #region 
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
                return SmartUkeyAppX86.SmartUKeyLogoutSD(Convert.ToInt32(keyHandle));
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
    #endregion

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
            public static extern int SmartUKeyLogoutSD(int keyHandle);

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
