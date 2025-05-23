﻿#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Jeffrey Su & Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/JeffreySu/WeiXinMPSDK/blob/master/license.md

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc
    
    文件名：WeixinTrace.cs
    文件功能描述：跟踪日志相关
    
    
    创建标识：Senparc - 20151012

    修改标识：Senparc - 20161225
    修改描述：v4.9.7 1、使用同步锁
                     2、修改日志储存路径，新路径为/App_Data/WeixinTraceLog/SenparcWeixinTrace-yyyyMMdd.log
                     3、添加WeixinExceptionLog方法

    修改标识：Senparc - 20161231
    修改描述：v4.9.8 将SendLog方法改名为SendApiLog，添加SendCustomLog方法

    修改标识：Senparc - 20170101
    修改描述：v4.9.9 1、优化日志记录方法（围绕OnWeixinExceptionFunc为主）
                     2、输出AccessTokenOrAppId

    修改标识：Senparc - 20170304
    修改描述：Senparc.Wexin v4.11.3 日志中添加对线程的记录

    修改标识：Senparc - 20181118
    修改描述：v16.5.0 使用 CO2NET v0.3.0 新的 SenparcTrace 记录方法

    修改标识：Senparc - 20220807
    修改描述：v6.15.5 添加 WeixinTrace.SendApiLog(string, Stream) 重写方法
----------------------------------------------------------------*/

using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.Trace;
using Senparc.Weixin.Exceptions;
using System;
using System.IO;

namespace Senparc.Weixin
{
    //TODO：将WeixinTrace和SenparcTrace通过某种标记明显区分开来

    /// <summary>
    /// 微信日志跟踪
    /// </summary>
    public class WeixinTrace : SenparcTrace
    {
        /// <summary>
        /// 记录WeixinException日志时需要执行的任务
        /// </summary>
        public static Action<WeixinException> OnWeixinExceptionFunc;

        /// <summary>
        /// 记录系统日志
        /// </summary>
        /// <param name="messageFormat"></param>
        /// <param name="param"></param>
        public static void Log(string messageFormat, params object[] param)
        {
            SenparcTrace.Log(messageFormat.FormatWith(param));
        }

        /// <summary>
        /// API请求日志（接收结果）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stream"></param>
        public static void SendApiLog(string url, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(stream))
            {
                SenparcTrace.SendApiLog(url, sr.ReadToEnd());
            }
        }

        #region WeixinException 相关日志

        /// <summary>
        /// WeixinException 日志
        /// </summary>
        /// <param name="ex"></param>
        public static void WeixinExceptionLog(WeixinException ex)
        {
            if (!Config.IsDebug)
            {
                return;
            }
            using (var traceItem = new SenparcTraceItem(SenparcTrace._logEndActon, "WeixinException"))
            {
                traceItem.Log(ex.GetType().Name);
                traceItem.Log("AccessTokenOrAppId：{0}", ex.AccessTokenOrAppId);
                traceItem.Log("Message：{0}", ex.Message);
                traceItem.Log("StackTrace：{0}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    traceItem.Log("InnerException：{0}", ex.InnerException.Message);
                    traceItem.Log("InnerException.StackTrace：{0}", ex.InnerException.StackTrace);
                }
            }

            if (OnWeixinExceptionFunc != null)
            {
                try
                {
                    OnWeixinExceptionFunc(ex);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// ErrorJsonResultException 日志
        /// </summary>
        /// <param name="ex"></param>
        public static void ErrorJsonResultExceptionLog(ErrorJsonResultException ex)
        {
            if (!Config.IsDebug)
            {
                return;
            }

            using (var traceItem = new SenparcTraceItem(SenparcTrace._logEndActon, "ErrorJsonResultException"))
            {
                traceItem.Log("ErrorJsonResultException");
                traceItem.Log("AccessTokenOrAppId：{0}", ex.AccessTokenOrAppId ?? "null");
                traceItem.Log("URL：{0}", ex.Url);
                traceItem.Log("errcode：{0}", ex.JsonResult.errcode);
                traceItem.Log("errmsg：{0}", ex.JsonResult.errmsg);
            }

            if (OnWeixinExceptionFunc != null)
            {
                try
                {
                    OnWeixinExceptionFunc(ex);
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
