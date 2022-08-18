---
sidebar_position: 1
---

# Api Controller

Api controller is a class which can be created under the Controllers folder or any other folder under your project's root folder. The name of a controller class must end with "Controller", it must have **[ApiController]** attribiute. All the public methods of the controller are called action methods whuch must be static and return **Result** or **Result`<T>`** object.

## Create your first controller

Create new C# class: **StatusController.cs** in Controllers folder (or any other you store you controllers at).

```cs title="Controllers/StatusController.cs"
using Brass.Controllers;
using Brass.Controllers.Parameters;

namespace myApp.Controllers;

[ApiController("[controller]")]
public static class StatusController
{
    [Get("OK")]
    public static Result Ok()
    {
        return Results.Ok();
    }
}
```

Now, you can run your API
```bash
dotnet watch run
```

There is a new endpoint at [http://localhost:7777/StatusController/OK](http://localhost:7777/StatusController/OK).

It doesn't return any content, only 200 - OK status code.

## Adding more methods

Add next controller method to **StatusController** class.

```cs
[Get("FromCode/{code}")]
public static Result FromCode(int code)
{
    if(code == 200) return Results.Ok();
    else if(code == 204) return Result.NoContent();
    else if(code == 404) return Result.NotFound();

    return Result.InternalServerError();
}
```

Now, there is a new endpoint at [http://localhost:7777/StatusController/FromCode](http://localhost:7777/StatusController/FromCode).

It doesn't return any content, only status code based on code from route.