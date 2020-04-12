using CDFCEntities.DeviceObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventLogger;
using System.Net;
using System.Collections.Specialized;
using CDFCValidaters.Exceptions;

namespace CDFCVideoExactor.Helpers {
    //注册信息，电话，Email等.
    public class RegisterInfo {
        public static DateTime IniTime = DateTime.Parse("1970/01/01");
        public string SerialNumber { get; set; }       //注册序列号;
        public string HardID { get; set; }          //注册机器码;
        
        //注册功能ID;
        public int? FeatureID {
            get {
                if (SerialNumber != null) {
                    try {
                        var sn = SerialNumber.AESDecrypt("cdfcstorage'code").AESDecrypt("cdfcstorage'code");
                        return int.Parse(sn.Substring(14, 4));
                    }
                    catch(Exception ex) {
                        RegisterLogger.WriteLine($"{nameof(RegisterInfo)}->{nameof(FeatureID)}:{ ex.Message}");
                    }
                }
                return null;
            }
        }
        //期限;
        public TimeSpan? LimTS {
            get {
                if (SerialNumber != null) {
                    try {
                        var sn = SerialNumber.AESDecrypt("cdfcstorage'code").AESDecrypt("cdfcstorage'code");
                        if(sn.Length >= 32) {
                            var isLim = int.Parse(sn.Substring(18, 4)) == 1;
                            if (isLim) {
                                var limDate = TimeSpan.FromSeconds(int.Parse(sn.Substring(22, 10)));
                                return limDate;
                            }
                            
                        }
                        return null;
                    }
                    catch (Exception ex) {
                        RegisterLogger.WriteLine($"{nameof(RegisterInfo)}->{nameof(FeatureID)}:{ ex.Message}");
                    }
                }
                return null;
            }
        }

        //注册的时间;
        public DateTime? RegisterTime { get;private set; }
        
        
        //注册电话：
        public string PhoneNumberString { get; set; }

        //注册邮箱;
        public string Email { get; set; }
        private static bool validated;
        private static RegisterInfo localRegisterInfo;
        public static RegisterInfo LocalRegisterInfo {
            get {
                var keyFile = "key.cdfccer";
                var keyCode = "edocrefsnartcfdc";
                if (File.Exists(keyFile) && localRegisterInfo == null && !validated) {
                    var fs = File.OpenRead(keyFile);
                    if(fs.Length > 204800) {
                        RegisterLogger.WriteLine($"{nameof(RegisterInfoHelper)}->{nameof(LocalRegisterInfo)}:The file is too long to be read!");
                        return null;
                    }
                    try {
                        var buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        var code = GetEndoByFileBytes(buffer);
                        var oriString = code.AESDecrypt(keyCode);
                        localRegisterInfo = GetRegisterInfoByOri(oriString);
                    }
                    catch(Exception ex) {
                        RegisterLogger.WriteLine($"{nameof(RegisterInfo)}->{nameof(LocalRegisterInfo)}:{ex.Message}");
                    }
                    finally {
                        validated = true;
                    }
                }
                return localRegisterInfo;
            }
        }
        public static RegisterInfo GetInfo(string oriString) {
            var md5sn = oriString.Substring(0, 32);

            var snLength = int.Parse(oriString.Substring(32, 4));
            var sn = oriString.Substring(36, snLength);

            var hardIdLength = int.Parse(oriString.Substring(36 + snLength, 4));
            var hardId = oriString.Substring(36 + snLength + 4, hardIdLength);

            var emailLength = int.Parse(oriString.Substring(36 + snLength + 4 + hardIdLength, 4));
            var email = oriString.Substring(36 + snLength + 4 + hardIdLength + 4, emailLength);

            var phoneLength = int.Parse(oriString.Substring(36 + snLength + 4 + hardIdLength + 4 + emailLength, 4));
            var phone = oriString.Substring(36 + snLength + 4 + hardIdLength + 4 + emailLength + 4, phoneLength);

            if (md5sn != MD5.GetMD5(sn)) {
                RegisterLogger.WriteLine($"{nameof(RegisterInfo)}->{nameof(GetInfo)}:Sn not matched");
                throw new SnNotMatchedException("Sn not matched!");
            }
            else {
                var info = new RegisterInfo {
                    SerialNumber = sn,
                    HardID = hardId,
                    Email = email,
                    PhoneNumberString = phone
                };
                return info;
            }
        }
        

