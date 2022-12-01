/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using FluentAssertions;
using LiteDB;
using NovaQueueLib;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static System.Net.Mime.MediaTypeNames;
using NovaQueue.Abstractions;
using NovaQueue.Core;
using NovaQueue.Persistence.LiteDB;

namespace NovaQueueTests
{
	public class NovaQueueTest_Threaded: IDisposable
	{
		/// <summary>
		/// How many records created by each producer thread
		/// </summary>
		const int _recordsToProduce = 10;

		/// <summary>
		/// Monotomically increasing value shared across producers
		/// </summary>
		int _producerCounter = 0;

		/// <summary>
		/// Shared by all consumers
		/// </summary>
		HashSet<int> _consumedRecords = new HashSet<int>();

		LiteDBRepository _repository;
		const string _collectionName = "threadedtestcollection";

		NQueue<T> CreateQueue<T>() =>
			new NQueue<T>(_repository, _collectionName, new NovaQueueOptions());

		NQueue<int> _queue;
		public NovaQueueTest_Threaded()
		{
			_repository = new LiteDBRepository("Filename=NovaQueueTest.db;connection=direct");
//			_repository.Clear(CollectionType.Queue | CollectionType.Completed | CollectionType.DeadLetter);
			_queue = CreateQueue<int>();
		}

		public void Dispose()
		{
			_repository.DropCollection();
			_repository.Dispose();
			_consumedRecords.Clear();
		}

		/// <summary>
		/// Consumers keep running until false
		/// </summary>
		bool _keepRunning = true;

		bool _consumerFailed = false;

		[Fact]
		public void Single()
		{
			Action producer = delegate () { Producer(_queue); };
			Action consumer = delegate () { Consumer(_queue); };
			RunTasks(producer, consumer, producerCount: 1, consumerCount: 1);
		}

		[Fact]
		public void MultipleProducers()
		{
			Action producer = delegate () { Producer(_queue); };
			Action consumer = delegate () { Consumer(_queue); };
			RunTasks(producer, consumer, producerCount: 10, consumerCount: 1);
		}


		[Fact]
		public void MultipleConsumers()
		{
			Action producer = delegate () { Producer(_queue); };
			Action consumer = delegate () { Consumer(_queue); };
			RunTasks(producer, consumer, producerCount: 1, consumerCount: 10);
		}

		[Fact]
		public void MultipleProducersMultipleConsumers()
		{
			Action producer = delegate () { Producer(_queue); };
			Action consumer = delegate () { Consumer(_queue); };
			RunTasks(producer, consumer, producerCount: 10, consumerCount: 10);
		}

		[Fact]
		public void Duplicate()
		{
			try
			{
				Action producer = delegate () { BadProducer(_queue); };
				Action consumer = delegate () { Consumer(_queue); };
				RunTasks(producer, consumer, producerCount: 1, consumerCount: 1);
				Assert.True(false);
			}
			catch (Exception ex)
			{
				ex.Should().BeOfType<DuplicateException>();
			}		
		}

		/// <summary>
		/// Runs a multi-threaded producer/consumer test
		/// </summary>
		/// <param name="producerCount"># of producer threads to run</param>
		/// <param name="consumerCount"># of consumer threads to run</param>
		/// <param name="producer">Function to run for each producer</param>
		/// <param name="consumer">Function to run for each consumer</param>
		void RunTasks(Action producer, Action consumer, int producerCount, int consumerCount)
		{
			List<Task> producers = new List<Task>();
			for (int i = 0; i < producerCount; i++)
			{
				Task producerTask = new Task(producer);
				producers.Add(producerTask);
				producerTask.Start();
			}

			List<Task> consumers = new List<Task>();
			for (int i = 0; i < consumerCount; i++)
			{
				Task consumerTask = new Task(consumer);
				consumers.Add(consumerTask);
				consumerTask.Start();
			}

			Task.WaitAll(producers.ToArray());
			WaitForEmptyQueue(_queue);

			_keepRunning = false;
			try
			{
				Task.WaitAll(consumers.ToArray());
			}
			catch (AggregateException ex)
			{
				throw ex.InnerException;
			}

			VerifyAllConsumed(producerCount);
		}

		void Producer(NQueue<int> queue)
		{
			for (int i = 0; i < _recordsToProduce; i++)
			{
				int next = Interlocked.Increment(ref _producerCounter);

				queue.Enqueue(next);
				Debug.WriteLine(" > " + next);
			}
		}

		void BadProducer(NQueue<int> queue)
		{
			for (int i = 0; i < _recordsToProduce; i++)
			{
				int next = 1; // Should cause DuplicateException in consumer

				queue.Enqueue(next);
			}
		}

		void Consumer(NQueue<int> queue)
		{
			try
			{
				while (_keepRunning)
				{
					var entry = queue.Dequeue();
					if (entry != null)
					{
						Debug.WriteLine(" < " + entry.Payload);
						if (!_consumedRecords.Add(entry.Payload))
						{
							throw new DuplicateException(entry.Payload);
						}
						queue.Commit(entry);
						Debug.WriteLine(" ! " + entry.Payload);
					}
					else
					{
						Thread.Sleep(1);
					}
				}
			}
			catch
			{
				_consumerFailed = true;
				throw;
			}
		}

		void WaitForEmptyQueue(NQueue<int> queue)
		{
			while (!_consumerFailed)
			{
				var c = queue.Count();
				if (c == 0) break;
				Debug.WriteLine($"@@ Remain : {c}");
				Thread.Sleep(100);
			}
		}

		void VerifyAllConsumed(int producerThreadCount)
		{
			int expected = producerThreadCount * _recordsToProduce;
			expected.Should().Be(_consumedRecords.Count);
		}
	}
}
