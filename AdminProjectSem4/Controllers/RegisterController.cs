using AdminProjectSem4.Models;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    public class RegisterController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 5;

            // Lấy dữ liệu
            var registerJson = await client.GetStringAsync("admin/Registers");
            var registers = JsonConvert.DeserializeObject<List<RegisterDto>>(registerJson) ?? new List<RegisterDto>();
            if (!string.IsNullOrEmpty(name))
            {
                registers = registers
                    .Where(ac =>
                        (!string.IsNullOrEmpty(ac.ExamName) && ac.ExamName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||
                        (ac.StudentName != null && !string.IsNullOrEmpty(ac.StudentName) &&
                         ac.StudentName.Contains(name, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            var totalPage = (int)Math.Ceiling((double)registers.Count / pageSize);
            var pagedData = registers
                .OrderByDescending(r => r.RegisterId)
                .OrderByDescending(r => r.RegisterId)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }

        public async Task<ActionResult> Details(int id)
        {
            client.BaseAddress = new Uri(uri);

            var courseStudent = JsonConvert.DeserializeObject<RegisterDto>(
                await client.GetStringAsync("admin/Registers/" + id));

            return View(courseStudent);
        }
    }
}
