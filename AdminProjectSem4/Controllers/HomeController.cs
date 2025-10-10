using AdminProjectSem4.Models;
using API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using WebEcom.Models;

namespace AdminProjectSem4.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        string url = "https://localhost:44341/api/admin/";
        HttpClient client = new HttpClient();

        public HomeController()
        {
            client.BaseAddress = new Uri(url);
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email = "", string password = "")
        {
            // Check input rỗng
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.error = "<p class='alert alert-danger'>Email or password cannot be empty!</p>";
                return View();
            }

            // Mã hóa mật khẩu
            string passMD5 = Cipher.GenerateMD5(password);

            // Lấy tất cả account từ API
            var accounts = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("Accounts")
            );

            // Tìm account hợp lệ
            var acc = accounts?
                .FirstOrDefault(account =>
                    account.Email == email &&
                    account.Password == passMD5 &&
                    (account.Role == 1 || account.Role == 2) &&
                    account.Status == true
                );

            // Nếu không tìm thấy -> báo lỗi
            if (acc == null)
            {
                ViewBag.error = "<p class='alert alert-danger'>Email or password is incorrect!</p>";
                ViewBag.Email = email;
                return View();
            }

            // Nếu login thành công -> tạo Claims
            var identity = new ClaimsIdentity(
                new[]
                {
            new Claim(ClaimTypes.NameIdentifier, acc.AccountId.ToString()),
            new Claim(ClaimTypes.Name, acc.Name ?? ""),
            new Claim("fullName", acc.FullName ?? ""),
            new Claim("roomId", acc.RoomId.ToString()),
            new Claim(ClaimTypes.Email, acc.Email ?? ""),
            new Claim("phone", acc.Phone ?? ""),
            new Claim(ClaimTypes.Role, acc.Role.ToString()),
            new Claim("image", acc.Image ?? ""),
            new Claim("status", acc.Status.ToString())
                }, "RESTINASecurityScheme");

            var principal = new ClaimsPrincipal(identity);

            // nhớ await ở đây
            await HttpContext.SignInAsync("RESTINASecurityScheme", principal);

            return RedirectToAction("Index");
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("RESTINASecurityScheme");
            return RedirectToAction("login");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
