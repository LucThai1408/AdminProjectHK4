using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    public class CourseController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        public bool fail { get; private set; }
        // GET: CourseController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;
            var all = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));

            if (!string.IsNullOrEmpty(name))
            {
                all = all
                    .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalPage = (int)Math.Ceiling((double)all.Count / pageSize);

            var courses = all
                .OrderByDescending(c => c.CourseId)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(courses);
        }

        // GET: CourseController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            client.BaseAddress = new Uri(uri + id);
            var course = JsonConvert.DeserializeObject<Course>(await client.GetStringAsync("Courses/" + id));
            return View(course);
        }

        // GET: CourseController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CourseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Course course)
        {
            client.BaseAddress = new Uri(uri);
            var rooms = JsonConvert.DeserializeObject<List<Course>>(await client.GetStringAsync("Courses"));
            if (course.Name == null || course.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Course name can not empty!";
                return View(course);
            }
            var response = await client.PostAsJsonAsync("Courses", new { Name = course.Name, Status = course.Status });
            TempData["msg"] = "Course created successfully!";
            return RedirectToAction("Index");

        }

        // GET: CourseController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);
            var course = JsonConvert.DeserializeObject<Course>(await client.GetStringAsync("Courses/" + id));
            return View(course);
        }

        // POST: CourseController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Course course)
        {
            client.BaseAddress = new Uri(uri);
            var courses = JsonConvert.DeserializeObject<List<Course>>(await client.GetStringAsync("Courses"));
            if (course.Name == null || course.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Room name can not empty!";
                return View(course);
            }
            var response = await client.PutAsJsonAsync("Courses/" + id, course);
            TempData["msg"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");
        }


        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);

            var room = JsonConvert.DeserializeObject<Course>(
                await client.GetStringAsync($"Courses/{id}")
            );

            if (room == null)
            {
                TempData["msg"] = "Course not found!";
                return RedirectToAction("Index");
            }
            room.Status = fail;

            var response = await client.PutAsJsonAsync($"Courses/{id}", room);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "Course has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling room!";
            }

            return RedirectToAction("Index");
        }
    }
}
