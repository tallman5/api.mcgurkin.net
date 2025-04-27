using McGurkin.Api.Features.Iam.Data;
using McGurkin.Api.Features.Iam.Data.Requests;
using McGurkin.ServiceProviders;
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
        Task<Response<string>> ChangePasswordAsync(ChangePasswordRequest request, ClaimsPrincipal user);
        Task<Response<string>> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<Response<string>> DeleteAccountAsync(SignInRequest request, ClaimsPrincipal user);
        Task<Response<Dictionary<string, string>>> DownloadMyDataAsync(ClaimsPrincipal user);
        Task<Response<string>> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
        Task<Token> GetGuestTokenAsync(bool expired = false);
        Task<Response<string>> RegisterAsync(RegisterRequest registerRequest);
        Task<Response<string>> ResendConfirmationAsync(ResendConfirmationRequest request);
        Task<Response<string>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<Response<Token>> SignInAsync(SignInRequest signInRequest);
    }

    public class IamService(
        IConfiguration configuration,
        IEmailSender emailSender,
        ILogger<IamService> logger,
        SignInManager<IamUser> signInManager,
        UserManager<IamUser> userManager) : IIamService
    {
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IamServiceConfig _iamServiceConfig = IamServiceConfig.FromConfiguration(configuration);
        private readonly ILogger<IamService> _logger = logger;
        private readonly SignInManager<IamUser> _signInManager = signInManager;
        private readonly UserManager<IamUser> _userManager = userManager;
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(30);

        public async Task<Response<string>> ChangePasswordAsync(ChangePasswordRequest request, ClaimsPrincipal principal)
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
                var response = Response<string>.Error("Failed to change password.");
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            return Response<string>.Success("Password changed successfully.");
        }

        public async Task<Response<string>> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Response<string>.Error(AuthConstants.ErrorMessages.UserNotFound);
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                return Response<string>.Error(AuthConstants.ErrorMessages.EmailConfirmationFailed);
            }

            return Response<string>.Success("Thank you for confirming your email!");
        }

        public async Task<Response<string>> DeleteAccountAsync(SignInRequest request, ClaimsPrincipal principal)
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
            return Response<string>.Success("Successfully deleted account.");
        }

        public async Task<Response<Dictionary<string, string>>> DownloadMyDataAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal) ??
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(IamUser).GetProperties()
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
            return Response<Dictionary<string, string>>.Success(personalData);
        }

        public async Task<Response<string>> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Response<string>.Success("Please check your email to reset your password.");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{origin}?code={code}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                AuthConstants.EmailTemplates.ResetPasswordSubject,
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return Response<string>.Success("Please check your email to reset your password.");
        }

        public async Task<Token> GetGuestTokenAsync(bool expired = false)
        {
            var user = await _userManager.FindByEmailAsync("guest@kixvu.com") ??
                throw new Exception("Guest user not found");

            var expiry = expired ? DateTimeOffset.Now.AddSeconds(1) : DateTimeOffset.Now.AddYears(1);
            return await GenerateAuthTokenAsync(user, expiry);
        }

        public async Task<Response<string>> RegisterAsync(RegisterRequest request)
        {
            var user = new IamUser
            {
                UserName = request.Email,
                Email = request.Email,
                ScreenName = request.ScreenName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var response = Response<string>.Error(AuthConstants.ErrorMessages.RegistrationFailed);
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{request.Origin}?userId={user.Id}&code={code}";

            await _emailSender.SendEmailAsync(
                user.Email,
                AuthConstants.EmailTemplates.ConfirmEmailSubject,
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return Response<string>.Success("We've sent you an email to confirm your address.");
        }

        public async Task<Response<string>> ResendConfirmationAsync(ResendConfirmationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Response<string>.Success(""); // Don't reveal that user doesn't exist
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{request.Origin}?userId={user.Id}&code={code}";

            await _emailSender.SendEmailAsync(
                request.Email,
                AuthConstants.EmailTemplates.ConfirmEmailSubject,
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return Response<string>.Success("");
        }

        public async Task<Response<string>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Response<string>.Success("Your password has been reset.");
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, request.Password);

            if (!result.Succeeded)
            {
                var response = Response<string>.Error(AuthConstants.ErrorMessages.PasswordResetFailed);
                response.Errors.AddRange(result.Errors.Select(e => e.Description));
                return response;
            }

            return Response<string>.Success("Your password has been reset. Please sign in with your new password.");
        }

        public async Task<Response<Token>> SignInAsync(SignInRequest signInRequest)
        {
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(
                    signInRequest.Email,
                    signInRequest.Password,
                    signInRequest.RememberMe,
                    lockoutOnFailure: true);

                if (signInResult.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in.", signInRequest.Email);
                    var user = await _userManager.FindByEmailAsync(signInRequest.Email) ??
                        throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.UserNotFound);

                    var token = await GenerateAuthTokenAsync(user, DateTimeOffset.Now.Add(TokenLifetime));
                    return Response<Token>.Success(token);
                }

                if (signInResult.IsLockedOut)
                {
                    _logger.LogWarning("Account {Email} is locked out.", signInRequest.Email);
                    throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.AccountLockedOut);
                }

                _logger.LogWarning("Invalid login attempt for {Email}", signInRequest.Email);
                throw new UnauthorizedAccessException(AuthConstants.ErrorMessages.InvalidCredentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing in user {Email}", signInRequest.Email);
                throw;
            }
        }

        private async Task<Token> GenerateAuthTokenAsync(IamUser user, DateTimeOffset expires)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateJwtToken(user, roles, expires);

            return new Token
            {
                Expires = expires,
                Roles = [.. roles],
                ScreenName = user.ScreenName,
                AccessToken = accessToken,
                UserName = user.UserName ?? user.ScreenName,
            };
        }

        private string GenerateJwtToken(IamUser user, IList<string> roles, DateTimeOffset expires)
        {
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.ScreenName),
                new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(CustomClaimTypes.ScreenName, user.ScreenName ?? string.Empty)
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
    }
}