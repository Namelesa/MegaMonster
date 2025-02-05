namespace MegaMonster.Services.Favors.Application.OperationResult;

public class ResultOperation
{
    public bool Success { get; }
    public string? Message { get; }

    private ResultOperation(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }

    public static ResultOperation Ok() => new(true);
    public static ResultOperation Fail(string message) => new(false, message);
}