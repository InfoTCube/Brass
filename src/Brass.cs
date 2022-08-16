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

namespace Brass;
public class App
{
    private HttpListener? listener;
    private Dictionary<string, Tuple<MethodInfo, Type?>> endpoints = new Dictionary<string, Tuple<MethodInfo, Type?>>();

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
            if(url.Length > 0 && url.Last() == '/') url = url.Remove(url.Length - 1);

            if(!endpoints.ContainsKey(url))
            {
                resp.StatusCode = 404;
                resp.Close();
                continue;
            }

            try
            {
                var method = endpoints[url].Item1;
                var type = endpoints[url].Item2;
                dynamic? output = method.Invoke(null, null);

                status = output?.StatusCode;
                if(type != null)
                    content = JsonSerializer.Serialize(output?.Content);

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
                content = Regex.Replace(content, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
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
            .Where(m => m.GetCustomAttributes(typeof(Get), false).Length > 0)
            .ToArray();

        foreach(var method in controllersMethods)
        {
            if(method.ReturnType != typeof(Result) && method.ReturnType.Name != typeof(Result<>).Name) 
                throw new Exception($"Controller - {method.Name} must return Result or Result<> type...");

            if(!method.IsStatic) 
                throw new Exception($"Controller - {method.Name} must be static...");

            var get = method.GetCustomAttribute(typeof(Get), false) as Get;
            var apiController = method.ReflectedType?.GetCustomAttribute(typeof(ApiController), false) as ApiController;
            string key = "";
            if(apiController?.Route != "") key += apiController?.Route.Replace("[controller]", method.ReflectedType?.Name);
            if(get?.Route != "") key += $"/{get?.Route}";
            Type? type = null;
            if(method.ReturnType.IsGenericType)
                type = method.ReturnType.GetGenericArguments().FirstOrDefault();
            endpoints[key] = new Tuple<MethodInfo, Type?>(method, type);
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