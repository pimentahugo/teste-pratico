namespace OrderTracking.Shared.Results;
public class Result<T>
{
	public bool IsSucess { get; }
	public string? Message { get; }
	public IReadOnlyCollection<string>? Errors { get; }
	public T? Data { get; }

	private Result(bool success, string? message = null, IEnumerable<string>? errors = null, T? data = default)
	{
		IsSucess = success;
		Message = message;
		Errors = errors?.ToList();
		Data = data;
	}

	public static Result<T> Ok(T? data = default, string? message = null)
		=> new(true, message, null, data);

	public static Result<T> Fail(string message)
		=> new(false, message);

	public static Result<T> Fail(IEnumerable<string> errors, string? message = null)
		=> new(false, message, errors);

	public static Result<T> Unauthorized(string? message = null)
		=> new(false, message);
}