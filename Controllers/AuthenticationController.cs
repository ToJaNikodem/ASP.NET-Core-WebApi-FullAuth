using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FullAuth.Dtos.Email;
using FullAuth.Dtos.User;
using FullAuth.Models;
using FullAuth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;

namespace FullAuth.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        public AuthenticationController(UserManager<User> userManager, ITokenService tokenService, IEmailService emailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignUpRequest signUpRequest)
        {
            try
            {
                if (await _userManager.FindByNameAsync(signUpRequest.UserName) != null)
                {
                    var errorList = new
                    {
                        errors = new
                        {
                            userName = new string[] { "Username already taken!" }
                        }
                    };
                    return BadRequest(errorList);
                }

                if (await _userManager.FindByEmailAsync(signUpRequest.Email) != null)
                {
                    var errorList = new
                    {
                        errors = new
                        {
                            email = new string[] { "Email already taken!" }
                        }
                    };
                    return BadRequest(errorList);
                }

                var appUser = new User
                {
                    UserName = signUpRequest.UserName,
                    Email = signUpRequest.Email
                };

                var createdUser = await _userManager.CreateAsync(appUser, signUpRequest.Password);
                if (!createdUser.Succeeded)
                {
                    return StatusCode(500);
                }

                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                if (!roleResult.Succeeded)
                {
                    return StatusCode(500);
                }

                var user = await _userManager.FindByNameAsync(signUpRequest.UserName);

                if (user == null)
                {
                    return StatusCode(500);
                }

                await SendVerification(user);

                return Ok("User created successfully!");

            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInRequest logInRequest)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(logInRequest.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(logInRequest.UserNameOrEmail);

                if (user == null)
                {
                    return BadRequest("Invalid credentials!");
                }

                if (!await _userManager.CheckPasswordAsync(user, logInRequest.Password))
                {
                    return BadRequest("Invalid credentials!");
                }

                await Send2fa(user);

                var loginToken = _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "loginToken");
                var encodedToken = HttpUtility.UrlEncode(loginToken.Result);

                return Ok(new TwoFactorAuthTokenResponse
                {
                    LoginToken = encodedToken,
                    UserId = user.Id
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("login-2fa")]
        public async Task<IActionResult> Login2fa([FromBody] LogIn2FaRequest logIn2FaRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(logIn2FaRequest.UserId);

                if (user == null)
                {
                    return BadRequest("Invalid user!");
                }

                if (!await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, logIn2FaRequest.Code))
                {
                    return BadRequest("Invalid verification code!");
                }

                if (!await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "loginToken", logIn2FaRequest.LoginToken))
                {
                    return BadRequest("Invalid token!");
                }

                (string accessToken, string refreshToken) = _tokenService.CreateTokens(user);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                await _userManager.UpdateAsync(user);

                return Ok(new TokensResponse
                {
                    RefreshToken = refreshToken,
                    AccessToken = accessToken
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(refreshTokenRequest.UserId);
                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!(refreshTokenRequest.RefreshToken == user.RefreshToken))
                {
                    return BadRequest("Invalid refresh token!");
                }

                if (user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest("Token expired!");
                }

                (string accessToken, string refreshToken) = _tokenService.CreateTokens(user);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                await _userManager.UpdateAsync(user);

                return Ok(new TokensResponse
                {
                    RefreshToken = refreshToken,
                    AccessToken = accessToken
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogOutRequest logOutRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(logOutRequest.UserId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _userManager.UpdateAsync(user);

                return Ok("User logged out successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteUserRequest deleteUserRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(deleteUserRequest.UserId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!await _userManager.CheckPasswordAsync(user, deleteUserRequest.Password))
                {
                    return BadRequest("Wrong password!");
                }

                await _userManager.DeleteAsync(user);
                return Ok("User deleted successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpPost("username-change")]
        public async Task<IActionResult> UsernameChange([FromBody] UsernameChangeRequest usernameChangeRequest)
        {
            try
            {
                if (await _userManager.FindByNameAsync(usernameChangeRequest.NewUserName) != null)
                {
                    var errorList = new
                    {
                        errors = new
                        {
                            newUserName = new string[] { "Username already taken!" }
                        }
                    };
                    return BadRequest(errorList);
                }

                var user = await _userManager.FindByIdAsync(usernameChangeRequest.UserId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                user.UserName = usernameChangeRequest.NewUserName;

                await _userManager.UpdateAsync(user);
                return Ok("Username changed successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpPost("password-change")]
        public async Task<IActionResult> PasswordChange([FromBody] PasswordChangeRequest passwordChangeRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(passwordChangeRequest.UserId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!await _userManager.CheckPasswordAsync(user, passwordChangeRequest.OldPassword))
                {
                    return BadRequest("Invalid password!");
                }

                var result = await _userManager.ChangePasswordAsync(user, passwordChangeRequest.OldPassword, passwordChangeRequest.NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok("Password changed successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("email-verification")]
        public async Task<IActionResult> EmailVerification([FromBody] EmailVerificationRequest emailVerificationRequest)
        {
            try
            {
                byte[] decodedUserId = WebEncoders.Base64UrlDecode(emailVerificationRequest.EncodedUserId);
                string userId = Encoding.UTF8.GetString(decodedUserId);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                var result = await _userManager.ConfirmEmailAsync(user, emailVerificationRequest.VerificationToken);

                if (!result.Succeeded)
                {
                    return BadRequest("Invalid token!");
                }

                return Ok("Email verified successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [Authorize]
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest resendVerificationEmailRequest)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(resendVerificationEmailRequest.UserId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (user.EmailConfirmed)
                {
                    return BadRequest("Email already confirmed!");
                }

                await SendVerification(user);

                return Ok("Verification email resend successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("send-password-reset-email")]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendPasswordResetEmailRequest sendPasswordResetEmailRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(sendPasswordResetEmailRequest.Email);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                await SendPasswordReset(user);

                return Ok("Rest password email send successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }

        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset([FromBody] PasswordResetRequest passwordResetRequest)
        {
            try
            {
                byte[] decodedUserId = WebEncoders.Base64UrlDecode(passwordResetRequest.EncodedUserId);
                string userId = Encoding.UTF8.GetString(decodedUserId);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                var result = await _userManager.ResetPasswordAsync(user, passwordResetRequest.ResetToken, passwordResetRequest.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest("Invalid link!");
                }

                return Ok("Password successfully reseted!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        private async Task SendVerification(User user)
        {
            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedVerificationToken = HttpUtility.UrlEncode(verificationToken.ToString());
            var encodedUserId = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Id.ToString()));
            var url = "http://localhost:8080/verify/" + encodedUserId + "/" + encodedVerificationToken;

            var emailData = new EmailDataDto
            {
                EmailTo = user.Email!,
                Subject = "Verify your email!",
                TemplateName = "email-verification.html",
                TemplateData = new Dictionary<string, string>
                {
                    {"{{ username }}", user.UserName! },
                    {"{{ url }}", url },
                }
            };

            await _emailService.SendEmailAsync(emailData);
        }

        private async Task SendPasswordReset(User user)
        {
            var verificationToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedVerificationToken = HttpUtility.UrlEncode(verificationToken.ToString());
            var encodedUserId = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Id.ToString()));
            var url = "http://localhost:8080/password-reset/" + encodedUserId + "/" + encodedVerificationToken;

            var emailData = new EmailDataDto
            {
                EmailTo = user.Email!,
                Subject = "Reset your password!",
                TemplateName = "password-reset.html",
                TemplateData = new Dictionary<string, string>
                {
                    {"{{ username }}", user.UserName! },
                    {"{{ url }}", url },
                }
            };

            await _emailService.SendEmailAsync(emailData);
        }

        private async Task Send2fa(User user)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

            var emailData = new EmailDataDto
            {
                EmailTo = user.Email!,
                Subject = "Your two factor authentication code!",
                TemplateName = "two-factor-authentication-code.html",
                TemplateData = new Dictionary<string, string>
                {
                    {"{{ username }}", user.UserName! },
                    {"{{ code }}", code },
                }
            };

            await _emailService.SendEmailAsync(emailData);
        }
    }
}