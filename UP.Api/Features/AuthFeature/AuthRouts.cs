namespace UP.Api.Features.AuthFeature
{
    public static class AuthRouts
    {
        private const string Base = "api/auth";

        public const string Login = $"{Base}/login";
        public const string Refresh = $"{Base}/refresh";
        public const string Register = $"{Base}/register";
    }
}
