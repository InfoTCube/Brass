using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Class)] 
public class ApiController : Attribute
{
    public string Route { get; } = "";
    public ApiController() {}
    public ApiController(string route)
    {
        this.Route = route;
    }
}
