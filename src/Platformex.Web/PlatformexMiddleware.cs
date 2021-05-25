using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            _commandPath = new Regex(basePath + "/(?<name>[a-z]+)/{0,1}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _queryPath = new Regex(basePath + "/(?<name>[a-z0-9]+)/{0,1}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            if (context.Request.Method == HttpMethods.Put && path.HasValue)
            {
                var match = _commandPath.Match(path.Value);
                if (match.Success)
                {
                    await PublishCommandAsync(match.Groups["name"].Value, context).ConfigureAwait(false);
                    return;
                }
            }
            if (context.Request.Method == HttpMethods.Post && path.HasValue)
            {
                var match = _queryPath.Match(path.Value);
                if (match.Success)
                {
                    await ExecuteQueryAsync(match.Groups["name"].Value, context).ConfigureAwait(false);
                    return;
                }
            }
            await _next(context);
        }
        private async Task ExecuteQueryAsync(String name, HttpContext context)
        {
            _log.LogTrace($"Execution query '{name}' from OWIN middleware");
            String requestJson;
            using (var streamReader = new StreamReader(context.Request.Body))
                requestJson = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            try
            {
                var result = await ExecuteQueryInternalAsync(name, requestJson, CancellationToken.None).ConfigureAwait(false);
                await WriteAsync(result, HttpStatusCode.OK, context).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                _log.LogDebug(ex, $"Failed to execute serialized query '{name}' due to: {ex.Message}");
                await WriteErrorAsync(ex.Message, HttpStatusCode.BadRequest, context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected exception when executing query '{name}' ");
                await WriteErrorAsync("Internal server error!", HttpStatusCode.InternalServerError, context).ConfigureAwait(false);
            }
        }

        private Task<object> ExecuteQueryInternalAsync(string name, string requestJson, CancellationToken none)
        {
            throw new NotImplementedException();
        }

        private async Task PublishCommandAsync(String name, HttpContext context)
        {
            _log.LogTrace($"Publishing command '{name}' from OWIN middleware");
            String requestJson;
            using (var streamReader = new StreamReader(context.Request.Body))
                requestJson = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            try
            {
                var result = await PublishSerilizedCommandInternalAsync(name, requestJson, CancellationToken.None).ConfigureAwait(false);
                await WriteAsync(result, HttpStatusCode.OK, context).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                _log.LogDebug(ex, $"Failed to publish serialized command '{name}' due to: {ex.Message}");
                await WriteErrorAsync(ex.Message, HttpStatusCode.BadRequest, context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Unexpected exception when executing '{name}' ");
                await WriteErrorAsync("Internal server error!", HttpStatusCode.InternalServerError, context).ConfigureAwait(false);
            }
        }

        private async Task<object> PublishSerilizedCommandInternalAsync(string name, string json, CancellationToken none)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (!_platform.Definitions.TryGetDefinition(name, out CommandDefinition commandDefinition))
                throw new ArgumentException($"No command definition found for command '{name}'");

            ICommand command;
            try
            {
                command = (ICommand)JsonConvert.DeserializeObject(json, commandDefinition.CommandType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to deserialize command '{name}': {ex.Message}", ex);
            }
            var executionResult = await _platform.ExecuteAsync(command);
            return executionResult;
        }

        private async Task WriteAsync(Object obj, HttpStatusCode statusCode, HttpContext context)
        {
            var json = JsonConvert.SerializeObject(obj);
            context.Response.StatusCode = (Int32)statusCode;
            await context.Response.WriteAsync(json).ConfigureAwait(false);
        }

        private Task WriteErrorAsync(
            String errorMessage,
            HttpStatusCode statusCode,
            HttpContext context)
        {
            return WriteAsync(new { ErrorMessage = errorMessage }, statusCode, context);
        }
    }
}