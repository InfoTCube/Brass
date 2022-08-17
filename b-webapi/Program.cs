using System;
using Brass;

namespace Bwebapi;
static class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        app.AddControllers();
        app.Run();
    }
}