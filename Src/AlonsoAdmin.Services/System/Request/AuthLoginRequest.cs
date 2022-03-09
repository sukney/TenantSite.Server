using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AlonsoAdmin.Services.System.Request
{
    public class AuthLoginRequest
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空！")]
        [Display(Name = "账号")]
        [StringLength(22, MinimumLength = 1, ErrorMessage = "")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Display(Name = "密码")]
        [Required(ErrorMessage = "密码不能为空！")]
        public string Password { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "验证码")]
        [MaxLength(6,ErrorMessage = "{0}不到超过{1}个字符")]
        public string? VerificationCode { get; set; }

        /// <summary>
        /// 验证码UUID
        /// </summary>
        [Display(Name = "验证码UUID")]
        [MaxLength(50,ErrorMessage = "{0}不到超过{1}个字符")]
        public string? UUID { get; set; }

    }
}
