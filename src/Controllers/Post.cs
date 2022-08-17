using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Method)]
public class Post : Attribute
{
    public string Route { get; } = "";
    public Post() {}
    public Post(string route)
    {
        this.Route = route;
    }
}