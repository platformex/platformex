using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Platformex.Web
{
    public class PlatformexMiddleware
    {
        private readonly Regex _commandPath;
        private readonly Regex _queryPath;
        private readonly RequestDelegate _next;
        private readonly IPlatform _platform;
        private readonly ILogger _log;

        public PlatformexMiddleware(
            RequestDelegate next,
            PlatformexWebApiOptions options,
            IPlatform platform,
            ILogger<PlatformexMiddleware> log)
        {
            _next = next;
            _platform = platform;
            _log = log;

            var basePath = "/*" + options.BasePath.Trim('/');

            _commandPath = new Regex(basePath + "/(?<context>[a-z]+)/(?<name>[a-z]+)/{0,1}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _queryPath = new Regex(basePath + "/queries/(?<name>[a-z0-9]+)/{0,1}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (context.Request.Method == HttpMethods.Put && path.HasValue)
            {
                var match = _commandPath.Match(path.Value ?? string.Empty);
                if (match.Success)
                {
                    var metadata = GetMetadata(context);
                    await PublishCommandOrServiceAsync(match.Groups["context"].Value, match.Groups["name"].Value, context, metadata);
                    return;
                }
            }

            if (context.Request.Method == HttpMethods.Post && path.HasValue)
            {
                var match = _queryPath.Match(path.Value ?? string.Empty);
                if (match.Success)
                {
                    var metdatda = GetMetadata(context);
                    await ExecuteQueryAsync(match.Groups["name"].Value, context, metdatda);
                    return;
                }
            }

            await _next(context);
        }

        private Dictionary<string, string> GetMetadata(HttpContext context)
        {
            var usr = context.User;
            if (usr.Identity == null) return new Dictionary<string, string>();

            var result = new Dictionary<string, string>
            {
                { MetadataKeys.UserName, usr.Claims.FirstOrDefault(i => i.Type == JwtClaimTypes.Id)?.Value},
                { MetadataKeys.UserId, usr.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value}
            };

            return result;
        }

        private async Task ExecuteQueryAsync(string name, HttpContext context,
            IEnumerable<KeyValuePair<string, string>> metadata)
        {
            _log.LogTrace($"Execution query '{name}' from OWIN middleware");
            string requestJson;
            using (var streamReader = new StreamReader(context.Request.Body))
                requestJson = await streamReader.ReadToEndAsync();
            try
            {
                var result = await ExecuteQueryInternalAsync(name, requestJson, metadata, CancellationToken.None);
                await WriteAsync(result, HttpStatusCode.OK, context);
            }
            catch (ArgumentException ex)
            {
                _log.LogDebug(ex, $"Failed to execute serialized query '{name}' due to: {ex.Message}");
                await WriteErrorAsync(ex.Message, HttpStatusCode.BadRequest, context);
            }
            catch (ForbiddenException ex)
            {
                _log.LogDebug(ex, $"Forbidden execute serialized query '{name}' due to: {ex.Message}");
                await WriteAsync(ex.Message, HttpStatusCode.Forbidden, context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.LogDebug(ex, $"Unauthorized Access query '{name}' due to: {ex.Message}");
                await WriteAsync(ex.Message, HttpStatusCode.Unauthorized, context);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected exception when executing query '{name}' ");
                await WriteErrorAsync("Internal server error!", HttpStatusCode.InternalServerError, context);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private async Task<object> ExecuteQueryInternalAsync(string name, string json,
            IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken none)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (!_platform.Definitions.TryGetDefinition(name, out QueryDefinition queryDefinition))
                throw new ArgumentException($"No command definition found for query '{name}'");

            IQuery query;
            try
            {
                query = (IQuery)JsonConvert.DeserializeObject(json, queryDefinition.QueryType);
                ((Query)query)?.MergeMetadata(new QueryMetadata(metadata));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to deserialize query '{name}': {ex.Message}", ex);
            }
            var executionResult = await _platform.QueryAsync(query);
            return executionResult;            
        }

        private async Task PublishCommandOrServiceAsync(string contextName, string name, HttpContext context,
            IEnumerable<KeyValuePair<string, string>> metadata)
        {
            _log.LogTrace($"Publishing command '{name}' in context '{contextName}' from OWIN middleware");
            string requestJson;
            using (var streamReader = new StreamReader(context.Request.Body))
                requestJson = await streamReader.ReadToEndAsync();
            try
            {
                var result = await PublishSerilizedCommandOrServiceInternalAsync(contextName, name, requestJson, metadata, CancellationToken.None);
                if (result is ForbiddenResult)
                {
                    await WriteAsync(result, HttpStatusCode.Forbidden, context);
                    return;
                }
                if (result is UnauthorizedResult)
                {
                    await WriteAsync(result.ToString(), HttpStatusCode.Unauthorized, context);
                    return;
                }
                await WriteAsync(result, HttpStatusCode.OK, context);
            }
            catch (ArgumentException ex)
            {
                _log.LogDebug(ex, $"Failed to publish serialized command '{name}' due to: {ex.Message}");
                await WriteErrorAsync(ex.Message, HttpStatusCode.BadRequest, context);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected exception when executing '{name}' ");
                await WriteErrorAsync("Internal server error!", HttpStatusCode.InternalServerError, context);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private async Task<object> PublishSerilizedCommandOrServiceInternalAsync(string context, string name,
            string json, IEnumerable<KeyValuePair<string, string>> metadata, CancellationToken none)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (_platform.Definitions.TryGetDefinition(context, name, out CommandDefinition commandDefinition))
            {
                ICommand command;
                string id;
                try
                {
                    command = (ICommand) JsonConvert.DeserializeObject(json, commandDefinition.CommandType);
                    id = ((JsonConvert.DeserializeObject<JObject>(json))?["Id"] ??
                          throw new InvalidOperationException()).Value<string>();
                    ((Command)command)?.MergeMetadata(new CommandMetadata(metadata));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to deserialize command '{name}': {ex.Message}", ex);
                }

                var executionResult = await _platform.ExecuteAsync(id, command);
                return executionResult;
            }

            if (_platform.Definitions.TryGetDefinition(context, name, out ServiceDefinition serviceDefinition))
            {
                var value = JsonConvert.DeserializeObject(json);
                if (value != null)
                {
                    var obj = JsonConvert.DeserializeObject<JObject>(json);
                    var parameters = obj != null ? obj.Properties()
                        .ToDictionary(i => i.Name, v => v.Value.ToObject<object>()) : new Dictionary<string, object>();

                    return  await _platform.Service(serviceDefinition.InterfaceType).Invoke(serviceDefinition.MethodName, parameters);
                }


            }
            throw new ArgumentException($"No command definition found for command '{name}'");
        }

        private async Task WriteAsync(object obj, HttpStatusCode statusCode, HttpContext context)
        {
            var json = JsonConvert.SerializeObject(obj);
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(json);
        }

        private Task WriteErrorAsync(
            string errorMessage,
            HttpStatusCode statusCode,
            HttpContext context)
        {
            return WriteAsync(new { ErrorMessage = errorMessage }, statusCode, context);
        }
    }
}