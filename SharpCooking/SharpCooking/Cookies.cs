using System;
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
                return _ExpireTime ?? 60;
            }
            set { _ExpireTime = value; }
        }

        private static int? _ExpireTime { get; set; }

        /// <summary>
        /// Set a cookie value, replacing if exists or creating new one if not
        /// </summary>
        /// <param name="obj">Object to be stored (Any type)</param>
        /// <param name="cookieKey">Key of the cookie</param>
        /// <param name="expireMinutes">(OPTIONAL) Expiring time in minutes. If not set, it will use the global one (Default 60 minutes)</param>
        public static void SetCookie(object obj, string cookieKey, int expireMinutes = 0)
        {
            if (expireMinutes == 0)
                expireMinutes = ExpireTime;

            if (Convert.GetTypeCode(obj) != TypeCode.Object)
            {
                var existingCookie = HttpContext.Current.Request[cookieKey];
                if (existingCookie == null)
                {
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, obj.ToString()));
                    HttpContext.Current.Request.Cookies.Add(new HttpCookie(cookieKey, obj.ToString()));
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
                else
                {
                    HttpContext.Current.Response.Cookies[cookieKey].Value = obj.ToString();
                    HttpContext.Current.Request.Cookies[cookieKey].Value = obj.ToString();
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
            }
            else
            {
                var existingCookie = HttpContext.Current.Request[cookieKey];
                if (existingCookie == null)
                {
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookieKey, HttpContext.Current.Server.UrlEncode(new JavaScriptSerializer().Serialize(obj)).Replace(";", "")));
                    HttpContext.Current.Request.Cookies.Add(new HttpCookie(cookieKey, HttpContext.Current.Server.UrlEncode(new JavaScriptSerializer().Serialize(obj)).Replace(";", "")));
                    HttpContext.Current.Response.Cookies[cookieKey].Expires = DateTime.Now.AddMinutes(expireMinutes);
                }
                else
                {
                    HttpContext.Current.Response.Cookies[cookieKey].Value = HttpContext.Current.Server.UrlEncode(new JavaScriptSerializer().Serialize(obj)).Replace(";", "");
                    HttpContext.Current.Request.Cookies[cookieKey].Value = HttpContext.Current.Server.UrlEncode(new JavaScriptSerializer().Serialize(obj)).Replace(";", "");
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
                if ((tempValue.StartsWith("{") && tempValue.EndsWith("}")) || //For object
                    (tempValue.StartsWith("[") && tempValue.EndsWith("]"))) //For array        
                    return
                        new JavaScriptSerializer().Deserialize<T>(tempValue);

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
