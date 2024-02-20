using Muflone.Messages.Events;
using Muflone.Persistence;

namespace Muflone.SpecificationTests
{ /// <summary>
  /// https://github.com/luizdamim/NEventStoreExample/tree/master/NEventStoreExample.Test
  /// </summary>
	public class InMemoryEventRepository : IRepository
	{
		private IEnumerable<DomainEvent> givenEvents;
		public IEnumerable<DomainEvent> Events { get; private set; }

		private static TAggregate ConstructAggregate<TAggregate>()
		{
			return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
		}

		public void Dispose()
		{
			// no op
		}

		public virtual void ApplyGivenEvents(IList<DomainEvent> events)
		{
			givenEvents = events;
		}

		public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : class, IAggregate
		{
			return await GetByIdAsync<TAggregate>(id, 0);
		}

		public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate
		{
			var aggregate = ConstructAggregate<TAggregate>();
			givenEvents.ForEach(aggregate.ApplyEvent);
			return Task.FromResult(aggregate);
		}

		public Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
		{
			Events = aggregate.GetUncommittedEvents().Cast<DomainEvent>();
			return Task.CompletedTask;
		}

		public Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
		{
			Events = aggregate.GetUncommittedEvents().Cast<DomainEvent>();
			return Task.CompletedTask;
		}

		public Task SaveAsync(IAggregate aggregate, Guid commitId)
		{
			return SaveAsync(aggregate, commitId, null!);
		}
	}

	public static class Helpers
	{
		//Stolen from Castle.Core.Internal
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null)
				return;
			foreach (T obj in items)
				action(obj);
		}
	}
}
