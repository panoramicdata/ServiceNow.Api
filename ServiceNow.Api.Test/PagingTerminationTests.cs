using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using System.Globalization;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Regression tests for how GetAllByQueryAsync decides that it has reached the end of a
/// result set, and for how the retrieved count is validated against the reported total.
///
/// These run entirely against a stubbed message handler. They need no credentials and no
/// network, so they exercise the paging contract on every build.
///
/// The bug they were written for: ServiceNow applies read ACLs AFTER sysparm_limit, so a
/// request for 1,000 rows can legitimately return fewer while a great deal of data still
/// follows. Paging used to stop on any short page. On a live customer query a 997-row page
/// at position 49 of 88 ended the walk at 48,724 of roughly 86,550 rows, losing 44% of the
/// result with no error raised.
/// </summary>
public class PagingTerminationTests
{
	private const string TableName = "cmdb_ci";
	private static readonly DateTime BaseTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// A short page must NOT be treated as the end of the data. Only an empty page ends paging.
	/// </summary>
	[Fact]
	public async Task ShortPageMidStream_KeepsPaging()
	{
		// 1,000 then a short 997 then 500, and only then nothing left.
		// The 997 is the ACL-shortened page that used to end the walk early.
		using var handler = new StubServiceNowHandler(totalCount: 2_497,
		[
			MakePage(0, 1_000),
			MakePage(1_000, 997),
			MakePage(1_997, 500),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(2_497, "a short page means ACL filtering, not end of data");
		handler.RequestCount.Should().Be(4, "paging should continue past the short page until an empty one");
	}

	/// <summary>
	/// The historic behaviour: stopping at the first short page. Guards against a regression
	/// back to it by asserting the count that bug produced is NOT what we get.
	/// </summary>
	[Fact]
	public async Task ShortPageMidStream_DoesNotStopAtTheShortPage()
	{
		using var handler = new StubServiceNowHandler(totalCount: 2_497,
		[
			MakePage(0, 1_000),
			MakePage(1_000, 997),
			MakePage(1_997, 500),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().NotHaveCount(1_997, "1,997 is the truncated count produced by stopping at the short page");
	}

	/// <summary>
	/// The reported total is a snapshot taken on the first page. A long walk over a live table
	/// legitimately gains records, so retrieving MORE than expected must not be an error.
	/// </summary>
	[Fact]
	public async Task RetrievingMoreThanTheReportedTotal_DoesNotThrow()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100,
		[
			MakePage(0, 105),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(105, "records created during the walk are not a data-loss signal");
	}

	/// <summary>
	/// A genuine shortfall must still be reported. This is the safety net that caught the
	/// original truncation, and it has to keep working.
	/// </summary>
	[Fact]
	public async Task RetrievingFewerThanTheReportedTotal_Throws()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100,
		[
			MakePage(0, 50),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var act = async () => await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		(await act.Should().ThrowAsync<Exception>().ConfigureAwait(true))
			.WithMessage("*100*50*", "the message should name both the expected and the retrieved count");
	}

	/// <summary>
	/// Validation is opt-out, and disabling it must suppress the shortfall error.
	/// </summary>
	[Fact]
	public async Task RetrievingFewerThanTheReportedTotal_WithValidationDisabled_DoesNotThrow()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100,
		[
			MakePage(0, 50),
			[]
		]);

		using var client = new ServiceNowClient(handler, new Options { ValidateCountItemsReturned = false });

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(50);
	}

	/// <summary>
	/// The '&gt;=' window always re-reads the boundary record. A trailing page containing only
	/// that record must end paging cleanly rather than raising the stall error.
	/// </summary>
	[Fact]
	public async Task TrailingPageOfOnlyTheBoundaryRecord_TerminatesCleanly()
	{
		var firstPage = MakePage(0, 1_000);
		// The window re-reads the last record of the previous page: same sys_id, same timestamp.
		var boundaryOnly = new List<JObject> { firstPage[^1] };

		using var handler = new StubServiceNowHandler(totalCount: 1_000, [firstPage, boundaryOnly]);
		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(1_000, "the re-read boundary record is de-duplicated, not counted twice");
	}

	/// <summary>
	/// If a FULL page shares a single timestamp the window cannot advance, and continuing would
	/// loop forever. That must still be reported as needing a larger page size.
	/// </summary>
	[Fact]
	public async Task FullPageThatCannotAdvanceTheWindow_Throws()
	{
		// Every row shares one timestamp, so the '>=' window can never move past it.
		var stuck = MakePage(0, 1_000, sameTimestamp: true);
		var stuckAgain = MakePage(1_000, 1_000, sameTimestamp: true);

		using var handler = new StubServiceNowHandler(totalCount: 5_000, [stuck, stuckAgain]);
		using var client = new ServiceNowClient(handler);

		var act = async () => await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		(await act.Should().ThrowAsync<ServiceNowApiException>().ConfigureAwait(true))
			.WithMessage("*Paging window has not increased*");
	}

	private static List<JObject> MakePage(int startIndex, int count, bool sameTimestamp = false)
	{
		var page = new List<JObject>(count);
		for (var i = 0; i < count; i++)
		{
			var index = startIndex + i;
			var created = sameTimestamp ? BaseTime : BaseTime.AddSeconds(index);
			page.Add(new JObject
			{
				["sys_id"] = $"sys{index:D8}",
				["sys_created_on"] = created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
			});
		}

		return page;
	}
}
