
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using AlonsoAdmin.Common.Auth;
using AlonsoAdmin.Common.Cache;
using AlonsoAdmin.Common.ResponseEntity;
using AlonsoAdmin.Common.Utils;
using AlonsoAdmin.Entities;
using AlonsoAdmin.HttpApi.Attributes;
using AlonsoAdmin.HttpApi.Auth;
using AlonsoAdmin.HttpApi.SwaggerHelper;
using AlonsoAdmin.MultiTenant.Extensions;
using AlonsoAdmin.Services.System.Interface;
using AlonsoAdmin.Services.System.Request;
using AlonsoAdmin.Services.System.Response;
using Hei.Captcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AlonsoAdmin.HttpApi.SwaggerHelper.CustomApiVersion;

namespace AlonsoAdmin.HttpApi.Controllers.V1.System
{
    /// <summary>
    /// 用户认证
    /// </summary>
    [Description("用户认证")]
    public class AuthController : ModuleBaseController
    {
        private IAuthToken _authToken;
        private readonly IAuthService _authService;
        private readonly SecurityCodeHelper _securityCode;
        private readonly ISysLoginLogService _loginLogService;
        private readonly ICache _cache;

        public AuthController(
            ICache cache,
            IAuthToken authToken,
            IAuthService authServices,
            ISysLoginLogService loginLogService,
            SecurityCodeHelper securityCodeHelper
            )
        {
            _authToken = authToken;
            _authService = authServices;
            _loginLogService = loginLogService;
            _securityCode = securityCodeHelper;
            _cache = cache;
        }

        /// <summary>
        /// 登录系统
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [NoOprationLog]
        [Description("登录验证")]
        public async Task<IResponseEntity<LoginResponse>> Login(AuthLoginRequest req)
        {

            var sw = new Stopwatch();
            sw.Start();
            var res = (await _authService.LoginAsync(req)) as IResponseEntity;
            sw.Stop();

            if (!res.Success)
            {
                return ResponseEntity.Error<LoginResponse>(res.Message);
            }
            else
            {

                var user = (res as IResponseEntity<AuthLoginResponse>).Data;


                #region 写登录日志

                string ua = HttpContext.Request.Headers["User-Agent"];
                var client = UAParser.Parser.GetDefault().Parse(ua);
                var device = client.Device.Family;
                var loginLogAddRequest = new LoginLogAddRequest()
                {
                    CreatedBy = user.Id,
                    CreatedByName = user.UserName,
                    RealName = user.DisplayName,
                    ElapsedMilliseconds = sw.ElapsedMilliseconds,
                    Status = res.Success,
                    Message = res.Message,

                    Browser = client.UA.Family,
                    Os = client.OS.Family,
                    Device = device.ToLower() == "other" ? "" : device,
                    BrowserInfo = ua,
                    Ip = IPHelper.GetIP(HttpContext?.Request)
                };

                await _loginLogService.CreateAsync(loginLogAddRequest);
                #endregion

                #region 构造JWT Token
                var claims = new Claim[]{
                    new Claim(ClaimAttributes.UserId, user.Id.ToString()),
                    new Claim(ClaimAttributes.UserName, user.UserName),
                    new Claim(ClaimAttributes.DisplayName,user.DisplayName??""),
                    new Claim(ClaimAttributes.PermissionId,user.PermissionId??""),
                    new Claim(ClaimAttributes.GroupId,user.GroupId??"")
                };
                var token = _authToken.Build(claims);
                #endregion

                var data = new LoginResponse
                {
                    Token = token,
                    Uuid = user.Id,
                    Info = new UserInfoResponse
                    {
                        Id = user.Id,
                        Name = user.UserName,
                        DisplayName = user.DisplayName,
                        Avatar = user.Avatar,
                        Menus = user.Menus,
                        FunctionPoints = user.FunctionPoints
                    }
                };

                return ResponseEntity.Ok<LoginResponse>(data);
            }
        }




        /// <summary>
        /// 获取图片验证码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [NoOprationLog]
        [HttpGet]
        public async Task<IResponseEntity<CaptchaResponse>> Captcha()
        {
            var code = _securityCode.GetRandomEnDigitalText(4);
            var UUID = Guid.NewGuid();
            await _cache.SetAsync(UUID.ToString("N"), code, TimeSpan.FromMinutes(5));
            var imgbyte = await _securityCode.GetEnDigitalCodeByteAsync(code);

            var base64 = Convert.ToBase64String(imgbyte);

            return ResponseEntity.Ok(new CaptchaResponse { Img = base64, UUID = UUID.ToString("N") });
        }

        /// <summary>
        /// 刷新用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [NoOprationLog]
        [Description("刷新用户信息-根据Token")]
        public async Task<IResponseEntity<LoginResponse>> GetUserInfo()
        {
            var res = await _authService.GetUserInfoAsync();
            var user = (res as IResponseEntity<AuthLoginResponse>).Data;
            if (!res.Success)
            {
                return ResponseEntity.Error<LoginResponse>(res.Message);
            }

            #region 构造JWT Token
            var claims = new Claim[]{
                    new Claim(ClaimAttributes.UserId, user.Id.ToString()),
                    new Claim(ClaimAttributes.UserName, user.UserName),
                    new Claim(ClaimAttributes.DisplayName,user.DisplayName??""),
                    new Claim(ClaimAttributes.PermissionId,user.PermissionId??""),
                    new Claim(ClaimAttributes.GroupId,user.GroupId??"")
                };
            var token = _authToken.Build(claims);
            #endregion


            var data = new LoginResponse
            {
                Uuid = user.Id,
                Token = token,
                Info = new UserInfoResponse
                {
                    Id = user.Id,
                    Name = user.UserName,
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    Menus = user.Menus,
                    FunctionPoints = user.FunctionPoints
                }

            };

            return ResponseEntity.Ok<LoginResponse>(data);
        }

        /// <summary>
        /// 获取用户数据权限组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Description("获取用户数据权限组-根据Token")]
        public async Task<IResponseEntity<List<GroupForListResponse>>> GetUserGroups()
        {
            return await _authService.GetUserGroupsAsync();
        }

    }
}