using FluentEmail.Core;
using NovaQueue.Abstractions;
using NovaQueue.Core.Services;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Core
{
	internal class DeadLetterJob<T> : IJob
	{
		private readonly string queueLink = "";
		private readonly ITransactionalQueue<T> queue;
		private readonly IEmailService emailService;
		private readonly string template = @"<h1>ATTENZIONE! La coda {{queueName}} ha aggiunto elementi nella DeadLetter.</h1>
<p>La DeadLetter al momento ha {{entriesCount}} elementi fermi nella coda, l'ultimo è stato inserito il {{lastAdded}}</p>
<p>Ti ricordo che questi elementi non verranno più processati senza il tuo intervento manuale</p>
<p><strong>Per visualizzare e gestire la DeadLetter <a href=""{{queueLink}}"">clicca qui</a></strong></p>";
		public DeadLetterJob(ITransactionalQueue<T> queue, IEmailService emailService)
		{
			this.queue = queue;
			this.emailService = emailService;
		}
		public async Task Execute(IJobExecutionContext context)
		{
			if (queue.Options.DeadLetter.IsEnabled)
			{
				var entries = queue.DeadLetterEntries();

				await emailService.SendEmailAsync<dynamic>(
					"TIE - DeadLetter Alert", 
					queue.Options.DeadLetter.AlertMailRecipients, 
					template, 
					new {
						queueName = queue.Options.Name,
						entriesCount = entries.Count(),
						lastAdded = entries.OrderByDescending(c => c.DateCreated).First().DateCreated.ToLongDateString(),
						queueLink
				});
			}
		}
	}
}
