using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Method)]
public class Delete : Attribute
{
    public string Route { get; } = "";
    public Delete() {}
    public Delete(string route)
    {
        this.Route = route;
    }
}