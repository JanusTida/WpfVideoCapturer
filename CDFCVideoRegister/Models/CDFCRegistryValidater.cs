using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace CDFCVideoRegister.Models {
    /// <summary>
    /// 与服务器的验证器（是否过期)
    /// </summary>
    public class CDFCRegisteryValidater {
        private const string defaultConnectHttpHost = "http://120.77.10.6:80/help";
        public CDFCRegisteryValidater(RegisterInfo info) {
            this.info = info;
        }
        private RegisterInfo info;
        /// <summary>
        /// 检查是否过期;
        /// </summary>
        /// <returns></returns>
        public int CheckExpired() {
            if (info == null) {
                return -1000;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(info.Phone + "&&");
            sb.Append(info.HardId + "&&");
            sb.Append(info.SoftName);
            int res = -1000;
            using (var client = new WebClient()) {
                var values = new NameValueCollection();
                string encoded = AES.Encrypt(sb.ToString(), "cdfctransfercode");
                //var s = AES.Decrypt(encoded, "cdfctransfercode");
                client.Headers.Add("820E487C", "FB300381");
                values.Add("Code", encoded);
                //    new Dictionary<string, string>
                //{
                //     { "Code", sb.ToString()}
                //};
                //var content = new FormUrlEncodedContent(values);

                var response = client.UploadValues(defaultConnectHttpHost, values);
                var responseString = Encoding.Default.GetString(response);
                Int32.TryParse(responseString, out res);
                //var responseString = await response.Content.ReadAsStringAsync();
            }
            return res;
        }

    }
}
