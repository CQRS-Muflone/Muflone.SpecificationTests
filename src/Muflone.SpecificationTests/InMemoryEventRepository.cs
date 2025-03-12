using Muflone.Core;
using Muflone.Messages.Events;
using Muflone.Persistence;

namespace Muflone.SpecificationTests
{
	public class InMemoryEventRepository : IRepository
	{
		private IEnumerable<DomainEvent> givenEvents = Enumerable.Empty<DomainEvent>();
		public IEnumerable<DomainEvent> Events { get; private set; } = Enumerable.Empty<DomainEvent>();

		private static TAggregate? ConstructAggregate<TAggregate>()
		{
			return (TAggregate?)Activator.CreateInstance(typeof(TAggregate), true);
		}

		public void Dispose()
		{
			// no op
		}

		public virtual void ApplyGivenEvents(IList<DomainEvent> events)
		{
			givenEvents = events;
		}

		public virtual async Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, CancellationToken cancellationToken) where TAggregate : class, IAggregate
		{
			return await GetByIdAsync<TAggregate>(id, 0, cancellationToken);
		}

		public Task<TAggregate?> GetByIdAsync<TAggregate>(IDomainId id, long version, CancellationToken cancellationToken) where TAggregate : class, IAggregate
		{
			var aggregate = ConstructAggregate<TAggregate>();
			if (aggregate is not null)
				givenEvents.ForEach(aggregate.ApplyEvent);
			return Task.FromResult(aggregate);
		}

		public virtual async Task SaveAsync(IAggregate aggregate, Guid commitId, CancellationToken cancellationToken = default)
		{
			await SaveAsync(aggregate, commitId, null);
		}

		public virtual Task SaveAsync(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders, CancellationToken cancellationToken = default)
		{
			Events = aggregate.GetUncommittedEvents().Cast<DomainEvent>();
			return Task.CompletedTask;
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
