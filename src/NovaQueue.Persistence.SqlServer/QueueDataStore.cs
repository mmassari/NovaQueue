using Dapper;
using Dommel;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static Slapper.AutoMapper;

namespace NovaQueue.Persistence.SqlServer
{
	internal class QueueDataStore
	{
		private readonly string connectionString;
		private readonly string tableName;
		public IDbConnection DBConnection => new SqlConnection(connectionString);
		public QueueDataStore(string connectionString)
		{
			this.connectionString = connectionString;
		}
		/// <summary>
		/// Get del pagamento Klarna da Database o dalla cache
		/// </summary>
		/// <param name="id">HPP Session ID</param>
		/// <returns></returns>
		public async Task<QueueEntry<object>> GetAsync(dynamic id)
		{
			using var conn = DBConnection;
			var model = await conn.QueryFirstAsync<QueueEntry<T>>($"SELECT * FROM {tableName} WHERE Id=@id", new { id = id });
			return model;

		}
		public async Task<IEnumerable<QueueEntry<object>>> GetAllAsync()
		{
			using var conn = DBConnection;
			return await conn.GetAllAsync<QueueEntry<object>>();
		}
		public async Task InsertAsync(QueueEntry<object> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task InsertAsync(IEnumerable<QueueEntry<object>> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task UpdateAsync(QueueEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.UpdateAsync(queue);			
		}
		public async Task DeleteAsync(QueueEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.DeleteAsync(queue);
		}

		public  Task<IEnumerable<QueueEntry<object>>> GetUncheckedOut(int maxEntries)
		{
			throw new NotImplementedException();
		}
		public Task<IEnumerable<QueueEntry<object>>> GetCheckedOut()
		{
			throw new NotImplementedException();
		}
	}
	internal class CompletedDataStore
	{
		private readonly string connectionString;
		private readonly string tableName;
		public IDbConnection DBConnection => new SqlConnection(connectionString);
		public CompletedDataStore(string connectionString)
		{
			this.connectionString = connectionString;
		}
		/// <summary>
		/// Get del pagamento Klarna da Database o dalla cache
		/// </summary>
		/// <param name="id">HPP Session ID</param>
		/// <returns></returns>
		public async Task<CompletedEntry<object>> GetAsync(dynamic id)
		{
			using var conn = DBConnection;
			var model = await conn.QueryFirstAsync<CompletedEntry<object>>($"SELECT * FROM {tableName} WHERE Id=@id", new { id = id });
			return model;

		}
		public async Task<IEnumerable<CompletedEntry<object>>> GetAllAsync()
		{
			using var conn = DBConnection;
			return await conn.GetAllAsync<CompletedEntry<object>>();
		}
		public async Task InsertAsync(CompletedEntry<object> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task InsertAsync(IEnumerable<CompletedEntry<object>> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task UpdateAsync(CompletedEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.UpdateAsync(queue);
		}
		public async Task DeleteAsync(CompletedEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.DeleteAsync(queue);
		}
	}
	internal class DeadLetterDataStore
	{
		private readonly string connectionString;
		private readonly string tableName;
		public IDbConnection DBConnection => new SqlConnection(connectionString);
		public DeadLetterDataStore(string connectionString)
		{
			this.connectionString = connectionString;
		}
		/// <summary>
		/// Get del pagamento Klarna da Database o dalla cache
		/// </summary>
		/// <param name="id">HPP Session ID</param>
		/// <returns></returns>
		public async Task<DeadLetterEntry<object>> GetAsync(dynamic id)
		{
			using var conn = DBConnection;
			var model = await conn.QueryFirstAsync<DeadLetterEntry<object>>($"SELECT * FROM {tableName} WHERE Id=@id", new { id = id });
			return model;

		}
		public async Task<IEnumerable<DeadLetterEntry<object>>> GetAllAsync()
		{
			using var conn = DBConnection;
			return await conn.GetAllAsync<DeadLetterEntry<object>>();
		}
		public async Task InsertAsync(DeadLetterEntry<object> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task InsertAsync(IEnumerable<DeadLetterEntry<object>> queue)
		{
			using var conn = DBConnection;
			var id = await conn.InsertAsync(queue);
		}
		public async Task UpdateAsync(DeadLetterEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.UpdateAsync(queue);
		}
		public async Task DeleteAsync(DeadLetterEntry<object> queue)
		{
			using var conn = DBConnection;
			await conn.DeleteAsync(queue);
		}
	}

}
