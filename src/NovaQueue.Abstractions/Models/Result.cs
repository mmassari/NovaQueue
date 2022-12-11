using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaQueue.Abstractions.Models
{
	public abstract class Result
	{
		public bool Success { get; protected set; }
		public bool Failure => !Success;
	}

	public abstract class Result<T> : Result
	{
		private T _data;

		protected Result(T data)
		{
			Data = data;
		}

		public T Data
		{
			get => Success ? _data : throw new Exception($"You can't access .{nameof(Data)} when .{nameof(Success)} is false");
			set => _data = value;
		}
	}

	public class SuccessResult : Result
	{
		public SuccessResult()
		{
			Success = true;
		}
	}

	public class SuccessResult<T> : Result<T>
	{
		public SuccessResult(T data) : base(data)
		{
			Success = true;
		}
	}

	public class ErrorResult : Result, IErrorResult
	{
		public ErrorResult(string message) : this(message, Array.Empty<Error>())
		{
		}

		public ErrorResult(string message, IReadOnlyCollection<Error> errors)
		{
			Message = message;
			Success = false;
			Errors = errors ?? Array.Empty<Error>();
		}
		public ErrorResult(string message, params Error[] errors)
		{
			Message = message;
			Success = false;
			Errors = errors ?? Array.Empty<Error>();
		}

		public string Message { get; }
		public IReadOnlyCollection<Error> Errors { get; }
	}

	public class ErrorResult<T> : Result<T>, IErrorResult
	{
		public ErrorResult(string message) : this(message, Array.Empty<Error>())
		{
		}

		public ErrorResult(string message, params Error[] errors) : base(default)
		{
			Message = message;
			Success = false;
			Errors = errors ?? Array.Empty<Error>();
		}

		public string Message { get; set; }
		public IReadOnlyCollection<Error> Errors { get; }
	}

	public class Error
	{
		public Error(string details) : this(null, details)
		{

		}

		public Error(string code, string details)
		{
			Code = code;
			Details = details;
		}

		public string Code { get; }
		public string Details { get; }
	}

	internal interface IErrorResult
	{
		string Message { get; }
		IReadOnlyCollection<Error> Errors { get; }
	}
	public class ValidationErrorResult : ErrorResult, IErrorResult
	{
		public ValidationErrorResult(string message, params ValidationError[] errors) 
			: base(message,errors) { }
	}
	public class ValidationErrorResult<T> : Result<T>, IErrorResult
	{
		public ValidationErrorResult(T data) : base(data)
		{
		}

		public ValidationErrorResult(string message, params ValidationError[] errors) : base(default)
		{
			Message = message;
			//if (errors != null)
			//	message += "\n" + string.Join("\n", errors.Select(c=>$"{c.PropertyName}={c.Details}"));
			Success = false;
			Errors = errors ?? Array.Empty<Error>();
		}

		public string Message { get; }
		public IReadOnlyCollection<Error> Errors { get; }
	}
	public class ValidationError : Error
	{
		public ValidationError(string propertyName, string details) : base(null, details)
		{
			PropertyName = propertyName;
		}
		public override string ToString()
		{
			return $"{PropertyName}={Details}";
		}
		public string PropertyName { get; }
	}
}
