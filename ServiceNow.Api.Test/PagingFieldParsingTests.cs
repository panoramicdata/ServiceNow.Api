using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using ServiceNow.Api.Exceptions;
using System.Globalization;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Regression tests for how the ordering field is read out of a returned row when paging.
///
/// These run entirely against a stubbed message handler, so they need no credentials and no network.
///
/// The bug they were written for (issue #74, reported as #25): paging read the ordering field with
/// ToString() and concatenated "Z" onto it. That breaks whenever sysparm_display_value is set:
/// with "all" every field is returned as a { display_value, value } object, so ToString() yields JSON
/// and the parse throws, taking the whole query with it.
/// </summary>
public class PagingFieldParsingTests
{
	private const string TableName = "cmdb_ci";
	private const int PageSize = 1000;
	private static readonly DateTime _baseTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// The reported failure: with sysparm_display_value=all the ordering field is an object, not a scalar.
	/// </summary>
	[Fact]
	public async Task ObjectShapedPagingField_PagesInsteadOfThrowing()
	{
		using var handler = new StubServiceNowHandler(totalCount: 1_500,
		[
			MakeObjectShapedPage(0, PageSize),
			MakeObjectShapedPage(PageSize, 500),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(1_500, "an object-shaped ordering field must not break paging");
	}

	/// <summary>
	/// With both representations present the raw value is the one to page on, since it carries the
	/// underlying UTC timestamp rather than a timezone-and-format-dependent rendering of it.
	/// </summary>
	[Fact]
	public async Task ObjectShapedPagingField_PagesOnTheRawValueNotTheDisplayValue()
	{
		// value says 10:00 UTC; display_value says something entirely different.
		var fullPage = MakePageSharingOneTimestamp(new JObject
		{
			["display_value"] = "31/12/2030 23:59:59",
			["value"] = "2026-01-01 10:00:00"
		});

		using var handler = new StubServiceNowHandler(totalCount: PageSize, [fullPage, []]);
		using var client = new ServiceNowClient(handler);

		_ = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		// The second request carries the paging window, built from whichever representation was used.
		handler.RequestUris.Should().HaveCountGreaterThan(1);
		handler.RequestUris[1].Should().Contain("2026-01-01 10:00:00", "the raw value is the correct boundary");
		handler.RequestUris[1].Should().NotContain("2030", "the display value must not be used as the boundary");
	}

	/// <summary>
	/// A value carrying its own offset used to be corrupted by concatenating "Z" onto it. It should now be
	/// converted to UTC properly.
	/// </summary>
	[Fact]
	public async Task PagingFieldWithAnExplicitOffset_IsConvertedToUtc()
	{
		// 05:00 at +05:00 is midnight UTC.
		var fullPage = MakePageSharingOneTimestamp("2026-01-02T05:00:00+05:00");

		using var handler = new StubServiceNowHandler(totalCount: PageSize, [fullPage, []]);
		using var client = new ServiceNowClient(handler);

		_ = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		handler.RequestUris.Should().HaveCountGreaterThan(1);
		handler.RequestUris[1].Should().Contain("2026-01-02 00:00:00", "the offset should be applied, not ignored");
	}

	/// <summary>
	/// An ordering field that is not a date at all should say so clearly, naming the field and the value,
	/// rather than surfacing a bare FormatException from inside a LINQ Max().
	/// </summary>
	[Fact]
	public async Task UnparseablePagingField_ThrowsNamingTheFieldAndValue()
	{
		var fullPage = MakePageSharingOneTimestamp("not a date at all");

		using var handler = new StubServiceNowHandler(totalCount: PageSize, [fullPage, []]);
		using var client = new ServiceNowClient(handler);

		var act = async () => await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		(await act.Should().ThrowAsync<ServiceNowApiException>().ConfigureAwait(true))
			.WithMessage("*sys_created_on*not a date at all*");
	}

	/// <summary>
	/// A plain UTC string, which is what comes back with no sysparm_display_value, must keep working exactly
	/// as before. This is the overwhelmingly common case.
	/// </summary>
	[Fact]
	public async Task PlainUtcPagingField_StillPagesAsBefore()
	{
		using var handler = new StubServiceNowHandler(totalCount: 1_500,
		[
			MakePlainPage(0, PageSize),
			MakePlainPage(PageSize, 500),
			[]
		]);

		using var client = new ServiceNowClient(handler);

		var result = await client
			.GetAllByQueryAsync(TableName, cancellationToken: TestContext.Current.CancellationToken)
			.ConfigureAwait(true);

		result.Should().HaveCount(1_500);
	}

	/// <summary>
	/// A full page of distinct records that all share one ordering-field value, so that the paging window the
	/// client derives from the page is deterministic and can be asserted on. The sys_ids must differ or the
	/// client's de-duplication would collapse the page to a single row.
	/// </summary>
	private static List<JObject> MakePageSharingOneTimestamp(JToken sharedCreatedOn)
		=> [.. Enumerable.Range(0, PageSize).Select(i => new JObject
		{
			["sys_id"] = $"sys{i:D8}",
			["sys_created_on"] = sharedCreatedOn.DeepClone()
		})];

	private static List<JObject> MakePlainPage(int startIndex, int count)
		=> [.. Enumerable.Range(0, count).Select(i => new JObject
		{
			["sys_id"] = $"sys{startIndex + i:D8}",
			["sys_created_on"] = _baseTime.AddSeconds(startIndex + i).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
		})];

	/// <summary>
	/// The shape returned when sysparm_display_value=all is requested.
	/// </summary>
	private static List<JObject> MakeObjectShapedPage(int startIndex, int count)
		=> [.. Enumerable.Range(0, count).Select(i =>
		{
			var created = _baseTime.AddSeconds(startIndex + i);
			return new JObject
			{
				["sys_id"] = $"sys{startIndex + i:D8}",
				["sys_created_on"] = new JObject
				{
					["display_value"] = created.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
					["value"] = created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
				}
			};
		})];
}
