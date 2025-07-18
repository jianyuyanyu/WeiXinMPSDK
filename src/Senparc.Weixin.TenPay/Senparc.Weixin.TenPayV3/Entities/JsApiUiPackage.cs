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
    
    文件名：JsApiUiPackage.cs
    文件功能描述：为 JsApi 在 UI 输出准备的信息包
    
    
    创建标识：Senparc - 20210904
  
    修改标识：mojinxun - 20250618
    修改描述：v2.1.0 兼容微信平台证书和微信支付公钥 / PR #3144

----------------------------------------------------------------*/

namespace Senparc.Weixin.TenPayV3
{
    /// <summary>
    /// 为 JsApi 在 UI 输出准备的信息包
    /// </summary>
    public class JsApiUiPackage
    {
        /// <summary>
        /// 微信AppId
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }
        /// <summary>
        /// 随机码
        /// </summary>
        public string NonceStr { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// PrepayId的打包信息
        /// </summary>
        public string PrepayIdPackage { get; set; }
        /// <summary>
        /// 签名类型，固定填RSA。
        /// </summary>
        public string SignType { get { return "RSA"; } }

        public JsApiUiPackage(string appId, string timestamp, string nonceStr,string prepayIdPackage, string signature)
        {
            AppId = appId;
            Timestamp = timestamp;
            NonceStr = nonceStr;
            PrepayIdPackage = prepayIdPackage;
            Signature = signature;
        }
    }
}
