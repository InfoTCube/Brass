using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromHeader : Attribute {}

