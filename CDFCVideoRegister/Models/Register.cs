using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace CDFCVideoRegister.Models {
    /// <summary>
    /// 与服务器注册交互的对象;
    /// </summary>
    public class Register {
        private const string defaultConnectHttpHost = "http://120.77.10.6:80/help";
        public Register(RegisterInfo info) {
            this.info = info;
        }
        private RegisterInfo info;
        public RegisterInfo RegisterInfo {
            get {
                return info;
            }
        }
        /// <summary>
        /// 异步与服务器验证;
        /// </summary>
        /// <returns></returns>
        public int Validate() {
            if (info == null) {
                return -1000;
            }
            int res = -1000;
            StringBuilder sb = new StringBuilder();
            sb.Append(info.Name + "&&");
            sb.Append(info.Phone + "&&");
            sb.Append(info.Email + "&&");
            sb.Append(info.SoftName + "&&");
            sb.Append(info.Company + "&&");
            sb.Append(info.HardId);


            using (var client = new WebClient()) {
                var values = new NameValueCollection();
                string encoded = AES.Encrypt(sb.ToString(), "cdfctransfercode");
                client.Headers.Add("820E487C", "A4153881");
                values.Add("Code", encoded);
                try {
                    var response = client.UploadValues(new Uri(defaultConnectHttpHost), values);
                    var responseString = Encoding.Default.GetString(response);
                    Int32.TryParse(responseString, out res);
                }
                catch(Exception ex) {
                    EventLogger.RegisterLogger.WriteLine("Register->Validate错误:" + ex.Message);
                }
                
                
            }
            return res;
        }
       
    }

}
