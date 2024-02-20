using KellermanSoftware.CompareNetObjects;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace Muflone.SpecificationTests
{
	/// <summary>
	///   https://github.com/luizdamim/NEventStoreExample/tree/master/NEventStoreExample.Test
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	public abstract class CommandSpecification<TCommand> where TCommand : Command
	{
		/// <summary>
		///   If the test expects an exception, set this in the constructor of your test
		/// </summary>
		protected Exception ExpectedException { get; set; } = default!;

		/// <summary>
		///   Mock this repository in the constructor of your test if the default one should not be enough
		/// </summary>
		protected InMemoryEventRepository Repository { get; set; }
		protected CommandSpecification()
		{
			//Use this or mock it from test
			Repository = new InMemoryEventRepository();
		}

		/// <summary>
		///   The list of events that represent the initial status of the aggregate root under test
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<DomainEvent> Given();

		/// <summary>
		///   The command that that triggers the events
		/// </summary>
		/// <returns></returns>
		protected abstract TCommand When();

		/// <summary>
		///   Returns the instance of the command handler
		/// </summary>
		/// <returns></returns>
		protected abstract ICommandHandlerAsync<TCommand> OnHandler();

		/// <summary>
		///   The list of events that should be compared to the ones emitted by the aggregate root
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<DomainEvent> Expect();

		private static void CompareEvents(IEnumerable<DomainEvent> expected, IEnumerable<DomainEvent> published)
		{
			if (published == null)
				published = new List<DomainEvent>();

			Assert.True(expected.Count() == published.Count(), "Different number of expected/published events.");

			var config = new ComparisonConfig();
			config.MembersToIgnore.Add("Headers");
			config.MembersToIgnore.Add("MessageId");

			var compareObjects = new CompareLogic(config);
			var eventPairs = expected.Zip(published, (e, p) => new { Expected = e, Published = p });
			foreach (var eventPair in eventPairs)
			{
				var result = compareObjects.Compare(eventPair.Expected, eventPair.Published);
				Assert.True(result.AreEqual,
					$"Events {eventPair.Expected.GetType()} and {eventPair.Published.GetType()} are different: {result.DifferencesString}");
			}
		}

		[Fact]
		public async Task SetUp()
		{
			Repository.ApplyGivenEvents(Given().ToList());
			var handler = OnHandler();
			try
			{
				await handler.HandleAsync(When());
				var expected = Expect().ToList();
				var published = Repository.Events;
				CompareEvents(expected, published);
			}
			catch (Exception exception) //Otherwise should be something expected
			{
				if (ExpectedException == null)
					Assert.True(false, $"{exception.GetType()}: {exception.Message}\n{exception.StackTrace}");
				Assert.True(exception.GetType() == ExpectedException.GetType(),
					$"Exception type {exception.GetType()} differs from expected type {ExpectedException.GetType()}");
				Assert.True(exception.Message == ExpectedException.Message,
					$"Exception message \"{exception.Message}\" differs from expected message \"{ExpectedException.GetType()}\"");
			}
		}
	}
}