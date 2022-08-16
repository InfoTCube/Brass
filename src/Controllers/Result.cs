using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brass.Controllers;

public class Result
{
    public Result() {}

    public Result(int? statusCode)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; set; } = 200;
}

public class Result<T>
{
    public Result() {}
    public Result(int? statusCode, T? content)
    {
        StatusCode = statusCode;
        Content = content;
    }

    public int? StatusCode { get; set; } = 200;
    public T? Content { get; set; }
}