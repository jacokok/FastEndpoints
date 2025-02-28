﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using static FastEndpoints.Config;

namespace FastEndpoints;

public abstract partial class Endpoint<TRequest, TResponse> : BaseEndpoint where TRequest : notnull, new() where TResponse : notnull, new()
{
    private static readonly Type tRequest = typeof(TRequest);
    private static readonly Type tResponse = typeof(TResponse);
    private static readonly bool isPlainTextRequest = Types.IPlainTextRequest.IsAssignableFrom(tRequest);

    /// <summary>
    /// specify to listen for GET requests on one or more routes.
    /// </summary>
    protected void Get(params string[] routePatterns)
    {
        Verbs(Http.GET);
        Routes(routePatterns);
    }

    /// <summary>
    /// specify to listen for POST requests on one or more routes.
    /// </summary>
    protected void Post(params string[] routePatterns)
    {
        Verbs(Http.POST);
        Routes(routePatterns);
    }

    /// <summary>
    /// specify to listen for PUT requests on one or more routes.
    /// </summary>
    protected void Put(params string[] routePatterns)
    {
        Verbs(Http.PUT);
        Routes(routePatterns);
    }

    /// <summary>
    /// specify to listen for PATCH requests on one or more routes.
    /// </summary>
    protected void Patch(params string[] routePatterns)
    {
        Verbs(Http.PATCH);
        Routes(routePatterns);
    }

    /// <summary>
    /// specify to listen for DELETE requests on one or more routes.
    /// </summary>
    protected void Delete(params string[] routePatterns)
    {
        Verbs(Http.DELETE);
        Routes(routePatterns);
    }

    /// <summary>
    /// specify one or more route patterns this endpoint should be listening for
    /// </summary>
    protected void Routes(params string[] patterns)
    {
        Configuration.Routes = patterns;
    }

    /// <summary>
    /// specify one or more http method verbs this endpoint should be accepting requests for
    /// </summary>
    protected void Verbs(params Http[] methods)
    {
        Configuration.Verbs = methods.Select(m => m.ToString()).ToArray();

        //default openapi descriptions (it's here because we need access to TRequest/TResponse)
        Configuration.InternalConfigAction = b =>
        {
            if (isPlainTextRequest)
            {
                b.Accepts<TRequest>("text/plain", "application/json");
                b.Produces<TResponse>(200, "text/plain", "application/json");
                return;
            }

            if (tRequest != Types.EmptyRequest)
            {
                if (methods.Contains(Http.GET))
                    b.Accepts<TRequest>("*/*", "application/json");
                else
                    b.Accepts<TRequest>("application/json");
            }

            if (tResponse == Types.Object || tResponse == Types.EmptyResponse)
                b.Produces<TResponse>(200, "text/plain", "application/json");
            else
                b.Produces<TResponse>(200, "application/json");
        };
    }

    /// <summary>
    /// disable auto validation failure responses (400 bad request with error details) for this endpoint
    /// </summary>
    protected void DontThrowIfValidationFails() => Configuration.ThrowIfValidationFails = false;

    /// <summary>
    /// allow unauthenticated requests to this endpoint. optionally specify a set of verbs to allow unauthenticated access with.
    /// i.e. if the endpoint is listening to POST, PUT &amp; PATCH and you specify AllowAnonymous(Http.POST), then only PUT &amp; PATCH will require authentication.
    /// </summary>
    protected void AllowAnonymous(params Http[] verbs)
    {
        Configuration.AnonymousVerbs =
            verbs.Length > 0
            ? verbs.Select(v => v.ToString()).ToArray()
            : Enum.GetNames(Types.Http);
    }

    /// <summary>
    /// enable file uploads with multipart/form-data content type
    /// </summary>
    protected void AllowFileUploads() => Configuration.AllowFormData = true;

    /// <summary>
    /// enable multipart/form-data submissions
    /// </summary>
    protected void AllowFormData() => Configuration.AllowFormData = true;

    /// <summary>
    /// specify one or more authorization policy names you have added to the middleware pipeline during app startup/ service configuration that should be applied to this endpoint.
    /// </summary>
    /// <param name="policyNames">one or more policy names (must have been added to the pipeline on startup)</param>
    protected void Policies(params string[] policyNames) => Configuration.PreBuiltUserPolicies = policyNames;

