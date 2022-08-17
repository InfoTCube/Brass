using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Method)]
public class Put : Attribute
{
    public string Route { get; } = "";
    public Put() {}
    public Put(string route)
    {
        this.Route = route;
    }
}