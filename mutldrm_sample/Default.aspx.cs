using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.IO;
using System.IdentityModel.Tokens;
using System.Security.Policy;

public partial class _Default : System.Web.UI.Page
{
    const string iv = "0123456789abcdef";
    const string inkaAccessKey = ""; //Inka Access Key, Check This Value In Inka PallyCon Console
    const string inkaSiteKey = ""; //Inka Site Key, Check This Value In Inka PallyCon Console
    const string inkaSiteID = ""; //Inka Site ID, Check This Value In Inka PallyCon Console
    const string kollusSecurityKey = ""; //Kollus Security Key, Check This Value In Kollus Console
    const string kollusCustomKey = "";//Kollus Custom User Key, Check This Value In Kollus Console
    const string licenseUrl = "https://license.pallycon.com/ri/licenseManager.do"; //Inka License Url
    const string certificateUrl = "https://license.pallycon.com/ri/fpsKeyManager.do?siteId=" + inkaSiteID; //Inka Certification URL(FPS)
    string drmType = "";
    string streamingType = "";
    string videoGateWayKR = "https://v.kr.kollus.com/";
    string videoGateWayJP = "https://v.jp.kollus.com/";
    protected void Page_Load(object sender, EventArgs e)
    {
        string VideoGateWayKR = "https://v.kr.kollus.com/";
        string VideoGateWayJP = "https://v.jp.kollus.com/";
        video.Src = VideoGateWayKR + "s?jwt=" + CreateWebToken("", "", "", 3600) + "&custom_key=" + kollusCustomKey;
        nextVideo.Value = VideoGateWayKR + "s?jwt=" + CreateWebToken("", "", "", 3600) + "&custom_key=" + kollusCustomKey;//this sample used hidden value, but live service is recommended to get next video address to ajax logic

    }


    /// <summary>
    /// Create Kollus WebToken Function
    /// </summary>
    /// <param name="mediaContentKey">Media Content Key, This Value in Kollus Console's Channel Page</param>
    /// <param name="uploadFileKey">Upload File Key, This Value in Kollus Console's Library Page</param>
    /// <param name="userid">Client User ID, Using Usually Website's Login ID</param>
    /// <param name="expireDate">Token's Expire Date, Format is Unit Time Stamp</param>
    /// <returns></returns>
    private string CreateWebToken(string mediaContentKey, string uploadFileKey, string userid, int expireDate)
    {
        string webToken = "";

        JObject payload = new JObject();
        JArray mediaContentArray = new JArray();
        JObject mediaContent = new JObject();
        JObject drmPolicy = new JObject();
        JObject data = new JObject();
        JObject customHeader = new JObject();
        SetStreamingType(Request.UserAgent);
        payload.Add("expt", ConvertToUnixTimestamp(DateTime.Now) + 300);
        payload.Add("cuid", userid);

        mediaContent.Add("mckey", mediaContentKey);
        drmPolicy.Add("kind", "inka");
        drmPolicy.Add("streaming_type", streamingType);
        data.Add("license_url", licenseUrl);
        data.Add("certificate_url", certificateUrl);
        customHeader.Add("key", "pallycon-customdata-v2");
        string inkaToken = CreateInkaPayload(uploadFileKey, userid);
        customHeader.Add("value", inkaToken);
        data.Add("custom_header", customHeader);
        drmPolicy.Add("data", data);
        mediaContent.Add("drm_policy", drmPolicy);
        mediaContentArray.Add(mediaContent);
        payload.Add("mc", mediaContentArray);

        JObject jwtHead = new JObject();
        jwtHead.Add("typ", "JWT");
        jwtHead.Add("alg", "HS256");
        string jwtHeadString = ToBase64Encoding(jwtHead.ToString());
        string payloadString = ToBase64Encoding(payload.ToString());
        string message = jwtHeadString + "." + payloadString;
        string signature = SHA256Encrypt(jwtHeadString + "." + payloadString, kollusSecurityKey);
        webToken = jwtHeadString + "." + payloadString + "." + signature;

        return webToken;
    }

