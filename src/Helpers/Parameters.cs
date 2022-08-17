using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Brass.Helpers;

public static class Parameters
{
    public static object? ParseFromRoute(ParameterInfo? param, string routeParam, string routeParamValue)
    {
        return param?.Name == routeParam ? Convert.ChangeType(routeParamValue, param.ParameterType) : null;
    }

    public static async Task<object?> ParseFromBody(ParameterInfo? param, HttpListenerRequest req)
    {
        string? body = await new System.IO.StreamReader(req.InputStream, req.ContentEncoding).ReadToEndAsync();
        return body != "" && param?.ParameterType != null ? JsonSerializer.Deserialize(body, param.ParameterType) : null;
    }

    public static object? ParseFromQuery(ParameterInfo? param, HttpListenerRequest req)
    {
        var value = req.QueryString[param?.Name];
        return value != null && param?.ParameterType != null ? Convert.ChangeType(value, param.ParameterType) : null;
    }

    public static object? ParseFromHeaders(ParameterInfo? param, HttpListenerRequest req)
    {
        var value = req.Headers[param?.Name];
        return value != null && param?.ParameterType != null ? Convert.ChangeType(value, param.ParameterType) : null;
    }
}
