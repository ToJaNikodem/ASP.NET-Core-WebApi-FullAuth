using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FullAuth.Dtos.User;
using FullAuth.Models;
using Microsoft.AspNetCore.Identity;
using FullAuth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using FullAuth.Dtos.Email;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace FullAuth.Controllers
{
    [Route("api/user")]
    [ApiController]
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

        // public class ErrorList
        // {
        //     public required List<UserNameErrorDetail> Errors { get; set; }
        // }

        // public class UserNameErrorDetail
        // {
        //     public required List<ErrorDetail> UserNameErrors { get; set; }
        // }

        // public class ErrorDetail
        // {
        //     public string? Message { get; set; }
        // }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignUpDto signUpDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _userManager.FindByNameAsync(signUpDto.UserName) != null)
                {
                    var errorList = new
                    {
                        errors = new
                        {
                            UserName = new string[] { "Username already taken!" }
                        }
                    };
                    return BadRequest(errorList);
                }

                if (await _userManager.FindByEmailAsync(signUpDto.Email) != null)
                {
                    return BadRequest("Email already taken!");
                }

                var appUser = new User
                {
                    UserName = signUpDto.UserName,
                    Email = signUpDto.Email
                };

                var createdUser = await _userManager.CreateAsync(appUser, signUpDto.Password);

                if (!createdUser.Succeeded)
                {
                    return StatusCode(500, createdUser.Errors);
                }

                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                if (!roleResult.Succeeded)
                {
                    return StatusCode(500, roleResult.Errors);
                }

                var user = await _userManager.FindByNameAsync(signUpDto.UserName);

                if (user == null)
                {
                    return StatusCode(500);
                }

                await SendVerification(user);

                return Ok(new NewUserDto
                {
                    UserName = appUser.UserName,
                    Email = appUser.Email
                });

            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInDto logInDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(logInDto.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(logInDto.UserNameOrEmail);

                if (user == null)
                {
                    return BadRequest();
                }

                if (!await _userManager.CheckPasswordAsync(user, logInDto.Password))
                {
                    return BadRequest();
                }
                (string accessToken, string refreshToken) = _tokenService.CreateTokens(user);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                await _userManager.UpdateAsync(user);

                return Ok(new TokensDto
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(refreshTokenDto.UserName);
                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!(refreshTokenDto.RefreshToken == user.RefreshToken))
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

                return Ok(new TokensDto
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
        public async Task<IActionResult> Logout([FromBody] LogOutDto logOutDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(logOutDto.UserName);

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
        public async Task<IActionResult> Delete([FromBody] DeleteUserDto deleteUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(deleteUserDto.UserName);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!await _userManager.CheckPasswordAsync(user, deleteUserDto.Password))
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
        public async Task<IActionResult> UsernameChange([FromBody] UsernameChangeDto usernameChangeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(usernameChangeDto.OldUserName);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                user.UserName = usernameChangeDto.NewUserName;

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
        public async Task<IActionResult> PasswordChange([FromBody] PasswordChangeDto passwordChangeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(passwordChangeDto.UserName);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                if (!await _userManager.CheckPasswordAsync(user, passwordChangeDto.OldPassword))
                {
                    return BadRequest("Invalid password!");
                }

                await _userManager.ChangePasswordAsync(user, passwordChangeDto.OldPassword, passwordChangeDto.NewPassword);

                return Ok("Password changed successfully!");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("email-verification")]
        public async Task<IActionResult> EmailVerification([FromBody] EmailVerificationDto emailVerificationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                byte[] decodedUserId = WebEncoders.Base64UrlDecode(emailVerificationDto.EncodedUserId);
                string userId = Encoding.UTF8.GetString(decodedUserId);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                var result = await _userManager.ConfirmEmailAsync(user, emailVerificationDto.VerificationToken);

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
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto resendVerificationEmailDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByNameAsync(resendVerificationEmailDto.UserName);

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
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendPasswordResetEmailDto sendPasswordResetEmailDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(sendPasswordResetEmailDto.Email);

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
        public async Task<IActionResult> PasswordReset([FromBody] PasswordResetDto passwordResetDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                byte[] decodedUserId = WebEncoders.Base64UrlDecode(passwordResetDto.EncodedUserId);
                string userId = Encoding.UTF8.GetString(decodedUserId);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest("User doesn't exist!");
                }

                var result = await _userManager.ResetPasswordAsync(user, passwordResetDto.ResetToken, passwordResetDto.NewPassword);

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
                TemplateName = "email-verification-email.html",
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
                TemplateName = "password-reset-email.html",
                TemplateData = new Dictionary<string, string>
                {
                    {"{{ username }}", user.UserName! },
                    {"{{ url }}", url },
                }
            };

            await _emailService.SendEmailAsync(emailData);
        }
    }
}