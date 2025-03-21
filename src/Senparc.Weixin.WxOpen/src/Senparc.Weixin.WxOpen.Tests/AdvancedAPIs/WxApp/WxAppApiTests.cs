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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.WxOpen.Tests;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp.Tests
{
    [TestClass()]
    public class WxAppApiTests : WxOpenBaseTest
    {
        [TestMethod()]
        public void CreateWxaQrCodeTest()
        {
            var dt1 = SystemTime.Now;
            using (var ms = new MemoryStream())
            {
                var result = WxAppApi.CreateWxQrCode(base._wxOpenAppId, ms, "pages/websocket", 100);
                Assert.AreEqual(ReturnCode.请求成功, result.errcode);
                
                ms.Seek(0, SeekOrigin.Begin);
                //储存图片

                var filePath = "../../Config/qr.jpg";
                File.Delete(filePath);
                using (var fs = new FileStream(filePath, FileMode.CreateNew))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }
                Assert.IsTrue(File.Exists(filePath));
            }

            var dt2 = SystemTime.Now;
            Console.WriteLine("执行时间：{0}ms", (dt2 - dt1).TotalMilliseconds);
        }

        [TestMethod()]
        public void CreateWxaQrCodeTest2()
        {
            var dt1 = SystemTime.Now;
            var filePath = "../../Config/qr2.jpg";
            var result = WxAppApi.CreateWxQrCode(base._wxOpenAppId, filePath, "pages/websocket", 100);
            var dt2 = SystemTime.Now;
            Console.WriteLine("执行时间：{0}ms", (dt2 - dt1).TotalMilliseconds);
        }

        [TestMethod()]
        public void CreateWxaQrCodeAsyncTest()
        {
            var dt1 = SystemTime.Now;
            var filePath = "../../Config/qr-async.jpg";
            Task.Factory.StartNew(async () =>
            {
                var ms = new MemoryStream();
                var result = await WxAppApi.CreateWxQrCodeAsync(base._wxOpenAppId, ms, "pages/websocket", 100);
                Assert.AreEqual(ReturnCode.请求成功, result.errcode);

                ms.Seek(0, SeekOrigin.Begin);
                //储存图片
                File.Delete(filePath);
                using (var fs = new FileStream(filePath, FileMode.CreateNew))
                {
                    await ms.CopyToAsync(fs);
                    await fs.FlushAsync();
                }

                Assert.IsTrue(File.Exists(filePath));
            });
            var dt2 = SystemTime.Now;

            while (!File.Exists(filePath))
            {

            }
            var dt3 = SystemTime.Now;
            Console.WriteLine("执行时间：{0}ms", (dt2 - dt1).TotalMilliseconds);
            Console.WriteLine("等待时间：{0}ms", (dt3 - dt2).TotalMilliseconds);
        }

        [TestMethod()]
        public void CreateWxaQrCodeAsyncTest2()
        {
            var dt1 = SystemTime.Now;
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET6_0_OR_GREATER
            var filePath = "../../../Config/qr-async2.jpg";
#else
            var filePath = "../../Config/qr-async2.jpg";
#endif
            Task.Factory.StartNew(async () =>
            {
                var result = await WxAppApi.CreateWxQrCodeAsync(base._wxOpenAppId, filePath, "pages/websocket", 100);
                Assert.AreEqual(ReturnCode.请求成功, result.errcode);
                Assert.IsTrue(File.Exists(filePath));
            });
            var dt2 = SystemTime.Now;

            while (!File.Exists(filePath))
            {

            }
            var dt3 = SystemTime.Now;
            Console.WriteLine("执行时间：{0}ms", (dt2 - dt1).TotalMilliseconds);
            Console.WriteLine("等待时间：{0}ms", (dt3 - dt2).TotalMilliseconds);
        }

        [TestMethod]
        public void GetWxaCodeUnlimitTest()
        {
            Console.WriteLine("GetWxaCodeUnlimitTest开始");
#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET6_0_OR_GREATER
            var filePath = "../../../qr-wxopen.jpg";
#else
            var filePath = "../../qr-wxopen.jpg";
# endif
            string scene = "notnull";
            var result = WxAppApi.GetWxaCodeUnlimit(base._wxOpenAppId, filePath, scene, "pages/websocket/websocket", true, "release", 640, false, new LineColor(100, 20, 30));
            Assert.IsNotNull(result);
            Console.WriteLine("GetWxaCodeUnlimitTest 返回结果");
            Console.WriteLine(result.ToJson());
            Assert.AreEqual(ReturnCode.请求成功, result.errcode);
            Assert.IsTrue(File.Exists(filePath));

        }

        [TestMethod]
        public void MsgSecCheckTest()
        {
            var contents = new[] { "特3456书yuuo莞6543李zxcz蒜7782法fgnv级", "完2347全dfji试3726测asad感3847知qwez到 " };//官方提供
            foreach (var content in contents)
            {
                try
                {
                    WxAppApi.MsgSecCheck(base._wxOpenAppId, content);
                }
                catch (ErrorJsonResultException ex)
                {
                    Console.WriteLine(ex.JsonResult.ToJson());
                    Assert.AreEqual(ReturnCode.内容含有违法违规内容, ex.JsonResult.errcode);
                    Assert.IsTrue(ex.JsonResult.errmsg.Contains("risky"));
                }
                catch
                {
                    throw;
                }
            }
        }

        [TestMethod()]
        public void MediaCheckAsyncTest()
        {
            var url = "https://sdk.weixin.senparc.com/images/v2/logo%20.png";

            var openId = "oeaTy0DgoGq-lyqvTauWVjbIVuP0";
            var result = WxAppApi.MediaCheckAsync(base._wxOpenAppId, url, "2", 2, openId, 1);

            Console.WriteLine(SystemTime.Now);
            Console.WriteLine(result);
        }
    }
}