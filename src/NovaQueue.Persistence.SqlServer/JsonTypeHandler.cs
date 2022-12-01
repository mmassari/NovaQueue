using Dapper;
using Dapper.Json;
using System.Data;
using System.Text.Json;

namespace NovaQueue.Persistence.SqlServer
{
	public class JsonTypeHandler : SqlMapper.TypeHandler<Json<object>>
	{
		public override void SetValue(IDbDataParameter parameter, Json<object> value)
		{
			parameter.Value = JsonSerializer.Serialize(value.Value);
		}

		public override Json<object> Parse(object value)
		{
			if (value is string json)
			{
				return new Json<object>(JsonSerializer.Deserialize<object>(json));
			}

			return new Json<object>(default);
		}
	}
}
