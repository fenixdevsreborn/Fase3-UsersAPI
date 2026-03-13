namespace Fcg.Users.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid email or password.") : base(message) { }
}
