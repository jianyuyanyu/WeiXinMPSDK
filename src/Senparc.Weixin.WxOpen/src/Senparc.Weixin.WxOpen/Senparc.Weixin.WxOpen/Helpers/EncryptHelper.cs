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
    
    文件名：EncryptHelper.cs
    文件功能描述：加密、解密处理类
    
    
    创建标识：Senparc - 20170130

    修改标识：Senparc - 20190402
    修改描述：v3.3.10 添加“微信小程序运动步数解密”功能：EncryptHelper.DecryptRunData()

    修改标识：Senparc - 20190727
    修改描述：完善 AES_Decrypt，处理偶然出现的 adding is invalid and cannot be removed 问题（未发现规律）

    修改标识：likui0623 - 20201013
    修改描述：添加解密到实例信息方法

    修改标识：Senparc - 20221129
    修改描述：v3.15.10 EncryptHelper.DecodeEncryptedData() 方法添加 keySize 参数

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
#if NET462
using System.Web.Script.Serialization;
#endif
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.WxOpen.Containers;
using Senparc.Weixin.WxOpen.Entities;

namespace Senparc.Weixin.WxOpen.Helpers
{
    /// <summary>
    /// 签名及加密帮助类
    /// </summary>
    public static class EncryptHelper
    {
        ///// <summary>
        ///// SHA1加密
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static string EncryptToSHA1(string str)
        //{
        //    SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
        //    byte[] str1 = Encoding.UTF8.GetBytes(str);
        //    byte[] str2 = sha1.ComputeHash(str1);
        //    sha1.Clear();
        //    (sha1 as IDisposable).Dispose();
        //    return Convert.ToBase64String(str2);
        //}

        #region 签名


        /// <summary>
        /// 获得签名
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public static string GetSignature(string rawData, string sessionKey)
        {
            var signature = Senparc.CO2NET.Helpers.EncryptHelper.GetSha1(rawData + sessionKey);
            //Senparc.Weixin.Helpers.EncryptHelper.SHA1_Encrypt(rawData + sessionKey);
            return signature;
        }

        /// <summary>
        /// 比较签名是否正确
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="rawData"></param>
        /// <param name="compareSignature"></param>
        /// <exception cref="WxOpenException">当SessionId或SessionKey无效时抛出异常</exception>
        /// <returns></returns>
        public static bool CheckSignature(string sessionId, string rawData, string compareSignature)
        {
            var sessionBag = SessionContainer.GetSession(sessionId);
            if (sessionBag == null)
            {
                throw new WxOpenException("SessionId无效（01）");
            }

            if (string.IsNullOrEmpty(sessionBag.SessionKey))
            {
                throw new WxOpenException("SessionKey无效（02）");
            }

            var signature = GetSignature(rawData, sessionBag.SessionKey);
            return signature == compareSignature;
        }

        #endregion

        #region 解密

        #region 私有方法

        private static byte[] AES_Decrypt(String Input, byte[] Iv, byte[] Key, int keySize = 128)
        {
#if NET462
            RijndaelManaged aes = new RijndaelManaged();
#else
            SymmetricAlgorithm aes = Aes.Create();
#endif
            aes.KeySize = keySize;//原始：256
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            aes.IV = Iv;
            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] xBuff = null;

            //using (ICryptoTransform decrypt = aes.CreateDecryptor(aes.Key, aes.IV) /*aes.CreateDecryptor()*/)
            //{
            //    var src = Convert.FromBase64String(Input); 
            //    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
            //    return dest;
            //    //return Encoding.UTF8.GetString(dest);
            //}


            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        //cs.Read(decryptBytes, 0, decryptBytes.Length);
                        //cs.Close();
                        //ms.Close();

                        //cs.FlushFinalBlock();//用于解决第二次获取小程序Session解密出错的情况


