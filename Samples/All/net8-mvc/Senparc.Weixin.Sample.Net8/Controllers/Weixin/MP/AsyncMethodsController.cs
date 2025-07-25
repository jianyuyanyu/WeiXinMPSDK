﻿using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Cache;
using Senparc.CO2NET.Extensions;
using Senparc.CO2NET.HttpUtility;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Threading.Tasks;

namespace Senparc.Weixin.Sample.Net8.Controllers
{
    public class AsyncMethodsController : BaseController
    {
        private string appId = Config.SenparcWeixinSetting.WeixinAppId;
        private string appSecret = Config.SenparcWeixinSetting.WeixinAppSecret;

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 使用异步Action测试异步临时二维码接口
        /// </summary>
        /// <returns></returns>
        public async Task<RedirectResult> QrCodeTest()
        {
            var ticks = SystemTime.Now.Ticks.ToString();
            var sceneId = int.Parse(ticks.Substring(ticks.Length - 7, 7));

            var qrResult = await QrCodeApi.CreateAsync(appId, 100, sceneId, QrCode_ActionName.QR_SCENE, "QrTest");
            var qrCodeUrl = QrCodeApi.GetShowQrCodeUrl(qrResult.ticket);

            return Redirect(qrCodeUrl);
        }

        /// <summary>
        /// 使用异步Action测试异步模板消息接口
        /// </summary>
        /// <param name="checkcode"></param>
        /// <returns></returns>
        public async Task<ActionResult> TemplateMessageTest(string checkcode)
        {
            var currentCache = CacheStrategyFactory.GetObjectCacheStrategyInstance();
            var cacheKey = $"TestCheckCode:{checkcode}";
            var openId = await currentCache.GetAsync(cacheKey) as string;//使用缓存，如果多台服务器可以使用分布式缓存共享

            if (openId.IsNullOrEmpty())
            {
                return Content("验证码已过期或不存在！请在“盛派网络小助手”公众号输入“tm”获取验证码。");
            }
            else
            {
                await currentCache.RemoveFromCacheAsync(cacheKey);

                var templateId = "cCh2CTTJIbVZkcycDF08n96FP-oBwyMVrro8C2nfVo4";
                var testData = new //TestTemplateData()
                {
                    first = new TemplateDataItem("【异步模板消息测试】"),
                    keyword1 = new TemplateDataItem(openId),
                    keyword2 = new TemplateDataItem("网页测试"),
                    keyword3 = new TemplateDataItem(SystemTime.Now.ToString("O")),
                    remark = new TemplateDataItem("更详细信息，请到Senparc.Weixin SDK官方网站（https://sdk.weixin.senparc.com）查看！")
                };

                var miniProgram = new TemplateModel_MiniProgram()
                {
                    appid = "wxfcb0a0031394a51c",//【盛派互动（BookHelper）】小程序
                    pagepath = "pages/index/index"
                };

                var result = await TemplateApi.SendTemplateMessageAsync(appId, openId, templateId, null, testData, miniProgram);
                return Content("异步模板消息已经发送到【盛派网络小助手】公众号，请查看。此前的验证码已失效，如需继续测试，请重新获取验证码。");
            }
        }


        #region 异步死锁测试

        /// <summary>
        /// 此方法会引发死锁，需要重启服务
        /// </summary>
        /// <returns></returns>
        public ActionResult DeadLockTest()
        {
            var result =
                RequestUtility.HttpGetAsync(CommonDI.CommonSP, "https://sdk.weixin.senparc.com",
                    cookieContainer: null).Result;
            return Content(result);
        }

        /// <summary>
        /// 此方法可以避免死锁
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NoDeadLockTest()
        {
            var result = await RequestUtility.HttpGetAsync(CommonDI.CommonSP, "https://sdk.weixin.senparc.com",
                cookieContainer: null);
            return Content(result);
        }


        private async Task<string> GetRemoteData()
        {
            string result = null;
            await Task.Run(() =>
               {
                   Task.Delay(1000);
                   result = "hi " + SystemTime.Now.ToString();
               });
            return result;
        }

        /// <summary>
        /// 此方法会引发死锁，需要重启服务
        /// </summary>
        /// <returns></returns>
        public ActionResult DeadLockTest2()
        {
            var result = GetRemoteData().Result;
            return Content(result);
        }


        /// <summary>
        /// 此方法可以避免死锁
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NoDeadLockTest2()
        {
            var result = await GetRemoteData();
            return Content(result);
        }



        /// <summary>
        /// 此方法加上.ConfigureAwait(false)可以避免死锁
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetRemoteData2()
        {
            string result = null;
            await Task.Run(() =>
            {
                Task.Delay(1000);
                result = "hi " + SystemTime.Now.ToString();
            }).ConfigureAwait(false);
            return result;
        }


        /// <summary>
        /// 此方法可以避免死锁
        /// </summary>
        /// <returns></returns>
        public ActionResult NoDeadLockTest3()
        {
            var result = GetRemoteData2().Result;
            return Content(result);
        }


        #endregion
    }
}