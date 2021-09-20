using FluentValidation.Results;
using System;
using System.Threading.Tasks;

namespace Platformex
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ValidationResult ValidationResult { get; }

        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public Result(ValidationResult result)
        {
            IsSuccess = result.IsValid;
            ValidationResult = result;
            Error = result.ToString();
        }

        public static Result Success => new(true, null);
        public static async Task<Result> SucceedAsync(Func<Task> func)
        {
            await func();
            return new Result(true, null);
        }

        public static Result Fail(string message) => new(false, message);

        public static UnauthorizedResult Unauthorized(string message) => new(message);
        public static ForbiddenResult Forbidden(string message) => new(message);
    }

    public class UnauthorizedResult : Result
    {
        public UnauthorizedResult(string error) : base(false, error)
        {
        }
    }
    public class ForbiddenResult : Result
    {
        public ForbiddenResult(string error) : base(false, error)
        {
        }
    }

}