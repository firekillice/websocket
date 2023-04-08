using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    [Route("Home")]
    public string Index()
    {
        return "Hello from Index method of Home Controller";
    }
}
