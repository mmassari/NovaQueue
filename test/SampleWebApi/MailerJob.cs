using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;
using System.Collections.ObjectModel;

namespace SampleWebApi;
public record MailPayload(string subject, string from, string to, string body);

public class MailerJob2 : QueueJobBase<MailPayload>
{
	protected override (bool Result, ValidationError[] Errors) Validate(MailPayload payload)
	{
		var errors = new List<ValidationError>();
		if (string.IsNullOrWhiteSpace(payload.from))
			errors.Add(new ValidationError(nameof(payload.from), "Field is mandatory!"));
		if (string.IsNullOrWhiteSpace(payload.to))
			errors.Add(new ValidationError(nameof(payload.to), "Field is mandatory!"));

		if (errors.Count == 0)
			return (true, null!);

		return (false, errors.ToArray());
	}
	protected override void JobExecute(MailPayload payload)
	{
		LogEvent($"MailerJob, I'm sending an email to {payload.to} with subject {payload.subject}");

		Task.Delay(3000);
	}
}

public class MailerJob : IQueueJobAsync<MailPayload>
{
	public async Task<(bool Result, ValidationError[] Errors)> ValidateAsync(MailPayload payload) =>
		await Task.Run(() =>
		{
			var errors = new List<ValidationError>();
			if (string.IsNullOrWhiteSpace(payload.from))
				errors.Add(new ValidationError(nameof(payload.from), "Field is mandatory!"));
			if (string.IsNullOrWhiteSpace(payload.to))
				errors.Add(new ValidationError(nameof(payload.to), "Field is mandatory!"));

			if (errors.Count == 0)
				return (true, null!);

			return (false, errors.ToArray());
		});


	public async Task<Result<QueueEntryLog>> RunWorkerAsync(MailPayload payload)
	{
		var validation = await ValidateAsync(payload);
		if (!validation.Result)
			return new ValidationErrorResult<QueueEntryLog>("Validation Errors", validation.Errors);

		var logs = new QueueEntryLog();
		logs.Add($"MailerJob, I'm sending an email to {payload.to} with subject {payload.subject}");

		await Task.Delay(3000);

		return new SuccessResult<QueueEntryLog>(logs);
	}
}
