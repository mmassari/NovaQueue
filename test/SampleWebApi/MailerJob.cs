using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SampleWebApi;
public class MailPayload
{
	public string subject { get; set; }
	public string from { get; set; }
	public string to { get; set; }
	public string body { get; set; }
}

public class MailerJob2 : QueueJobBase<MailPayload>
{
	protected override IEnumerable<ValidationError> Validate(MailPayload payload)
	{
		var errors = new List<ValidationError>();
		if (string.IsNullOrWhiteSpace(payload.from))
			errors.Add(new ValidationError(nameof(payload.from), "Field is mandatory!"));
		if (string.IsNullOrWhiteSpace(payload.to))
			errors.Add(new ValidationError(nameof(payload.to), "Field is mandatory!"));

		if (errors.Count == 0)
			return null;

		return errors.ToArray();
	}
	protected override void JobExecute(MailPayload payload)
	{
		LogEvent($"MailerJob, I'm sending an email to {payload.to} with subject {payload.subject}");

		Task.Delay(3000);
	}
}

public class MailerJob : IQueueJobAsync<MailPayload>
{
	public event JobLogEventHandler<MailPayload> LogMessageReceived;

	public async Task<ValidationError[]> ValidateAsync(MailPayload payload) =>
		await Task.Run(() =>
		{
			var errors = new List<ValidationError>();
			if (string.IsNullOrWhiteSpace(payload.from))
				errors.Add(new ValidationError(nameof(payload.from), "Field is mandatory!"));
			if (string.IsNullOrWhiteSpace(payload.to))
				errors.Add(new ValidationError(nameof(payload.to), "Field is mandatory!"));

			if (errors.Count == 0)
				return null;

			return errors.ToArray();
		});

	public async Task<Result> RunWorkerAsync(QueueEntry<MailPayload> entry)
	{
		var errors = await ValidateAsync(entry.Payload);
		if (errors != null)
			return new ValidationErrorResult("Validation Errors", errors);

		LogMessageReceived?.Invoke(entry,$"MailerJob, I'm sending an email to {entry.Payload.to} with subject {entry.Payload.subject}");

		await Task.Delay(3000);

		return new SuccessResult();
	}
}
