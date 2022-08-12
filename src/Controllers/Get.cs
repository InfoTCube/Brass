using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

[AttributeUsage(AttributeTargets.Method)] 
public class Get : Attribute
{
    public string Route { get; } = "";
    public Get() {}
    public Get(string route) 
    {   
        this.Route = route;
    }
}
