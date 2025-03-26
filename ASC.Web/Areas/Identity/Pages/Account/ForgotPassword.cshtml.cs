﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using ASC.Utilities;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                var userEmail = HttpContext.User.GetCurrentUserDetails().Email;
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại");
                    return Page();
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { userId = user.Id, code = encodedCode, email = userEmail },
                    protocol: Request.Scheme);
                Console.WriteLine($"Token: {code}");
                Console.WriteLine($"Encoded Token: {encodedCode}");
                Console.WriteLine($"Callback URL: {callbackUrl}");
                await _emailSender.SendEmailAsync(Input.Email, "Reset Password",
                    $"Please reset your password by clicking here: <a href='{callbackUrl}'>Link</a>");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
