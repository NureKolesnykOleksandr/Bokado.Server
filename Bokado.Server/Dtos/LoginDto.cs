namespace Bokado.Server.Dtos
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GoogleLoginDTO
    {
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }

    public class RegisterDTO : LoginDTO
    {
        public string Username { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class AuthResultDTO
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }
}
