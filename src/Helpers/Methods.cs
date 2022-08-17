using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Brass.Controllers;

namespace Brass.Helpers;

public static class Methods
{
    public static string GetHttpMethodWithRoute(ref string key, MethodInfo? method)
    {
        var get = method?.GetCustomAttribute(typeof(Get), false) as Get;
        if(get != null)
        {
            if(get?.Route != "") key += $"/{get?.Route}";
            return "GET";
        }
        var put = method?.GetCustomAttribute(typeof(Put), false) as Put;
        if(put != null)
        {
            if(put?.Route != "") key += $"/{put?.Route}";
            return "PUT";
        }
        var post = method?.GetCustomAttribute(typeof(Post), false) as Post;
        if(post != null)
        {
            if(post?.Route != "") key += $"/{post?.Route}";
            return "POST";
        }
        var patch = method?.GetCustomAttribute(typeof(Patch), false) as Patch;
        if(patch != null)
        {
            if(patch?.Route != "") key += $"/{patch?.Route}";
            return "PATCH";
        }
        var delete = method?.GetCustomAttribute(typeof(Delete), false) as Delete;
        if(delete != null)
        {
            if(delete?.Route != "") key += $"/{delete?.Route}";
            return "DELETE";
        }

        return "GET";
    }
}
