using FluentEmail.Core.Models;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NovaQueue.Abstractions.Models;
using NovaQueue.Abstractions;

namespace NovaQueue.Communication.Email
{
	public class FluentEmailService : IEmailService
	{
		private readonly IFluentEmailFactory _fluentEmailFactory;
		private readonly ILogger<FluentEmailService> _logger;
		public FluentEmailService(ILogger<FluentEmailService> logger, IFluentEmailFactory fluentEmailFactory)
		{
			_logger = logger;
			_fluentEmailFactory = fluentEmailFactory;
		}

		public async Task<Result<string>> SendEmailAsync<TModel>(string subject, List<string> recipients, string liquidTemplate, TModel model)
		{
			var sendResponse = await _fluentEmailFactory
								 .Create()
								 .To(string.Join(";",recipients))
								 .Subject(subject)
								 .UsingTemplateFromFile(liquidTemplate, model)
								 .SendAsync();
			if(sendResponse.Successful)
			{
				return new SuccessResult<string>(sendResponse.MessageId);
			}

			return new ErrorResult<string>(
				"Mail not sended", 
				sendResponse.ErrorMessages.Select(c => 
					new Error(c)).ToArray()) 
				{ 
					Data = sendResponse.MessageId 
				};
		}
	}
}
