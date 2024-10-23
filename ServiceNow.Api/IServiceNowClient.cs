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
	Task<JObject> CreateAsync(string tableName, JObject jObject);
	Task<JObject> CreateAsync(string tableName, JObject jObject, CancellationToken cancellationToken);
	Task<JObject> CreateAsync(string tableName, JObject jObject, string extraQueryString);
	Task<JObject> CreateAsync(string tableName, JObject jObject, string extraQueryString, CancellationToken cancellationToken);

	Task<T> CreateAsync<T>(T @object) where T : Table;
	Task<T> CreateAsync<T>(T @object, CancellationToken cancellationToken) where T : Table;
	Task<T> CreateAsync<T>(T @object, string extraQueryString) where T : Table;
	Task<T> CreateAsync<T>(T @object, string? extraQueryString, CancellationToken cancellationToken) where T : Table;

	Task DeleteAsync(string tableName, string sysId);
	Task DeleteAsync(string tableName, string sysId, CancellationToken cancellationToken);

	Task DeleteAsync<T>(string sysId) where T : Table;
	Task DeleteAsync<T>(string sysId, CancellationToken cancellationToken = default) where T : Table;

	Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath);
	Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, CancellationToken cancellationToken);
	Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, string filename);
	Task<string> DownloadAttachmentAsync(Attachment attachment, string outputPath, string? filename, CancellationToken cancellationToken = default);

	Task<List<JObject>> GetAllByQueryAsync(string tableName);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, CancellationToken cancellationToken);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string query);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string query, CancellationToken cancellationToken);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string> fieldList);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string> fieldList, CancellationToken cancellationToken);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string extraQueryString, CancellationToken cancellationToken);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString, string? customOrderByField);
	Task<List<JObject>> GetAllByQueryAsync(string tableName, string? query, List<string>? fieldList, string? extraQueryString, string? customOrderByField, int? pageSize, CancellationToken cancellationToken);

	Task<List<T>> GetAllByQueryAsync<T>() where T : Table;
	Task<List<T>> GetAllByQueryAsync<T>(CancellationToken cancellationToken) where T : Table;
	Task<List<T>> GetAllByQueryAsync<T>(string query) where T : Table;
	Task<List<T>> GetAllByQueryAsync<T>(string? query, CancellationToken cancellationToken) where T : Table;

	Task<List<Attachment>> GetAttachmentsAsync(string tableName, string tableSysId);
	Task<List<Attachment>> GetAttachmentsAsync(string tableName, string tableSysId, CancellationToken cancellationToken);

	Task<List<Attachment>> GetAttachmentsAsync<T>(T table) where T : Table;
	Task<List<Attachment>> GetAttachmentsAsync<T>(T table, CancellationToken cancellationToken) where T : Table;

	Task<JObject?> GetByIdAsync(string tableName, string sysId);
	Task<JObject?> GetByIdAsync(string tableName, string sysId, CancellationToken cancellationToken);

	Task<T?> GetByIdAsync<T>(string sysId) where T : Table;
	Task<T?> GetByIdAsync<T>(string sysId, CancellationToken cancellationToken) where T : Table;

	Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList);
	Task<JObject?> GetLinkedEntityAsync(string link, List<string> fieldList, CancellationToken cancellationToken);

	Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className);
	Task<RestResponse<MetaDataResult>> GetMetaForClassAsync(string className, CancellationToken cancellationToken);

	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, CancellationToken cancellationToken);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string query);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string query, CancellationToken cancellationToken);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string> fieldList);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string> fieldList, CancellationToken cancellationToken);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string>? fieldList, string extraQueryString);
	Task<Page<JObject>> GetPageByQueryAsync(int skip, int take, string tableName, string? query, List<string>? fieldList, string? extraQueryString, CancellationToken cancellationToken);

	Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take) where T : Table;
	Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, CancellationToken cancellationToken) where T : Table;
	Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, string query) where T : Table;
	Task<Page<T>> GetPageByQueryAsync<T>(int skip, int take, string? query, CancellationToken cancellationToken) where T : Table;

	Task<JObject> PatchAsync(string tableName, JObject jObject);
	Task<JObject> PatchAsync(string tableName, JObject jObject, CancellationToken cancellationToken);

	Task<JObject> UpdateAsync(string tableName, JObject jObject);
	Task<JObject> UpdateAsync(string tableName, JObject jObject, CancellationToken cancellationToken);

	Task UpdateAsync<T>(T @object) where T : Table;
	Task UpdateAsync<T>(T @object, CancellationToken cancellationToken) where T : Table;
}