    private string CreateInkaPayload(string contentId, string userId)
    {
        string result = "";

        string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd") + "T" + DateTime.UtcNow.ToString("HH:mm:ss") + "Z";
        SetStreamingType(Request.UserAgent);
        if (drmType != null)
        {
            JObject inkaPayload = new JObject();
            JObject inkaToken = new JObject();
            JObject playBackPolicy = new JObject();
            JObject security_policy = new JObject();
            playBackPolicy.Add("limit", true);
            playBackPolicy.Add("persistent", false);
            playBackPolicy.Add("duration", 3600);
            inkaToken.Add("playback_policy", playBackPolicy);
            //inkaToken.Add("allow_mobile_abnormal_device", false);
            inkaToken.Add("playready_security_level", 150);

            var token = Convert.ToBase64String(AESEncrypt256(inkaToken.ToString(), inkaSiteKey));
            string hash = inkaAccessKey + drmType + inkaSiteID + userId + contentId + token + timeStamp;
            SHA256Managed sha256 = new SHA256Managed();
            SHA256 sha256Hash = SHA256.Create();
            hash = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(hash)));
            inkaPayload.Add("drm_type", drmType);
            inkaPayload.Add("site_id", inkaSiteID);
            inkaPayload.Add("user_id", userId);
            inkaPayload.Add("cid", contentId);
            inkaPayload.Add("token", token);
            inkaPayload.Add("timestamp", timeStamp);
            inkaPayload.Add("hash", hash);
            result = ToBase64Encoding(inkaPayload.ToString());
            result = HttpUtility.UrlEncode(result);
        }
        else
        {
            result = "";
        }

        return result;
    }

    private void SetStreamingType(string userAgent)
    {
        string[] browsers = new string[] { "CriOS", "Edge", "Edg", "Firefox", "Chrome", "Safari", "Opera", "MSIE", "Trident" };
        string userBrowser = "";
        foreach (string browser in browsers)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, browser))
            {
                userBrowser = browser;
                break;
            }
        }
        switch (userBrowser)
        {
            case "MSIE":
                drmType = "PlayReady";
                streamingType = "dash";
                break;
            case "Trident":
                drmType = "PlayReady";
                streamingType = "dash";
                break;
            case "Edge":
                drmType = "PlayReady";
                streamingType = "dash";
                break;
            case "Edg":
                drmType = "PlayReady";
                streamingType = "dash";
                break;
            case "Chrome":
                drmType = "Widevine";
                streamingType = "dash";
                break;
            case "Firefox":
                drmType = "Widevine";
                streamingType = "dash";
                break;
            case "Opera":
                drmType = "PlayReady";
                streamingType = "dash";
                break;
            case "Safari":
                drmType = "FairPlay";
                streamingType = "hls";
                break;
            case "CriOS":
                drmType = "FairPlay";
                streamingType = "hls";
                break;
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, "Macintosh") && System.Text.RegularExpressions.Regex.IsMatch(userAgent, "Edg"))
        {
            drmType = "Widevine";
            streamingType = "dash";
        }


    }

    static readonly char[] padding = { '=' };
    private string ToBase64Encoding(string text)
    {
        byte[] arr = System.Text.Encoding.UTF8.GetBytes(text);
        string rtnValue = System.Convert.ToBase64String(arr).TrimEnd(padding).Replace('+', '-').Replace('/', '_').Replace("=", "");
        return rtnValue;
    }
    private string ToBase64Encoding(byte[] text)
    {
        string rtnValue = System.Convert.ToBase64String(text).TrimEnd(padding).Replace('+', '-').Replace('/', '_').Replace("=", "");
        return rtnValue;
    }
    static DateTime ConvertFromUnixTimestamp(double timestamp)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddSeconds(timestamp);
    }


    static double ConvertToUnixTimestamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan diff = date - origin;
        return Math.Floor(diff.TotalSeconds);
    }

    public string SHA256Encrypt(string message, string secret)
    {
        byte[] keyByte = System.Text.Encoding.UTF8.GetBytes(secret);
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        using (System.Security.Cryptography.HMACSHA256 hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return HttpUtility.UrlEncode(Convert.ToBase64String(hashmessage));
        }
    }

    public Byte[] AESEncrypt256(String Input, String key)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);

        var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Encoding.UTF8.GetBytes(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        String Output = Convert.ToBase64String(xBuff);
        return xBuff;
    }


    public String AESDecrypt256(String Input, String key)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        var decrypt = aes.CreateDecryptor();
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                오후 3:53
                byte[] xXml = Convert.FromBase64String(Input);
                cs.Write(xXml, 0, xXml.Length);
            }

            xBuff = ms.ToArray();
        }

        String Output = Encoding.UTF8.GetString(xBuff);
        return Output;
    }
}