    /// <summary>
    /// allows access if the claims principal has ANY of the given roles
    /// </summary>
    /// <param name="rolesNames">one or more roles that has access</param>
    protected void Roles(params string[] rolesNames) => Configuration.Roles = rolesNames;

    /// <summary>
    /// allows access if the claims principal has ANY of the given permissions
    /// </summary>
    /// <param name="permissions">the permissions</param>
    protected void Permissions(params string[] permissions)
    {
        Configuration.AllowAnyPermission = true;
        Configuration.Permissions = permissions;
    }

    /// <summary>
    /// allows access if the claims principal has ALL of the given permissions
    /// </summary>
    /// <param name="permissions">the permissions</param>
    protected void PermissionsAll(params string[] permissions)
    {
        Configuration.AllowAnyPermission = false;
        Configuration.Permissions = permissions;
    }

    /// <summary>
    /// allows access if the claims principal has ANY of the given claim types
    /// </summary>
    /// <param name="claimTypes">the claim types</param>
    protected void Claims(params string[] claimTypes)
    {
        Configuration.AllowAnyClaim = true;
        Configuration.ClaimTypes = claimTypes;
    }

    /// <summary>
    /// allows access if the claims principal has ALL of the given claim types
    /// </summary>
    /// <param name="claimTypes">the claim types</param>
    protected void ClaimsAll(params string[] claimTypes)
    {
        Configuration.AllowAnyClaim = false;
        Configuration.ClaimTypes = claimTypes;
    }

    /// <summary>
    /// specify which authentication schemes to use for authenticating requests to this endpoint
    /// </summary>
    /// <param name="authSchemeNames">the authentication scheme names</param>
    protected void AuthSchemes(params string[] authSchemeNames)
    {
        Configuration.AuthSchemes = authSchemeNames;
    }

    /// <summary>
    /// configure a collection of pre-processors to be executed before the main handler function is called. processors are executed in the order they are defined here.
    /// </summary>
    /// <param name="preProcessors">the pre processors to be executed</param>
    protected void PreProcessors(params IPreProcessor<TRequest>[] preProcessors) => Configuration.PreProcessors = preProcessors;

    /// <summary>
    /// configure a collection of post-processors to be executed after the main handler function is done. processors are executed in the order they are defined here.
    /// </summary>
    /// <param name="postProcessors">the post processors to be executed</param>
    protected void PostProcessors(params IPostProcessor<TRequest, TResponse>[] postProcessors) => Configuration.PostProcessors = postProcessors;

    /// <summary>
    /// specify response caching settings for this endpoint
    /// </summary>
    /// <param name="durationSeconds">the duration in seconds for which the response is cached</param>
    /// <param name="location">the location where the data from a particular URL must be cached</param>
    /// <param name="noStore">specify whether the data should be stored or not</param>
    /// <param name="varyByHeader">the value for the Vary response header</param>
    /// <param name="varyByQueryKeys">the query keys to vary by</param>
    protected void ResponseCache(int durationSeconds, ResponseCacheLocation location = ResponseCacheLocation.Any, bool noStore = false, string? varyByHeader = null, string[]? varyByQueryKeys = null)
    {
        Configuration.ResponseCacheSettings = new()
        {
            Duration = durationSeconds,
            Location = location,
            NoStore = noStore,
            VaryByHeader = varyByHeader,
            VaryByQueryKeys = varyByQueryKeys
        };
    }

    /// <summary>
    /// set endpoint configurations options using an endpoint builder action ///
    /// </summary>
    /// <param name="builder">the builder for this endpoint</param>
    protected void Options(Action<RouteHandlerBuilder> builder) => Configuration.UserConfigAction = builder;

