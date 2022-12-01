/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using FluentAssertions;
using LiteDB;
using NovaQueue.Abstractions;
using NovaQueue.Core;
using NovaQueue.Persistence.LiteDB;
using System;
using System.Collections.Generic;
using Xunit;

namespace NovaQueueTests
{
	public class NovaQueueTest_Transactional : IDisposable
	{
		LiteDBRepository _repository;
		const string _collectionName = "transactionaltestcollection";

		NQueue<T> CreateQueue<T>() =>
			new NQueue<T>(_repository, _collectionName, new NovaQueueOptions());

		public NovaQueueTest_Transactional()
		{
			_repository = new LiteDBRepository("Filename=NovaQueueTest.db;connection=shared");
//			_repository.Clear(CollectionType.Queue | CollectionType.Completed | CollectionType.DeadLetter);
		}

		public void Dispose()
		{
			_repository.DropCollection();
			_repository.Dispose();
		}

		[Fact]
		public void Ctor_DbCollectionName()
		{
			var logs = CreateQueue<string>();

			logs.Count().Should().Be(0);
		}

		[Fact]
		public void Enqueue()
		{
			var logs = CreateQueue<string>();

			logs.Enqueue("AddTest");

			logs.Count().Should().Be(1);
		}

		[Fact]
		public void EnqueueBatch()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			logs.Count().Should().Be(3);
		}

		[Fact]
		public void Dequeue()
		{
			var logs = CreateQueue<string>();

			const string entry = "NextTest";
			logs.Enqueue(entry);

			var record = logs.Dequeue();
			record.IsCheckedOut.Should().Be(true);
			record.Payload.Should().Be(entry);
			logs.Count().Should().Be(1);

			record = logs.Dequeue();
			record.Should().BeNull();
		}

		[Fact]
		public void DequeueBatch()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(1);
			records.Count.Should().Be(1);
			records[0].Payload.Should().Be("a");
			logs.Count().Should().Be(3);

			records = logs.Dequeue(2);
			records.Count.Should().Be(2);
			records[0].Payload.Should().Be("b");
			records[1].Payload.Should().Be("c");
			logs.Count().Should().Be(3);

			records = logs.Dequeue(2);
			records.Count.Should().Be(0);
		}

		[Fact]
		public void Fifo()
		{
			var logs = CreateQueue<int>();

			const int count = 1000;

			for (int i = 0; i < count; i++)
			{
				logs.Enqueue(i);
			}

			for (int i = 0; i < count; i++)
			{
				var next = logs.Dequeue();
				next.Payload.Should().Be(i);
				logs.Commit(next);
			}

			logs.Count().Should().Be(0);
		}

		[Fact]
		public void CurrentCheckouts()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(1);
			var checkouts = logs.CurrentCheckouts();
			checkouts.Count.Should().Be(1);
			checkouts[0].IsCheckedOut.Should().BeTrue();
			checkouts[0].Payload.Should().Be(batch[0]);
		}

		[Fact]
		public void ResetOrphans()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(1);

			logs.CurrentCheckouts().Count.Should().Be(1);

			logs.ResetOrphans();

			logs.CurrentCheckouts().Count.Should().Be(0);
		}

		[Fact]
		public void Abort()
		{
			var logs = CreateQueue<string>();

			logs.Enqueue("AddTest");

			var record = logs.Dequeue();
			logs.Abort(record);

			logs.Count().Should().Be(1);
			logs.CurrentCheckouts().Count.Should().Be(0);
		}

		[Fact]
		public void AbortBatch()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(3);
			logs.Abort(records);

			logs.Count().Should().Be(3);
			logs.CurrentCheckouts().Count.Should().Be(0);
		}

		[Fact]
		public void Commit()
		{
			var logs = CreateQueue<string>();

			logs.Enqueue("AddTest");

			var record = logs.Dequeue();
			logs.Commit(record);
			logs.Count().Should().Be(0);
		}

		[Fact]
		public void CommitBatch()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(3);
			logs.Commit(records);

			logs.Count().Should().Be(0);
		}

		[Fact]
		public void Clear()
		{
			var logs = CreateQueue<string>();

			List<string> batch = new List<string>() { "a", "b", "c" };
			logs.Enqueue(batch);

			var records = logs.Dequeue(1);

			logs.Clear();
			logs.Count().Should().Be(0);
		}

		[Fact]
		public void ComplexObject()
		{
			var logs = CreateQueue<CustomRecord>();

			var record1 = new CustomRecord()
			{
				Device = new DeviceLocation { LatitudeDegrees = 120, LongitudeDegrees = 30 },
				LogValue = "test",
				SensorReading = 2.2
			};
			var record2 = new CustomRecord()
			{
				Device = new DeviceLocation { LatitudeDegrees = 121, LongitudeDegrees = 31 },
				LogValue = "test2",
				SensorReading = 2.3
			};
			var record3 = new CustomRecord()
			{
				Device = new DeviceLocation { LatitudeDegrees = 122, LongitudeDegrees = 32 },
				LogValue = "test3",
				SensorReading = 2.4
			};

			var batch = new List<CustomRecord>() { record1, record2, record3 };

			logs.Enqueue(batch);

			var records = logs.Dequeue(1);
			records.Count.Should().Be(1);
			logs.Count().Should().Be(3);
			records[0].Payload.LogValue.Should().Be(record1.LogValue);
			logs.CurrentCheckouts().Count.Should().Be(1);

			logs.Abort(records);
			logs.Count().Should().Be(3);
			logs.CurrentCheckouts().Count.Should().Be(0);

			records = logs.Dequeue(1);
			logs.Commit(records);
			logs.Count().Should().Be(2);
			logs.CurrentCheckouts().Count.Should().Be(0);

			records = logs.Dequeue(2);
			records.Count.Should().Be(2);
			logs.Count().Should().Be(2);
			logs.CurrentCheckouts().Count.Should().Be(2);

			records[0].Payload.LogValue.Should().Be(record2.LogValue);
			records[1].Payload.LogValue.Should().Be(record3.LogValue);

			logs.Commit(records);
			logs.Count().Should().Be(0);
			logs.CurrentCheckouts().Count.Should().Be(0);

			records = logs.Dequeue(2);
			records.Count.Should().Be(0);
		}
	}
}
