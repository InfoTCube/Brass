using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Method)]
public class Patch : Attribute
{
    public string Route { get; } = "";
    public Patch() {}
    public Patch(string route)
    {
        this.Route = route;
    }
}