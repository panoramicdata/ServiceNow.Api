using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Tests for querying a table or view that does not carry the paging field, which is sys_created_on
/// unless configured otherwise. Custom ServiceNow views frequently have no date/time field at all.
///
/// These run entirely against a stubbed message handler. They need no credentials and no network.
///
/// The bug they were written for: the missing-field check used to run only when a page came back
/// FULL, so it fired only where there was reason to believe more data followed. Moving paging
/// termination onto the empty-page signal moved that check onto every page, and a three-row read of
/// a view with no date/time field, needing no paging whatsoever, began failing outright.
/// </summary>
public class MissingPagingFieldTests
{
	private const string TableName = "u_view_without_created_on";
	private const int PageSize = 1_000;

	/// <summary>
	/// The regression case: a result that fits inside one page needs no paging, so the absent field
	/// is irrelevant and the rows must come back.
	/// </summary>
	[Fact]
	public async Task ShortPageWithNoPagingField_ReturnsTheRows()
	{
		using var handler = new StubServiceNowHandler(totalCount: 3, [MakeRows(0, 3), []]);
		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(3, "a result that fits in one page needs no paging field");
		handler.RequestCount.Should().Be(1, "the complete result arrived in the first request");
	}

	/// <summary>
	/// The caller asks for specific fields, as a report macro does. The paging field and sys_id are
	/// added to the request and trimmed from the result, so their absence must not surface.
	/// </summary>
	[Fact]
	public async Task ShortPageWithNoPagingField_ReturnsTheRequestedFields()
	{
		var fieldList = new List<string> { "cmdbci_name", "os_u_end_of_support" };

		using var handler = new StubServiceNowHandler(totalCount: 3, [MakeRows(0, 3), []]);
		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, fieldList: fieldList, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(3);
		result[0].Properties().Select(property => property.Name)
			.Should().BeEquivalentTo(fieldList, "sys_id and the paging field are requested for paging, then trimmed");
	}

	/// <summary>
	/// A genuine shortfall must still raise. Returning 3 of 100 records silently is worse than
	/// failing, because the report renders as though those were all the records there are.
	/// </summary>
	[Fact]
	public async Task ShortPageWithNoPagingField_ShortOfTheReportedTotal_Throws()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100, [MakeRows(0, 3), []]);
		using var client = new ServiceNowClient(handler);

		var act = async () => await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		(await act.Should().ThrowAsync<ServiceNowApiException>().ConfigureAwait(true))
			.WithMessage("*does not have*sys_created_on*", "the message must name the paging problem")
			.WithMessage("*PagingFieldName*", "and the option that resolves it");
	}

	/// <summary>
	/// A FULL page means more records may follow, and without the field the window cannot advance to
	/// reach them. That is the case the check exists for, and it must keep raising.
	/// </summary>
	[Fact]
	public async Task FullPageWithNoPagingField_Throws()
	{
		using var handler = new StubServiceNowHandler(totalCount: 5_000, [MakeRows(0, PageSize), []]);
		using var client = new ServiceNowClient(handler);

		var act = async () => await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		(await act.Should().ThrowAsync<ServiceNowApiException>().ConfigureAwait(true))
			.WithMessage("*does not have*sys_created_on*", "a full page cannot be assumed to be the whole result");
	}

	/// <summary>
	/// The reported total is not filtered by row-level access, so a few unreadable records leave a
	/// legitimate shortfall. Inside the configured tolerance that counts as complete.
	/// </summary>
	[Fact]
	public async Task ShortPageWithNoPagingField_WithinTolerance_ReturnsWhatWasRead()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100, [MakeRows(0, 95), []]);
		using var client = new ServiceNowClient(handler, new Options { ValidateCountItemsReturnedTolerance = 10 });

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(95, "a shortfall inside the tolerance is not a reason to demand a paging field");
	}

	/// <summary>
	/// Switching the count check off opts out of completeness entirely, so the missing field cannot
	/// raise either.
	/// </summary>
	[Fact]
	public async Task ShortPageWithNoPagingField_WithValidationDisabled_ReturnsWhatWasRead()
	{
		using var handler = new StubServiceNowHandler(totalCount: 100, [MakeRows(0, 3), []]);
		using var client = new ServiceNowClient(handler, new Options { ValidateCountItemsReturned = false });

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(3);
	}

	/// <summary>
	/// Rows as a view with no date/time column returns them: a sys_id, business fields, and no
	/// sys_created_on anywhere.
	/// </summary>
	private static List<JObject> MakeRows(int startIndex, int count)
	{
		var rows = new List<JObject>(count);
		for (var i = 0; i < count; i++)
		{
			var index = startIndex + i;
			rows.Add(new JObject
			{
				["sys_id"] = $"sys{index:D8}",
				["cmdbci_name"] = $"server{index:D4}",
				["os_u_end_of_support"] = "2027-01-01"
			});
		}

		return rows;
	}
}
