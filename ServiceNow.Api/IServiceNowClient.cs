using Newtonsoft.Json.Linq;
using ServiceNow.Api.MetaData;
using ServiceNow.Api.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceNow.Api;
public interface IServiceNowClient : IDisposable
{
	Task<JObject> CreateAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default);
	Task<JObject> CreateAsync(string tableName, JObject jObject, string? extraQueryString = null, CancellationToken cancellationToken = default);
	Task<T> CreateAsync<T>(T @object, CancellationToken cancellationToken = default) where T : Table;
	Task<T> CreateAsync<T>(T @object, string? extraQueryString = null, CancellationToken cancellationToken = default) where T : Table;
	Task DeleteAsync(string tableName, string sysId, CancellationToken cancellationToken = default);
	Task DeleteAsync<T>(string sysId, CancellationToken cancellationToken = default) where T : Table;
	Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, string? filename = null, CancellationToken cancellationToken = default);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query = null, List<string>? fieldList = null, string? extraQueryString = null, string? customOrderByField = null, int? pageSize = null, CancellationToken cancellationToken = default);
	Task<List<T>> GetAllByQueryAsync<T>(string? query = null, CancellationToken cancellationToken = default) where T : Table;
	Task<List<Attachment>> GetAttachmentsAsync(string tableName, string tableSysId, CancellationToken cancellationToken = default);
	Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken = default) where T : Table;
	Task<JObject?> GetByIdAsync(string tableName, string sysId, CancellationToken cancellationToken = default);
	Task<T?> GetByIdAsync<T>(string sysId, CancellationToken cancellationToken = default) where T : Table;
	Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList, CancellationToken cancellationToken = default);
	Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className, CancellationToken cancellationToken = default);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query = null, List<string>? fieldList = null, string? extraQueryString = null, CancellationToken cancellationToken = default);
	Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, string? query = null, CancellationToken cancellationToken = default) where T : Table;
	Task<JObject> PatchAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default);
	Task<JObject> UpdateAsync(string tableName, JObject jObject, CancellationToken cancellationToken = default);
	Task UpdateAsync<T>(T @object, CancellationToken cancellationToken = default) where T : Table;
}