using NovaQueue.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IEmailService
	{
		Task<Result<string>> SendEmailAsync<TModel>(string subject, List<string> recipients, string liquidTemplate, TModel model);
	}
}