using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    public class CourseStudentController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        public bool fail { get; private set; }
        // GET: CourseStudentController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;

            var CourseStudentJson = await client.GetStringAsync("CourseStudents");
            var CourseStudents = JsonConvert.DeserializeObject<List<CourseStudent>>(CourseStudentJson) ?? new List<CourseStudent>();

            var accountJson = await client.GetStringAsync("admin/Accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountJson) ?? new List<Account>();

            var coursesJson = await client.GetStringAsync("Courses");
            var courses = JsonConvert.DeserializeObject<List<Course>>(coursesJson) ?? new List<Course>();

            foreach (var cs in CourseStudents)
            {
                cs.Student = accounts.FirstOrDefault(s => s.AccountId == cs.StudentId);
                cs.Course = courses.FirstOrDefault(c => c.CourseId == cs.CourseId);
            }
            CourseStudents = CourseStudents
                .Where(cs => cs.Course != null && cs.Student != null
                          && cs.Course.Status == true
                          && cs.Student.Status == true)
                .ToList();
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                CourseStudents = CourseStudents
                    .Where(cs =>
                        (cs.Student.FullName != null && cs.Student.FullName.ToLower().Contains(name)) ||
                        (cs.Course.Name != null && cs.Course.Name.ToLower().Contains(name)))
                    .ToList();
            }
            var totalPage = (int)Math.Ceiling((double)CourseStudents.Count / pageSize);
            var pagedData = CourseStudents
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }


        // GET: CourseStudentController/Details/5
        public async Task<ActionResult> Detail(int id)
        {
            client.BaseAddress = new Uri(uri);

            var courseStudent = JsonConvert.DeserializeObject<CourseStudent>(
                await client.GetStringAsync("CourseStudents/" + id));

            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));

            var students = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts"));

            courseStudent.Course = courses.FirstOrDefault(c => c.CourseId == courseStudent.CourseId);
            courseStudent.Student = students.FirstOrDefault(s => s.AccountId == courseStudent.StudentId);

            ViewBag.Student = new SelectList(students, "AccountId", "FullName", courseStudent.StudentId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseStudent.CourseId);

            return View(courseStudent);
        }


        // GET: CourseStudentController/Create
        public async Task<ActionResult> Create()
        {
            client.BaseAddress = new Uri(uri);

            var allCourses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses")) ?? new List<Course>();
            var courses = allCourses.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Student = new SelectList(students, "AccountId", "FullName");
            ViewBag.Course = new SelectList(courses, "CourseId", "Name");

            return View();
        }


        // POST: CourseStudentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CourseStudent courseStudent)
        {
            try
            {
                client.BaseAddress = new Uri(uri);
                var response = await client.PostAsJsonAsync("CourseStudents", courseStudent);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể thêm CourseStudent. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi gửi dữ liệu: " + ex.Message;
            }

            var allCourses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses")) ?? new List<Course>();
            var courses = allCourses.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(students, "AccountId", "FullName", courseStudent.StudentId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseStudent.CourseId);

            return View(courseStudent);
        }

        // GET: CourseStudentController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);

            var courseStudent = JsonConvert.DeserializeObject<CourseStudent>(await client.GetStringAsync("CourseStudents/" + id));

            var allCourses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses")) ?? new List<Course>();
            var courses = allCourses.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Student = new SelectList(students, "AccountId", "FullName", courseStudent.StudentId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseStudent.CourseId);

            return View(courseStudent);
        }

        // POST: CourseStudentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CourseStudent courseStudent)
        {
            try
            {
                client.BaseAddress = new Uri(uri);

                var response = await client.PutAsJsonAsync($"CourseStudents/{id}", courseStudent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể cập nhật courseStudent. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi cập nhật dữ liệu: " + ex.Message;
            }

            // 🔹 Nếu lỗi, load lại danh sách để dropdown không bị rỗng
            var allCourses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));
            var courses = allCourses.Where(c => c.Status == true).ToList();
            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Student = new SelectList(students, "AccountId", "FullName", courseStudent.StudentId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseStudent.CourseId);

            return View(courseStudent);
        }

       

        // POST: CourseStudentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);
            var courseStudent = JsonConvert.DeserializeObject<CourseStudent>(
                await client.GetStringAsync($"CourseStudents/{id}")
            );

            if (courseStudent == null)
            {
                TempData["msg"] = "Course Subject not found!";
                return RedirectToAction("Index");
            }
            courseStudent.Status = fail;

            var response = await client.PutAsJsonAsync($"CourseStudents/{id}", courseStudent);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "CourseStudent ubject has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling courseStudent!";
            }

            return RedirectToAction("Index");
        }
    }
}
