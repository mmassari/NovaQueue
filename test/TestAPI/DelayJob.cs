using FluentValidation;
using FluentValidation.Results;
using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;

namespace TestApi;
public record DelayPayload(string title, DateTime dt, int counter);

public class DelayJob : QueueJobBase<DelayPayload>
{
	protected override ValidationError[] Validate(DelayPayload payload)
	{
		ValidationResult result = new DelayPayloadValidator().Validate(payload);
		if (result.IsValid)
			return null!;
		var err = result.Errors.Select(c => new ValidationError(c.PropertyName, c.ErrorMessage));
		LogEvent("VALIDATION ERROR!! " + string.Join(" - ", err));
		return err.ToArray();
	}
	protected override void JobExecute(DelayPayload payload)
	{
		LogEvent($"RunWorkerAsync - Payload: {payload}");
		Task.Delay(3000);
		if (payload.counter % 10 == 0)
		{
			var err = "Errore durante la divisione per 10";
			throw new InvalidOperationException(err);
		}
	}
	public class DelayPayloadValidator : AbstractValidator<DelayPayload>
	{
		public DelayPayloadValidator()
		{
			RuleFor(c => c.title)
				.NotEmpty()
				.WithMessage("The title can't be empty");
			RuleFor(c => c.dt)
				.NotEmpty()
				.GreaterThan(new DateTime(2021, 1, 1))
				.WithMessage("The date must be greater than 01/01/2021");
			RuleFor(c => c.counter)
				.GreaterThan(0)
				.WithMessage("The counter must be greater than 0");
		}
	}
}

