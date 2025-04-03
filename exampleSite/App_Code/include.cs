using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace utility
{
    public class myInclude
    {
        public string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36";
        private Random rnd = new Random(DateTime.Now.Millisecond);
        public myInclude()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                // 如果沒有任何 SSL 錯誤，則通過驗證
                if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None) return true;
                // 如果有錯誤，則根據應用程式需求決定是否繼續
                // 這裡可以加入更多自訂邏輯來檢查憑證，或是使用預設行為
                return false;
            };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
        public string sha256(string str)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // 將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);
                // 取得雜湊值位元組陣列
                var hash = sha256.ComputeHash(bytes);
                // 取得 SHA256 雜湊
                var sha256Hash = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
                return sha256Hash;
            }
        }
        public string b2s(byte[] input)
        {
            return System.Text.Encoding.UTF8.GetString(input);
        }
        public string base_url(HttpContext context)
        {
            // 網頁根網址
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/";
        }
        public void exit(HttpContext context)
        {
            // 結束程式
            context.Response.StatusCode = 200;
            context.Response.Body.FlushAsync();
            context.Response.Body.Close();
            //context.Response.Flush(); //強制輸出緩衝區資料
            //context.Response.Clear(); //清除緩衝區的資料
            //context.Response.End(); //結束資料輸出
        }
        public void echoBinary(HttpContext context,string value)
        {
            context.Response.WriteAsync(value);
        }
        public void echoBinary(HttpContext context,byte[] value)
        {
            context.Response.Body.WriteAsync(value, 0, value.Length);
        }
        public string base_dir()
        {
            // 網站根目錄
            //return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        public void allowAjax(HttpContext context)
        {
            // 添加 CORS 標頭，允許所有來源
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");

            if (context.Request.Method == "OPTIONS")
            {
                // 處理瀏覽器的 "pre-flight" OPTIONS 請求
                context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Accept, Origin, X-Requested-With");
                context.Response.Headers.Append("Access-Control-Max-Age", "1728000");
                context.Response.StatusCode = 204; // 設置狀態碼為 204 No Content
                context.Response.ContentLength = 0; // OPTIONS 請求不需要返回內容
                context.Response.Body.FlushAsync(); // 快速結束響應
                return;
            }
        }
        public Dictionary<string, object> getGET_POST(HttpContext context, string inputs, string method)
        {
            /*
                string GETS_STRING="mode,id";
                Dictionary<string, object> get = x.getGET_POST(GETS_STRING, "GET");
                string vv=x.print_r(get,0);
                foreach (string a in get.Keys)
                {
                    Response.Write(a+":"+get[a]+"<br>");
                }
               sample:
                string GETS_STRING="mode,id";
                Dictionary<string, object> get = x.getGET_POST(GETS_STRING, "GET");

                string POSTS_STRING ="abc,b,s_a,s_b[],ddd";

                Dictionary<string, object> post = x.getGET_POST(POSTS_STRING, "POST");
                string q = x.print_r(get, 0);
                string p = x.print_r(post, 0);
                Response.Write("<pre>" + q + "<br>" + p + "</pre>");
                Response.Write("aaaaaaa->" + post["s_a"]+"<br>");
                Response.Write("aaaaaab->" + post["s_b[]"] + "<br>");             
             * 
            */
            method = method.ToUpper();
            Dictionary<string, object> get_post = new Dictionary<string, object>();
            switch (method)
            {
                case "GET":
                    foreach (string k in inputs.Split(','))
                    {
                        if (context.Request.Query.ContainsKey(k))
                        {
                            get_post[k] = context.Request.Query[k];
                        }
                        else
                        {
                            get_post[k] = "";
                        }

                    }
                    break;
                case "POST":
                    foreach (string k in inputs.Split(','))
                    {
                        if (context.Request.Form.ContainsKey(k))
                        {
                            if (context.Request.Form[k].Count() != 1)
                            {
                                //暫時先這樣，以後再修= =
                                //alert(this.Context.Request.Form.GetValues(k).Length.ToString());
                                get_post[k] = implode("┃", context.Request.Form[k]);
                            }
                            else
                            {
                                get_post[k] = context.Request.Form[k];
                            }
                        }
                        else
                        {
                            get_post[k] = "";
                        }
                    }
                    break;
            }
            return get_post;
        }

        public string implode(string keyword, List<string> arrays)
        {
            return string.Join<string>(keyword, arrays);
        }
        public string implode(string keyword, Dictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, Dictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, ArrayList arrays)
        {
            string[] tmp = new String[arrays.Count];
            for (int i = 0; i < arrays.Count; i++)
            {
                tmp[i] = arrays[i].ToString();
            }
            return string.Join(keyword, tmp);
        }

        public string json_encode(object obj)
        {
            // 用 System.Text.Json
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        // 寫一個 json_decode
        public dynamic json_decode(string json)
        {
            // 用 System.Text.Json
            try
            {
                if (json.Trim().StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<ExpandoObject>>(json);
                }
                return JsonSerializer.Deserialize<ExpandoObject>(json);
            }
            catch
            {
                return null;
            }
        }


        //strtotime 轉換成 Unix time
        public string strtotime(string value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (Convert.ToDateTime(value) - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            if (is_string_like(value, "."))
            {
                //有小數點               
                double sec = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000.0) / 1000000.0;
                return sec.ToString();
            }
            else
            {
                return span.TotalSeconds.ToString();
            }
        }
        public bool is_string_like(string data, string find_string)
        {
            return (data.IndexOf(find_string) == -1) ? false : true;
        }
        public bool is_istring_like(string data, string find_string)
        {
            return (data.ToUpper().IndexOf(find_string.ToUpper()) == -1) ? false : true;
        }
        public string strtotime(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span.TotalSeconds.ToString();
        }
        public string date()
        {
            return date("Y-m-d H:i:s", strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format)
        {
            return date(format, strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        //UnixTimeToDateTime
        public DateTime UnixTimeToDateTime(string text)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            // Add the number of seconds in UNIX timestamp to be converted.            
            dateTime = dateTime.AddSeconds(Convert.ToDouble(text));
            return dateTime;
        }
        public string date(string format, string unixtimestamp)
        {
            DateTime tmp = UnixTimeToDateTime(unixtimestamp);
            tmp = tmp.AddHours(+8);
            switch (format)
            {
                case "Y-m-d H:i:s":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss");
                case "Y/m/d":
                    return tmp.ToString("yyyy/MM/dd");
                case "Y/m/d H:i:s":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss");
                case "Y/m/d H:i:s.fff":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                case "Y-m-d_H_i_s":
                    return tmp.ToString("yyyy-MM-dd_HH_mm_ss");
                case "Y-m-d":
                    return tmp.ToString("yyyy-MM-dd");
                case "H:i:s":
                    return tmp.ToString("HH:mm:ss");
                case "Y-m-d H:i":
                    return tmp.ToString("yyyy-MM-dd HH:mm");
                case "Y_m_d_H_i_s":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss");
                case "Y_m_d_H_i_s_fff":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                case "w":
                    //回傳week, sun =0 , sat = 6, mon=1.....
                    return Convert.ToInt16(tmp.DayOfWeek).ToString();
                case "Y":
                    return tmp.ToString("yyyy");
                case "m":
                    return tmp.ToString("MM");
                case "d":
                    return tmp.ToString("dd");
                case "H":
                    return tmp.ToString("HH");
                case "i":
                    return tmp.ToString("mm");
                case "s":
                    return tmp.ToString("ss");
                case "Y-m-d H:i:s.fff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case "Y-m-d H:i:s.ffffff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                case "H:i:s.fff":
                    return tmp.ToString("HH:mm:ss.fff");
                case "H:i:s.ffffff":
                    return tmp.ToString("HH:mm:ss.ffffff");
            }
            return "";
        }
        public string implode(string keyword, ConcurrentDictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }

        public string implode(string keyword, ConcurrentDictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, string[] arrays)
        {
            return string.Join(keyword, arrays);
        }

        public byte[] file_get_contents_post(string URL, Dictionary<string, string> posts)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            string[] mPostData = new string[posts.Keys.Count];
            int step = 0;
            foreach (string k in posts.Keys)
            {
                //postParameters.Add(k, posts[k]);
                mPostData[step] = post_encode_string(k) + "=" + post_encode_string(posts[k]);
                //file_put_contents("C:\\temp\\a.txt", mPostData[step], true);
                step++;

            }
            return file_get_contents_post(URL, implode("&", mPostData));
        }
        public byte[] file_get_contents(string url)
        {
            if (url.ToLower().IndexOf("http://") > -1 || url.ToLower().IndexOf("https://") > -1)
            {
                // URL                                
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 60000;
                request.Proxy = null;
                request.UserAgent = _userAgent;
                //request.Referer = getSystemKey("HTTP_REFERER");
                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                byteData = ReadStream(stream, 32765);
                response.Close();
                stream.Close();
                return byteData;
            }
            else
            {
                byte[] data;
                using (var fs = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    data = ReadStream(fs, 8192);
                    fs.Close();
                };
                return data;
            }
        }
        public byte[] s2b(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }
        public string dirname(string path)
        {
            return Directory.GetParent(path).FullName;
        }
        public string mypd()
        {
            //return dirname(System.Web.HttpContext.Current.Request.PhysicalPath);
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public byte[] file_get_contents_post(string url, string postData)
        {

            HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(url);

            //ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] data = Encoding.UTF8.GetBytes(postData);

            httpWReq.Method = "POST";
            httpWReq.ContentType = "application/x-www-form-urlencoded";
            httpWReq.UserAgent = _userAgent; // "user_agent','Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36";
            httpWReq.Proxy = null;
            httpWReq.Timeout = 60000;
            //httpWReq.Referer = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            //httpWReq.Referer = url;//getSystemKey("HTTP_REFERER");
            httpWReq.ContentLength = data.Length;

            using (Stream stream = httpWReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

            Stream streamD = response.GetResponseStream();
            byte[] byteData = ReadStream(streamD, 32767);
            response.Close();
            streamD.Close();
            httpWReq.Abort();
            return byteData;
            //byte[] responseString = new StreamReader(response.GetResponseStream()).ToArray();

        }
        private byte[] ReadStream(Stream stream, int initialLength)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] bytes = new byte[read];
            Array.Copy(buffer, bytes, read);
            return bytes;
        }
        private string post_encode_string(string value)
        {
            /*int limit = 2000;

            StringBuilder sb = new StringBuilder();
            int loops = value.Length / limit;

            for (int i = 0; i <= loops; i++)
            {
                if (i < loops)
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i, limit)));
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i)));
                }
            }
            //Uri.EscapeDataString()
            return sb.ToString();
            */
            return Uri.EscapeDataString(value);
        }
        public int rand(int min, int max)
        {
            return rnd.Next(min, max);
        }
        public bool is_dir(string path)
        {
            return Directory.Exists(path);
        }
        public bool is_file(string filepath)
        {
            return File.Exists(filepath);
        }
        public void file_put_contents(string filepath, string input)
        {
            file_put_contents(filepath, s2b(input), false);
        }
        public void file_put_contents(string filepath, byte[] input)
        {
            file_put_contents(filepath, input, false);
        }
        public void file_put_contents(string filepath, string input, bool isFileAppend)
        {
            file_put_contents(filepath, s2b(input), isFileAppend);
        }
        public void file_put_contents(string filepath, byte[] input, bool isFileAppend)
        {

            switch (isFileAppend)
            {
                case true:
                    {
                        FileMode FM = new FileMode();
                        if (!is_file(filepath))
                        {
                            FM = FileMode.Create;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.Write, FileShare.Read))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                        else
                        {
                            FM = FileMode.Append;
                            using (FileStream myFile = File.Open(@filepath, FM, FileAccess.Write, FileShare.Read))
                            {
                                myFile.Seek(myFile.Length, SeekOrigin.Begin);
                                myFile.Write(input, 0, input.Length);
                                myFile.Dispose();
                            }
                        }
                    }
                    break;
                case false:
                    {
                        using (FileStream myFile = File.Open(@filepath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                        {
                            myFile.Write(input, 0, input.Length);
                            myFile.Dispose();
                        };
                    }
                    break;
            }
        }
    }
}