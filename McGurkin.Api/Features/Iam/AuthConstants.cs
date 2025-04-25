namespace McGurkin.Api.Features.Iam;

public static class AuthConstants
{
    public static class ErrorMessages
    {
        public const string UserNotFound = "Unable to load user.";
        public const string InvalidCredentials = "Invalid credentials.";
        public const string AccountLockedOut = "Account is locked out.";
        public const string EmailConfirmationFailed = "Email confirmation failed.";
        public const string RegistrationFailed = "Registration failed.";
        public const string PasswordResetFailed = "Password reset failed.";
    }

    public static class EmailTemplates
    {
        public const string ConfirmEmailSubject = "Confirm your email";
        public const string ResetPasswordSubject = "Reset Password";
    }

    public static class Routes
    {
        public const string ConfirmEmail = "/iam/confirm-email";
        public const string ResetPassword = "/iam/reset-password";
    }
}
