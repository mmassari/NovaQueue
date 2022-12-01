using FluentEmail.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Core.Services
{
	public interface IEmailService
	{
		Task<SendResponse> SendEmailAsync<TModel>(string subject, List<string> recipients, string liquidTemplate, TModel model);
	}
}