                        byte[] xXml = Convert.FromBase64String(Input);
                        byte[] msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                        Array.Copy(xXml, msg, xXml.Length);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    //cs.Dispose();
                    xBuff = decode2(ms.ToArray());
                }
            }
            catch (System.Security.Cryptography.CryptographicException e)
            {
                //Padding is invalid and cannot be removed.
                Console.WriteLine("===== CryptographicException =====");

                using (var ms = new MemoryStream())
                {
                    //cs 不自动释放，用于避免“Padding is invalid and cannot be removed”的错误    —— 2019.07.27 Jeffrey
                    var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write);
                    {
                        //cs.Read(decryptBytes, 0, decryptBytes.Length);
                        //cs.Close();
                        //ms.Close();

                        //cs.FlushFinalBlock();//用于解决第二次获取小程序Session解密出错的情况

                        byte[] xXml = Convert.FromBase64String(Input);
                        byte[] msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                        Array.Copy(xXml, msg, xXml.Length);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    //cs.Dispose();
                    xBuff = decode2(ms.ToArray());
                }
            }
            return xBuff;
        }

        private static byte[] decode2(byte[] decrypted)
        {
            int pad = (int)decrypted[decrypted.Length - 1];
            if (pad < 1 || pad > 32)
            {
                pad = 0;
            }
            byte[] res = new byte[decrypted.Length - pad];
            Array.Copy(decrypted, 0, res, 0, decrypted.Length - pad);
            return res;
        }


        #endregion

        /// <summary>
        /// 解密所有消息的基础方法
        /// </summary>
        /// <param name="sessionKey">储存在 SessionBag 中的当前用户 会话 SessionKey</param>
        /// <param name="encryptedData">接口返回数据中的 encryptedData 参数</param>
        /// <param name="iv">接口返回数据中的 iv 参数，对称解密算法初始向量</param>
        /// <param name="keySize">默认 128，</param>
        /// <returns></returns>
        public static string DecodeEncryptedData(string sessionKey, string encryptedData, string iv, int keySize = 128)
        {
            //var aesCipher = Convert.FromBase64String(encryptedData);
            var aesKey = Convert.FromBase64String(sessionKey);
            var aesIV = Convert.FromBase64String(iv);

            var result = AES_Decrypt(encryptedData, aesIV, aesKey, keySize);
            var resultStr = Encoding.UTF8.GetString(result);
            return resultStr;
        }

        /// <summary>
        /// 解密消息（通过SessionId获取）
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <exception cref="WxOpenException">当SessionId或SessionKey无效时抛出异常</exception>
        /// <returns></returns>
        public static string DecodeEncryptedDataBySessionId(string sessionId, string encryptedData, string iv)
        {
            var sessionBag = SessionContainer.GetSession(sessionId);
            if (sessionBag == null)
            {
                throw new WxOpenException("SessionId无效");
            }

            if (string.IsNullOrEmpty(sessionBag.SessionKey))
            {
                throw new WxOpenException("SessionKey无效");
            }

            var resultStr = DecodeEncryptedData(sessionBag.SessionKey, encryptedData, iv);
            return resultStr;
        }


        /// <summary>
        /// 检查解密消息水印
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="appId"></param>
        /// <returns>entity为null时也会返回false</returns>
        public static bool CheckWatermark(this DecodeEntityBase entity, string appId)
        {
            if (entity == null)
            {
                return false;
            }
            return entity.watermark.appid == appId;
        }

        #region 解密实例信息

        /// <summary>
        /// 解密到实例信息
        /// </summary>
        /// <typeparam name="T">DecodeEntityBase</typeparam>
        /// <param name="sessionId"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static T DecodeEncryptedDataToEntity<T>(string sessionId, string encryptedData, string iv)
        where T : DecodeEntityBase
        {
            var jsonStr = DecodeEncryptedDataBySessionId(sessionId, encryptedData, iv);

            //Console.WriteLine("===== jsonStr =====");
            //Console.WriteLine(jsonStr);
            //Console.WriteLine();

            var entity = SerializerHelper.GetObject<T>(jsonStr);
            return entity;
        }
        /// <summary>
        /// 解密到实例信息
        /// </summary>
        /// <typeparam name="T">DecodeEntityBase</typeparam>
        /// <param name="sessionKey"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static T DecodeEncryptedDataToEntityEasy<T>(string sessionKey, string encryptedData, string iv)
       where T : DecodeEntityBase
        {
            var jsonStr = DecodeEncryptedData(sessionKey, encryptedData, iv);

            //Console.WriteLine("===== jsonStr =====");
            //Console.WriteLine(jsonStr);
            //Console.WriteLine();

            var entity = SerializerHelper.GetObject<T>(jsonStr);
            return entity;
        }

        /// <summary>
        /// 解密UserInfo消息（通过SessionId获取）
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <exception cref="WxOpenException">当SessionId或SessionKey无效时抛出异常</exception>
        /// <returns></returns>
        public static DecodedUserInfo DecodeUserInfoBySessionId(string sessionId, string encryptedData, string iv)
        {
            return DecodeEncryptedDataToEntity<DecodedUserInfo>(sessionId, encryptedData, iv);
        }

        /// <summary>
        /// 解密手机号
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static DecodedPhoneNumber DecryptPhoneNumber(string sessionId, string encryptedData, string iv)
        {
            return DecodeEncryptedDataToEntity<DecodedPhoneNumber>(sessionId, encryptedData, iv);
        }
        /// <summary>
        /// 解密手机号(根据sessionKey解密)
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static DecodedPhoneNumber DecryptPhoneNumberBySessionKey(string sessionKey, string encryptedData, string iv)
        {
            //var resultStr = DecodeEncryptedData(sessionKey, encryptedData, iv);

            //var entity = SerializerHelper.GetObject<DecodedPhoneNumber>(resultStr);
            //return entity;

            return DecodeEncryptedDataToEntityEasy<DecodedPhoneNumber>(sessionKey, encryptedData, iv);
        }

        /// <summary>
        /// 解密微信小程序运动步数
        /// 2019-04-02
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static DecodedRunData DecryptRunData(string sessionId, string encryptedData, string iv)
        {
            return DecodeEncryptedDataToEntity<DecodedRunData>(sessionId, encryptedData, iv);
        }


        #endregion

        #endregion
    }
}
