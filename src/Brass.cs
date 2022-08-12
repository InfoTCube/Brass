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

namespace Brass;
public class App
{
    private HttpListener? listener;
    private Dictionary<string, MethodInfo> endpoints = new Dictionary<string, MethodInfo>();

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

            try
            {
                if(req.RawUrl != null)
                    Console.WriteLine(endpoints[req.RawUrl.ToString().Remove(0,1)].Name);
            }
            catch
            {
                resp.StatusCode = 404;
                resp.Close();
            }

            if(content != "")
            {
                content = Regex.Replace(content, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
                byte[] data = Encoding.UTF8.GetBytes(content);
                resp.ContentType = "text/json";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
            }
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
            var get = method.GetCustomAttribute(typeof(Get), false) as Get;
            var apiController = method.ReflectedType?.GetCustomAttribute(typeof(ApiController), false) as ApiController;
            endpoints[$"{apiController?.Route.Replace("[controller]", method.ReflectedType?.Name)}/{get?.Route}"] = method;
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
        Console.WriteLine($"Listening for connections on {url}");
        
        Task listenTask = HandleIncomingConnections();
        listenTask.GetAwaiter().GetResult();

        listener.Close();
    }
}