        /// <summary>
        /// 通过文件字节生成endo;
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 通过endo生成文件字节;
        /// </summary>
        /// <param name="endo"></param>
        /// <returns></returns>
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

        public static RegisterInfo GetRegisterInfoByOri(string oriString) {
            if (oriString == null)
                throw new ArgumentNullException(nameof(oriString));
            try {
                var fieldList = oriString.Split(new char[] { '&', '&' }).ToList();
                fieldList.RemoveAll(p => string.IsNullOrEmpty(p));
                if (fieldList.Count < 4) {
                    RegisterLogger.WriteLine($"{nameof(RegisterInfoHelper)}->{nameof(GetRegisterInfoByOri)}:Bad filed count {fieldList.Count}");
                    throw new ArgumentNullException("字段数量错误:" + fieldList.Count);
                }
                var info = new RegisterInfo {
                    PhoneNumberString = fieldList[0],
                    SerialNumber = fieldList[1],
                    HardID = fieldList[2],
                    Email = fieldList[3]
                };
                if (fieldList.Count > 4) {
                    int tStamp = 0;
                    if(int.TryParse(fieldList[4], out tStamp)) {
                        info.RegisterTime = IniTime.AddSeconds(tStamp);
                    }
                }
                
                return info;
            }
            catch (Exception ex) {
                RegisterLogger.WriteLine($"{nameof(RegisterInfoHelper)}->{nameof(GetRegisterInfoByOri)}:{ex.Message}");
                return null;
            }
        }
    }

    public static class RegisterInfoHelper {
        private static  RegisterStatus? registerState;
        public static RegisterStatus RegisterState {
            get {
                Func<RegisterStatus,RegisterStatus> assertAct = e => {
                    registerState = e;
                    return e;
                };
                if(registerState == null) {
                    try {
                        if (RegisterInfo.LocalRegisterInfo != null) {
                            //进行本地验证;
                            var info = RegisterInfo.LocalRegisterInfo;
                            int reFeatureId = ConfigState.RequiredFeature;    //要求的功能ID;
    
                            if (info.HardID != ComInfo.LocalHardID) {
                                return assertAct(RegisterStatus.NotMatchedHardID);
                            }
                            else if(info.FeatureID != reFeatureId && info.FeatureID != 4096){
                                return assertAct(RegisterStatus.NotMatchedFeatureID);
                            }
                            else if (info.LimTS != null && info.RegisterTime + info.LimTS.Value < DateTime.Now ) {
                                return assertAct(RegisterStatus.IDExpired);
                            }
                            else{
                                return assertAct(RegisterStatus.OK);
                            }
                        }
                        else {
                            return assertAct(RegisterStatus.EmptySerialNumber);
                        }
                    }
                    catch {
                        
                    }
                    
                }
                return registerState??(registerState = RegisterStatus.None).Value;
            }
        }
        
        /// <summary>
        /// 注册信息;
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static byte[] SubRegisterInfo(RegisterInfo info) {
            if(info == null) 
                throw new ArgumentNullException(nameof(info));
            try {
                var address = "http://cflabverify.applinzi.com/help";
                StringBuilder sb = new StringBuilder();
                sb.Append($"{info.PhoneNumberString}&&");
                sb.Append($"{info.Email}&&");
                sb.Append($"{info.HardID}&&");
                sb.Append($"{info.SerialNumber}");
                using (var client = new WebClient()) {
                    var values = new NameValueCollection();
                    string encoded = AES.Encrypt(sb.ToString(), "cdfctransfercode");
                    //var s = AES.Decrypt(encoded, "cdfctransfercode");
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    client.Headers.Add("820E487C", "ED245610");
                    values.Add("Code", encoded);
                    var response = client.UploadValues(address, values);
                    return response;
                }
            }
            catch(Exception ex) {
                RegisterLogger.WriteLine($"{nameof(RegisterInfoHelper)}->{nameof(SubRegisterInfo)}:{ex.Message}");
                return null;
            }
            
        }
    }
    public enum RegisterStatus {
        OK,
        NotMatchedHardID,
        NotMatchedFeatureID,
        InvalidSerialNumber,
        EmptySerialNumber,
        IDExpired,
        None
    }
}
