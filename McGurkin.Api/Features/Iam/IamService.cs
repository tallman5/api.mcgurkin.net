using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Iam.Data.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using RegisterRequest = McGurkin.Api.Features.Iam.Data.Requests.RegisterRequest;
using ResetPasswordRequest = McGurkin.Api.Features.Iam.Data.Requests.ResetPasswordRequest;

namespace McGurkin.Api.Features.Iam
{
    public interface IIamService
    {
        Task<string> ChangePasswordAsync(ChangePasswordRequest request, ClaimsPrincipal user);
        Task<string> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<string> DeleteAccountAsync(SignInRequest request, ClaimsPrincipal user);
        Task<Dictionary<string, string>> DownloadMyDataAsync(ClaimsPrincipal user);
        Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
        Task<Token> GetGuestTokenAsync(bool expired = false);
        Task<string> RegisterAsync(RegisterRequest registerRequest);
        Task<string> ResendConfirmationAsync(ResendConfirmationRequest request);
        Task<string> ResetPasswordAsync(ResetPasswordRequest request);
        Task<Token> SignInAsync(SignInRequest signInRequest);
    }

    public class IamService(
        IConfiguration configuration,
        IEmailSender emailSender,
        ILogger<IamService> logger,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager) : IIamService
    {
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IamServiceConfig _iamServiceConfig = IamServiceConfig.FromConfiguration(configuration);
        private readonly ILogger<IamService> _logger = logger;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(30);

        public async Task<string> ChangePasswordAsync(ChangePasswordRequest request, ClaimsPrincipal principal)
        {
            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            var user = await _userManager.GetUserAsync(principal);
            if (null == user)
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return "Failed to change password.";
            }

            return "Password changed successfully.";
        }

        public async Task<string> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return AuthConstants.ErrorMessages.UserNotFound;
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                return AuthConstants.ErrorMessages.EmailConfirmationFailed;
            }

            return "Thank you for confirming your email!";
        }

        public async Task<string> DeleteAccountAsync(SignInRequest request, ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal) ??
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.InvalidCredentials);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to delete user with ID '{user.Id}'");
            }

            await _signInManager.SignOutAsync();
            return "Successfully deleted account.";
        }

        public async Task<Dictionary<string, string>> DownloadMyDataAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal) ??
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(IdentityUser).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(PersonalDataAttribute)));

            foreach (var prop in personalDataProps)
            {
                personalData.Add(prop.Name, prop.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            if (logins != null)
            {
                foreach (var login in logins)
                    personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
            }
            return personalData;
        }

        public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return "Please check your email to reset your password.";
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{origin}?code={code}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                AuthConstants.EmailTemplates.ResetPasswordSubject,
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return "Please check your email to reset your password.";
        }

        private async Task<Token> GenerateAuthTokenAsync(IdentityUser user, DateTimeOffset expires)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateJwtToken(user, roles, expires);

            return new Token
            {
                Expires = expires,
                Roles = [.. roles],
                AccessToken = accessToken,
                UserName = user.UserName ?? throw new UnauthorizedAccessException()
            };
        }

        private string GenerateJwtToken(IdentityUser user, IList<string> roles, DateTimeOffset expires)
        {
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? throw new UnauthorizedAccessException()),
                new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_iamServiceConfig.IssuerKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _iamServiceConfig.Issuer,
                audience: _iamServiceConfig.Issuer,
                claims: claims,
                expires: expires.UtcDateTime,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Token> GetGuestTokenAsync(bool expired = false)
        {
            var user = await _userManager.FindByEmailAsync("guest@kixvu.com") ??
                throw new Exception("Guest user not found");

            var expiry = expired ? DateTimeOffset.Now.AddSeconds(1) : DateTimeOffset.Now.AddYears(1);
            return await GenerateAuthTokenAsync(user, expiry);
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            var user = new IdentityUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return AuthConstants.ErrorMessages.RegistrationFailed;
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{request.Origin}?userId={user.Id}&code={code}";

            await _emailSender.SendEmailAsync(
                user.Email,
                AuthConstants.EmailTemplates.ConfirmEmailSubject,
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return "We've sent you an email to confirm your address.";
        }

        public async Task<string> ResendConfirmationAsync(ResendConfirmationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ""; // Don't reveal that user doesn't exist
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{request.Origin}?userId={user.Id}&code={code}";

            await _emailSender.SendEmailAsync(
                request.Email,
                AuthConstants.EmailTemplates.ConfirmEmailSubject,
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return "";
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return "Your password has been reset.";
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, request.Password);

            if (!result.Succeeded)
            {
                return AuthConstants.ErrorMessages.PasswordResetFailed;
            }

            return "Your password has been reset. Please sign in with your new password.";
        }

        public async Task<Token> SignInAsync(SignInRequest signInRequest)
        {
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(
                    signInRequest.UserName,
                    signInRequest.Password,
                    signInRequest.RememberMe,
                    lockoutOnFailure: true);

                if (signInResult.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(signInRequest.UserName) ??
                        throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

                    var token = await GenerateAuthTokenAsync(user, DateTimeOffset.Now.Add(TokenLifetime));
                    return token;
                }

                if (signInResult.IsLockedOut)
                {
                    _logger.LogWarning("Account {UserName} is locked out.", signInRequest.UserName);
                    throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.AccountLockedOut);
                }

                _logger.LogWarning("Invalid login attempt for {UserName}", signInRequest.UserName);
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.InvalidCredentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing in user {UserName}", signInRequest.UserName);
                throw;
            }
        }
    }
}