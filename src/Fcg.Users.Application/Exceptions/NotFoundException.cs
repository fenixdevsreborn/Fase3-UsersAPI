namespace Fcg.Users.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string resource, object key) : base($"{resource} with id '{key}' was not found.") { }
}
