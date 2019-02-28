using System;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace SharpCooking
{
    public static class Cookies
    {
        /// <summary>
        /// Set the default value for global cookies in MINUTES
        /// </summary>
        /// <param name="minutes"></param>
        public static void SetExpireTime(int minutes)
        {
            ExpireTime = minutes;
        }

        /// <summary>
        /// Expiring time of a cookie set by MINUTES
        /// </summary>
        private static int ExpireTime
        {
            get
            {
                return _ExpireTime == 0 ? 60 : _ExpireTime;
            }
            set { _ExpireTime = value; }
        }

        private static int _ExpireTime { get; set; }

        /// <summary>
        /// Set a cookie value, replacing if exists or creating new one if not
        /// </summary>
        /// <param name="obj">Object to be stored (Any type)</param>
        /// <param name="cookieKey">Key of the cookie</param>
        /// <param name="expireMinutes">(OPTIONAL) Expiring time in minutes. If not set, it will use the global one (Default 60 minutes)</param>
        public static void SetCookie(object obj, string cookieKey, int expireMinutes = 0)
        {
            try
            {
                if (obj == null)
                    RemoveCookie(cookieKey);

                if (expireMinutes == 0)
                    expireMinutes = ExpireTime;
            }
            catch
            {
                expireMinutes = ExpireTime; }

            if (Convert.GetTypeCode(obj) != TypeCode.Object)
            {
                var existingCookie = HttpContext.Current.Request.Cookies[cookieKey];
                var objString = obj.ToString();

                if (Encoding.ASCII.GetBytes(objString).Length > 4000)
                {
                    throw new Exception("Maximum value of 4KB of json size reached ! Its not avaliable ");
                }

                if (existingCookie == null)
                {   
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, objString));
                    HttpContext.Current.Request.Cookies.Add(new HttpCookie(cookieKey, objString));
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
                else
                {
                    var existingResponseCookie = HttpContext.Current.Response.Cookies[cookieKey];
                    if (existingResponseCookie == null)
                        HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, objString));
                    else
                        HttpContext.Current.Response.Cookies[cookieKey].Value = objString;

                    
                    HttpContext.Current.Request.Cookies[cookieKey].Value = objString;
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
            }
            else
            {
                var existingCookie = HttpContext.Current.Request[cookieKey];
                var jsonObject =
                    HttpContext.Current.Server.UrlEncode(new JavaScriptSerializer().Serialize(obj)).Replace(";", "");

                if (Encoding.ASCII.GetBytes(jsonObject).Length > 4000)
                {
                    throw new Exception("Maximum value of 4KB of json size reached ! Its not avaliable ");
                }

                if (existingCookie == null)
                {   
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, jsonObject));
                    HttpContext.Current.Request.Cookies.Add(new HttpCookie(cookieKey, jsonObject));
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
                else
                {
                    var existingResponseCookie = HttpContext.Current.Response.Cookies[cookieKey];
                    if (existingResponseCookie == null)
                        HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, jsonObject));
                    else
                        HttpContext.Current.Response.Cookies[cookieKey].Value = jsonObject;

                    HttpContext.Current.Response.Cookies[cookieKey].Value = jsonObject;
                    HttpContext.Current.Request.Cookies[cookieKey].Value = jsonObject;
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
            }
        }

        /// <summary>
        /// Return a string[] containing all cookies inside Request
        /// </summary>
        /// <returns></returns>
        public static string[] GetCookieCollection()
        {
            return HttpContext.Current.Request.Cookies.AllKeys;
        }

        /// <summary>
        /// Return the serialized value for the given key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static T GetCookie<T>(string cookieKey) where T : class
        {
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Cookies[cookieKey]?.Value))
            {
                var tempValue = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Cookies[cookieKey]?.Value);
                SetCookie(tempValue, cookieKey);

                if ((tempValue.StartsWith("{") && tempValue.EndsWith("}")) || //For object
                    (tempValue.StartsWith("[") && tempValue.EndsWith("]"))) //For array        
                {
                    return
                          new JavaScriptSerializer().Deserialize<T>(tempValue);
                }

                return tempValue as T;
            }
            return null;
        }

        /// <summary>
        /// Return primitive value as object, being necessary to do a implicit conversion on the value get
        /// </summary>
        /// <param name="cookieKey">Key of the cookie</param>
        /// <returns></returns>
        public static object GetCookiePrimitiveType(string cookieKey)
        {
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Cookies[cookieKey]?.Value))
                return HttpContext.Current.Request.Cookies[cookieKey]?.Value;

            return null;
        }

        /// <summary>
        /// Remove cookie associated on the given key
        /// </summary>
        /// <param name="cookieKey"></param>
        public static void RemoveCookie(string cookieKey)
        {
            HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddDays(-1);
        }
    }
}
