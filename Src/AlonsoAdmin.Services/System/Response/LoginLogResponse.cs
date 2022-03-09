using AlonsoAdmin.Entities.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlonsoAdmin.Services.System.Response
{
    /// <summary>
    /// ForList 实体对象（一般用于列表页展示数据用）
    /// </summary>
    public class LoginLogForListResponse : SysLoginLogEntity
    {

    }
    /// <summary>
    /// ForIem 实体对象（一般用于明细页展示数据用）
    /// </summary>
    public class LoginLogForItemResponse : SysLoginLogEntity
    {

    }


    /// <summary>
    /// 登录信息
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 帐号ID
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// 帐号信息
        /// </summary>
        public UserInfoResponse Info { get; set; }

    }
    /// <summary>
    /// 用户
    /// </summary>
    public class UserInfoResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 菜单 
        /// </summary>
        public List<ResourceForMenuResponse> Menus { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public List<string> FunctionPoints { get; set; }
    }

    /// <summary>
    /// 验证码图片。
    /// </summary>
    public class CaptchaResponse
    {
        /// <summary>
        /// 图片base64
        /// </summary>
        public string Img { get; set; }
        /// <summary>
        /// UUID
        /// </summary>
        public string UUID { get; set; }

    }
}
