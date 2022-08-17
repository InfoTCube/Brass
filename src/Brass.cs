using System;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Brass.Controllers;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Brass.Helpers;
using Brass.Controllers.Parameters;

namespace Brass;
public class App
{
    private HttpListener? listener;

    // Tuple<url endpoint, http method>, Tuple<method, returning type, parameters, route parameter>
    private Dictionary<Tuple<string, string>, Tuple<MethodInfo, Type?, ParameterInfo[]?, string>> endpoints 
        = new Dictionary<Tuple<string, string>, Tuple<MethodInfo, Type?, ParameterInfo[]?, string>>();

    private async Task HandleIncomingConnections()
    {
        if(listener is null) return;
        bool runServer = true;
        while (runServer)
        {
            HttpListenerContext ctx = await listener.GetContextAsync();

            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            string content = "";
            int status = 500;
            
            if(req.RawUrl == null)
            {
                resp.StatusCode = 404;
                resp.Close();
                continue;
            }

            string url = req.RawUrl.Remove(0,1);
            url = url.Contains('?') ? url.Remove(url.IndexOf('?')) : url;
            if(url.Length > 0 && url.Last() == '/') url = url.Remove(url.Length - 1);
            string routeParamValue = url.Contains('/') ? url.Substring(url.LastIndexOf("/") + 1) : "";
            
            var apiMethod = new Tuple<string, string>(url, req.HttpMethod);

            if(!endpoints.ContainsKey(apiMethod))
            {
                url = url.Contains('/') ? url.Remove(url.LastIndexOf('/')) : url;
                apiMethod = new Tuple<string, string>(url, req.HttpMethod);
                if(!endpoints.ContainsKey(apiMethod) || endpoints[apiMethod].Item4 == "")
                {
                    resp.StatusCode = 404;
                    resp.Close();
                    continue;
                }
            }

            try
            {
                var method = endpoints[apiMethod].Item1;
                var type = endpoints[apiMethod].Item2;
                var parameters = endpoints[apiMethod].Item3;
                var routeParam = endpoints[apiMethod].Item4;

                Object?[]? parametersObject = null;

                if(parameters != null)
                {
                    parametersObject = new Object[parameters.Count()];

                    int i = 0; //iterator
                    foreach(var param in parameters)
                    {
                        if(param.GetCustomAttributes(typeof(FromRoute), false).Length > 0) parametersObject[i] = Parameters.ParseFromRoute(param, routeParam, routeParamValue);
                        else if(param.GetCustomAttributes(typeof(FromBody), false).Length > 0) parametersObject[i] = await Parameters.ParseFromBody(param, req);
                        else if(param.GetCustomAttributes(typeof(FromQuery), false).Length > 0) parametersObject[i] = Parameters.ParseFromQuery(param, req);
                        else if(param.GetCustomAttributes(typeof(FromHeader), false).Length > 0) parametersObject[i] = Parameters.ParseFromHeaders(param, req);
                        else if(param.Name == routeParam) parametersObject[i] = Parameters.ParseFromRoute(param, routeParam, routeParamValue);
                        else if(!param.ParameterType.IsPrimitiveType()) await Parameters.ParseFromBody(param, req);
                        else 
                        {
                            parametersObject[i] = Parameters.ParseFromQuery(param, req);
                            if(parametersObject[i] != null) 
                            {
                                ++i;
                                continue;
                            }

                            parametersObject[i] = Parameters.ParseFromHeaders(param, req);
                        }

                        ++i;
                        continue;
                    }
                }

                dynamic? output = method.Invoke(null, parametersObject);

                status = output?.StatusCode;
                if(type != null) content = JsonSerializer.Serialize(output?.Content);
            }
            catch(Exception e)
            {
                resp.StatusCode = 500;
                byte[] data = Encoding.UTF8.GetBytes(e.Message);
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                resp.ContentType = "text";
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
                continue;
            }

            if(content != "")
            {
                byte[] data = Encoding.UTF8.GetBytes(content);
                resp.StatusCode = status;
                resp.ContentType = "text/json";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
            }
            resp.StatusCode = status;
            resp.Close();
        }
    }

    public void AddControllers()
    {
        var controllersMethods = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(t => t.GetTypes())
            .Where(m => m.GetCustomAttributes(typeof(ApiController), false).Length > 0)
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(Get), false).Length > 0 
                || m.GetCustomAttributes(typeof(Post), false).Length > 0
                || m.GetCustomAttributes(typeof(Patch), false).Length > 0
                || m.GetCustomAttributes(typeof(Put), false).Length > 0
                || m.GetCustomAttributes(typeof(Delete), false).Length > 0)
            .ToArray();

        foreach(var method in controllersMethods)
        {
            if(method.ReturnType != typeof(Result) && method.ReturnType.Name != typeof(Result<>).Name) 
                throw new Exception($"Controller - {method.Name} must return Result or Result<> type...");

            if(!method.IsStatic) 
                throw new Exception($"Controller - {method.Name} must be static...");

            string httpMethod = "GET";
            string key = "";
            var apiController = method.ReflectedType?.GetCustomAttribute(typeof(ApiController), false) as ApiController;
            if(apiController?.Route != "") key += apiController?.Route.Replace("[controller]", method.ReflectedType?.Name);

            httpMethod = Methods.GetHttpMethodWithRoute(ref key, method);

            var parameters = method.GetParameters();

            //look for route parameters
            string routeParam = "";
            if(key.Contains('{') && key.Contains('}'))
            {
                routeParam = key.Split('{', '}')[1];
            }
            key = key.Contains('{') ? key.Remove(key.IndexOf('{')-1) : key;

            Type? type = null;
            if(method.ReturnType.IsGenericType)
                type = method.ReturnType.GetGenericArguments().FirstOrDefault();

            endpoints[new Tuple<string, string>(key, httpMethod)] = new Tuple<MethodInfo, Type?, ParameterInfo[]?, string>(method, type, parameters, routeParam);
        }
    }

    public void Run(int port = 7777)
    {
        if(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(x => x.Port == port))
        {
            throw new Exception($"Port {port} is already in use...");
        }

        string url = $"http://localhost:{port}/";
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine($"Listening on {url}");
        
        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        listener.Close();
    }
}