    /// <summary>
    /// describe openapi metadata for this endpoint. this method clears the default Accepts/Produces metadata.
    /// <c>b => b.Accepts&lt;Request&gt;("text/plain")</c>
    /// </summary>
    /// <param name="builder">the route handler builder for this endpoint</param>
    protected void Describe(Action<RouteHandlerBuilder> builder)
    {
        Action<RouteHandlerBuilder> clearDefaultsAction = b =>
        {
            b.Add(epBuilder =>
            {
                foreach (var m in epBuilder.Metadata.Where(o => o.GetType().Name is "ProducesResponseTypeMetadata" or "AcceptsMetadata").ToArray())
                    epBuilder.Metadata.Remove(m);
            });
        };

        Configuration.UserConfigAction = clearDefaultsAction + builder;
    }

    //todo: remove in v4.0
    [Obsolete("Use the one of the other Summary() overloads.", false)]
    protected void Summary(string endpointDescription, params (int statusCode, string statusCodeDescription)[] statusCodeDescriptions)
    {
        Summary(s =>
        {
            s.Description = endpointDescription;
            foreach (var (code, desc) in statusCodeDescriptions)
                s[code] = desc;
        });
    }

    /// <summary>
    /// provide a summary/description for this endpoint to be used in swagger/ openapi
    /// </summary>
    /// <param name="endpointSummary">an action that sets values of an endpoint summary object</param>
    protected void Summary(Action<EndpointSummary> endpointSummary)
    {
        Configuration.Summary = new();
        endpointSummary(Configuration.Summary);
    }

    /// <summary>
    /// provide a summary/description for this endpoint to be used in swagger/ openapi
    /// </summary>
    /// <param name="endpointSummary">an action that sets values of an endpoint summary object</param>
    protected void Summary(Action<EndpointSummary<TRequest>> endpointSummary)
    {
        var summary = new EndpointSummary<TRequest>();
        endpointSummary(summary);
        Configuration.Summary = summary;
    }

    /// <summary>
    /// provide a summary/description for this endpoint to be used in swagger/ openapi
    /// </summary>
    /// <param name="endpointSummary">an endpoint summary instance</param>
    protected void Summary(EndpointSummary endpointSummary)
    {
        Configuration.Summary = endpointSummary;
    }

    /// <summary>
    /// specify one or more string tags for this endpoint so they can be used in the exclusion filter during registration.
    /// </summary>
    /// <param name="endpointTags">the tag values to associate with this endpoint</param>
    protected void Tags(params string[] endpointTags)
    {
        Configuration.Tags = endpointTags;
    }

    /// <summary>
    /// specify the version of the endpoint if versioning is enabled
    /// </summary>
    /// <param name="version">the version of this endpoint</param>
    /// <param name="deprecateAt">the version group number starting at which this endpoint should not be included in swagger</param>
    protected void Version(int version, int? deprecateAt = null)
    {
        Configuration.Version.Current = GetVersion(version);
        Configuration.Version.DeprecatedAt = deprecateAt ?? 0;

        static int GetVersion(int epVer)
        {
            return
                epVer is 0 && VersioningOpts is not null
                ? VersioningOpts.DefaultVersion
                : epVer;
        }
    }

    /// <summary>
    /// rate limit requests to this endpoint based on a request http header sent by the client.
    /// </summary>
    /// <param name="hitLimit">how many requests are allowed within the given duration</param>
    /// <param name="durationSeconds">the frequency in seconds where the accrued hit count should be reset</param>
    /// <param name="headerName">
    /// the name of the request header used to uniquely identify clients.
    /// specifying null will first look for 'X-Forwarded-For' and if not present, will use `HttpContext.Connection.RemoteIpAddress`
    /// </param>
    protected void Throttle(int hitLimit, double durationSeconds, string? headerName = null)
    {
        Configuration.HitCounter = new(headerName, durationSeconds, hitLimit);
    }

    /// <summary>
    /// specify the json serializer context if code generation for request/response dtos
    /// </summary>
    /// <typeparam name="TContext">the type of the json serializer context for this endpoint</typeparam>
    protected void SerializerContext<TContext>() where TContext : JsonSerializerContext
    {
        Configuration.SerializerContext =
            (JsonSerializerContext)Activator.CreateInstance(
                typeof(TContext),
                new JsonSerializerOptions(SerializerOpts))!;
    }

    /// <summary>
    /// register the validator for this endpoint as scoped instead of singleton. which will enable constructor injection at the cost of performance.
    /// </summary>
    protected void ScopedValidator() => Configuration.ScopedValidator = true;
}