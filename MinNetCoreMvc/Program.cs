using MinNetCoreMvc.MinNetCoreMvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMinControllers();

var app = builder.Build();

app.MapMinControllerRoute(name: "default", pattern: "{controller}/{action}/{id?}");

app.MapGet("/", () => "Hello World!");


app.Run();


public class HomeController {
    
    public Result Foo(string x, int y, double z) => new Result(x, y, z); /// http://localhost:7113/home/foo?x=11&y=22&z=33

    [Microsoft.AspNetCore.Mvc.HttpGet("bar/{x}/{y}/{z}")]
    public ValueTask<Result> Bar(string x, int y, double z) => ValueTask.FromResult(new Result(x, y, z));/// http://localhost:7113/bar/11/22/33

    [Microsoft.AspNetCore.Mvc.HttpGet("bay/{x}/{y}/{z}")]
    public ValueTask<MinObjectActionResult> Bay(string x, int y, double z) => ValueTask.FromResult(new MinObjectActionResult(new Result(x, y, z)));//http://localhost:7113/bay/11/22/33

    /// http://localhost:7113/baz
    /// {"X":"abc","Y":"222","Z":333}
    [Microsoft.AspNetCore.Mvc.HttpPost("/baz")]
    public ValueTask<IMinActionResult> Baz(Result input) => ValueTask.FromResult<IMinActionResult>(new MinJsonResult(input));
}

public record Result {
    public string X { get; set; }
    public int Y { get; set; }
    public double Z { get; set; }
    public Result(string x, int y, double z) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
}