namespace Shared.Common;

/// <summary>
/// کلاس پایه برای نتیجه عملیات
/// این کلاس برای مدیریت موفق یا ناموفق بودن عملیات استفاده می‌شود
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public bool IsFailure => !IsSuccess;
    public string ErrorMessage { get; protected set; } = string.Empty;

    protected Result(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string errorMessage) => new(false, errorMessage);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string errorMessage) => new(default!, false, errorMessage);
}

/// <summary>
/// کلاس نتیجه با مقدار بازگشتی
/// </summary>
public class Result<T> : Result
{
    public T Value { get; private set; }

    protected internal Result(T value, bool isSuccess, string errorMessage) : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}

