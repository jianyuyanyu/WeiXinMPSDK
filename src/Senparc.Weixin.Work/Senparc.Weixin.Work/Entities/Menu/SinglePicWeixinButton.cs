﻿/*----------------------------------------------------------------
    Copyright (C) 2025 Senparc
    
    文件名：SinglePicWeixinButton.cs
    文件功能描述：调起微信相册按钮
    
    
    创建标识：Senparc - 20150313
    
    修改标识：Senparc - 20150313
    修改描述：整理接口

    修改标识：Senparc - 20181005
    修改描述：菜单按钮类型（ButtonType）改为使用 Senparc.NeuChar.MenuButtonType
----------------------------------------------------------------*/
using Senparc.NeuChar;

namespace Senparc.Weixin.Work.Entities.Menu
{
    /// <summary>
    /// 单个按键
    /// </summary>
    public class SinglePicWeixinButton : SingleButton
    {
        /// <summary>
        /// 类型为pic_weixin时必须。
        /// 用户点击按钮后，微信客户端将调起微信相册，完成选择操作后，将选择的相片发送给开发者的服务器，并推送事件给开发者，同时收起相册，随后可能会收到开发者下发的消息。
        /// 仅支持微信iPhone5.4.1以上版本，和Android5.4以上版本的微信用户，旧版本微信用户点击后将没有回应，开发者也不能正常接收到事件推送。
        /// </summary>
        public string key { get; set; }

        public SinglePicWeixinButton()
            : base(MenuButtonType.pic_weixin.ToString())
        {
        }
    }
}
