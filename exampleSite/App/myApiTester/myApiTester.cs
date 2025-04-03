using Microsoft.AspNetCore.Builder;
using utility;

public static class myApiTesterApi
{
    static myInclude my = new myInclude();
    public static bool isLogin(HttpContext context)
    {
        if (context.Session.GetString("APITESTER_ISLOGIN") == null ||
        context.Session.GetString("APITESTER_ISLOGIN") != "YES")
        {
            return false;
        }
        return true;
    }
    public static bool isLoginJump(HttpContext context) // 如果是 false 跳登入
    {
        if (context.Session.GetString("APITESTER_ISLOGIN") == null ||
        context.Session.GetString("APITESTER_ISLOGIN") != "YES")
        {
            context.Response.Redirect(my.base_url(context) + "apitester/login");
            my.echoBinary(context, my.s2b("請先登入"));
            my.exit(context);
            return false;
        }
        return true;
    }
    public static void RegisterTestApi(this IEndpointRouteBuilder app)
    {

        //UI 控台
        var group = app.MapGroup("/apitester");

        group.MapGet("/", (HttpContext context) =>
        {
            if (context.Session.GetString("APITESTER_ISLOGIN") == null ||
            context.Session.GetString("APITESTER_ISLOGIN") != "YES")
            {
                return Results.Redirect(my.base_url(context) + "apitester/login");
            }
            return Results.Redirect(my.base_url(context) + "apitester/showPanel");
        });
        group.MapPost("/login_check", (HttpContext context) =>
        {
            string POSTS_STRING = "pd";
            var POSTS = my.getGET_POST(context, POSTS_STRING, "POST");
            string pd = POSTS["pd"].ToString();
            string pd_has = my.sha256("羽山好棒棒_" + pd);
            var o = new Dictionary<string, object>();
            if (pd_has == "8496859f029edb451ded11e72de355022f9ce23ea856c5485f14d2e1e4f19f99")
            {
                context.Session.SetString("APITESTER_ISLOGIN", "YES");

                o["status"] = "OK";
                return Results.Json(o);
            }

            o["status"] = "NO";
            o["reason"] = "密碼錯誤";
            return Results.Json(o);
        });
        group.MapGet("/showPanel", (HttpContext context) =>
        {
            isLoginJump(context);
            string op = Path.Combine(my.base_dir(), "App", "myApiTester", "html", "app_tester.html");
            string html = my.b2s(my.file_get_contents(op));
            // 一次取代四個
            html = html.Replace("{{BASE_URL}}", my.base_url(context));
            return Results.Bytes(my.s2b(html), "text/html");
        });
        group.MapGet("/getApiLists", (HttpContext context) =>
        {
            isLoginJump(context);
            string op = Path.Combine(my.base_dir(), "App", "myApiTester", "html", "apiLists.txt");
            string txt = my.b2s(my.file_get_contents(op));
            return Results.Bytes(my.s2b(txt), "application/json");
        });
        group.MapGet("/top", (HttpContext context) =>
        {
            isLoginJump(context);
            string op = Path.Combine(my.base_dir(), "App", "myApiTester", "template", "top.html");
            string html = my.b2s(my.file_get_contents(op));
            html = html.Replace("{{BASE_URL}}", my.base_url(context));
            return Results.Bytes(my.s2b(html), "text/html");
        });
        group.MapGet("/logout", (HttpContext context) =>
        {
            // 登出，清除 Session
            context.Session.Remove("APITESTER_ISLOGIN");
            context.Response.Cookies.Delete("APITESTER_ISLOGIN");
            isLoginJump(context);
            return Results.Text("");
        });
        group.MapGet("/login", (HttpContext context) =>
        {
            // my.base_dir() +
            string op = Path.Combine(my.base_dir(), "App", "myApiTester", "html", "login.html");
            //return Results.Text(op);
            string html = my.b2s(my.file_get_contents(op));
            // 一次取代二個
            html = html.Replace("{{BASE_URL}}", my.base_url(context));

            return Results.Bytes(my.s2b(html), "text/html");
            //my.echoBinary(context, my.s2b(html));
        });


        var MG = app.MapGroup("/MYAPI");
        MG.MapMethods("/", new[] { "GET", "POST" }, (HttpContext context) =>
        {
            string GETS_STRING = "mode";
            var GETS = my.getGET_POST(context, GETS_STRING, "GET");
            switch (GETS["mode"].ToString())
            {
                case "add_GET":
                    GETS_STRING = "A,B";
                    GETS = my.getGET_POST(context, GETS_STRING, "GET");
                    double A = Convert.ToDouble(GETS["A"].ToString());
                    double B = Convert.ToDouble(GETS["B"].ToString());
                    return Results.Text((A + B).ToString());
                case "add_POST":
                    string POSTS_STRING = "A,B";
                    var POSTS = my.getGET_POST(context, POSTS_STRING, "POST");
                    A = Convert.ToDouble(POSTS["A"].ToString());
                    B = Convert.ToDouble(POSTS["B"].ToString());
                    return Results.Text((A + B).ToString());
                case "add_GET_POST":
                    GETS_STRING = "A,B";
                    GETS = my.getGET_POST(context, GETS_STRING, "GET");
                    A = Convert.ToDouble(GETS["A"].ToString());
                    B = Convert.ToDouble(GETS["B"].ToString());
                    POSTS_STRING = "AA,BB";
                    POSTS = my.getGET_POST(context, POSTS_STRING, "POST");
                    double AA = Convert.ToDouble(POSTS["AA"].ToString());
                    double BB = Convert.ToDouble(POSTS["BB"].ToString());
                    string MESSAGE = "A+B=" + (A + B).ToString() + "<br>";
                    MESSAGE += "AA+BB=" + (AA + BB).ToString();
                    return Results.File(my.s2b(MESSAGE), "text/html");
                case "mul_GET":
                    GETS_STRING = "A,B";
                    GETS = my.getGET_POST(context, GETS_STRING, "GET");
                    A = Convert.ToDouble(GETS["A"].ToString());
                    B = Convert.ToDouble(GETS["B"].ToString());
                    return Results.Text((A * B).ToString());
                case "datetime":
                    my.allowAjax(context);
                    return Results.Text(my.date("Y-m-d H:i:s"));
                case "datetime_login":
                    isLoginJump(context); // 需登入才能使用，不是的話跳回登入頁面
                    my.allowAjax(context);
                    return Results.Text(my.date("Y-m-d H:i:s"));
                default:
                    my.exit(context);
                    return Results.Text("");
            }
        });
    }
}