using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT
{
    public static class UnixTimestamp
    {
        #region Unix Timestamp 10位 总秒数
        /// <summary>
        /// 生成十位的 Unix 时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int GetUnixTimeStamp(DateTime dt)
        {
            DateTime dateStart = new DateTime(1970, 1, 1).ToLocalTime();
            int timeStamp = Convert.ToInt32((dt - dateStart).TotalSeconds);

            return timeStamp;
        }
        /// <summary>
        /// 日期格式转 Unix 时间戳
        /// </summary>
        /// <param name="dtStr"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static bool GetUnixTimeStampByStr(string dtStr, out int timeStamp)
        {
            DateTime dt = DateTime.Now;
            if (DateTime.TryParse(dtStr, out dt))
            {
                timeStamp = GetUnixTimeStamp(dt);
                return true;
            }
            else
            {
                timeStamp = 0;
                return false;
            }
        }

        /// <summary>
        /// 十位的 Unix 时间戳 转 DateTime
        /// </summary>
        /// <param name="stam"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeFormUnixTimeStamp(int stam)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            DateTime dt = startTime.AddSeconds(stam);
            return dt.ToLocalTime();

        }
        #endregion
    }
}
