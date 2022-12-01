/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using LiteDB;
using NovaQueue.Core;
using Xunit;
using System;
using System.Collections.Generic;
using FluentAssertions;
using NovaQueue.Abstractions;
using NovaQueue.Persistence.LiteDB;

namespace NovaQueueTests
{
	/// <summary>
	/// Many tests may appear similar to the Transactional tests but there are subtle differences
	/// </summary>
	public class NovaQueueTest_NotTransactional : IDisposable
	{
		LiteDBRepository _repository;
		const string _collectionName = "nottransactionaltestcollection";

		NQueueSimple<T> CreateQueue<T>()
		{
			var queue = new NQueueSimple<T>(_repository, _collectionName);
			return queue;
		}

		public NovaQueueTest_NotTransactional()
		{
			_repository = new LiteDBRepository("Filename=NovaQueueTest.db;connection=direct");
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
		public void Ctor_Collection()
		{
			var logs = CreateQueue<string>();

			logs.Count().Should().Be(0);
		}

		[Fact]
		public void Dequeue()
		{
			var logs = CreateQueue<string>();

			const string entry = "NextTest";
			logs.Enqueue(entry);

			var record = logs.Dequeue();
			record.IsCheckedOut.Should().BeFalse();
			record.Payload.Should().Be(entry);
			logs.Count().Should().Be(0);

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
			logs.Count().Should().Be(2);

			records = logs.Dequeue(2);
			records.Count.Should().Be(2);
			records[0].Payload.Should().Be("b");
			records[1].Payload.Should().Be("c");
			logs.Count().Should().Be(0);

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
				int next = logs.Dequeue().Payload;
				next.Should().Be(i);
			}

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
				Device = new DeviceLocation()
				{
					LatitudeDegrees = 120,
					LongitudeDegrees = 30
				},
				LogValue = "test",
				SensorReading = 2.2
			};
			var record2 = new CustomRecord()
			{
				Device = new DeviceLocation()
				{
					LatitudeDegrees = 121,
					LongitudeDegrees = 31
				},
				LogValue = "test2",
				SensorReading = 2.3
			};
			var record3 = new CustomRecord()
			{
				Device = new DeviceLocation()
				{
					LatitudeDegrees = 122,
					LongitudeDegrees = 32
				},
				LogValue = "test3",
				SensorReading = 2.4
			};

			var batch = new List<CustomRecord>() { record1, record2, record3 };

			logs.Enqueue(batch);

			var records = logs.Dequeue(1);
			records.Count.Should().Be(1);
			logs.Count().Should().Be(2);
			records[0].Payload.LogValue.Should().Be(record1.LogValue);

			records = logs.Dequeue(2);
			records.Count.Should().Be(2);
			logs.Count().Should().Be(0);

			records[0].Payload.LogValue.Should().Be(record2.LogValue);
			records[1].Payload.LogValue.Should().Be(record3.LogValue);

			records = logs.Dequeue(2);
			records.Count.Should().Be(0);
		}
	}
}
