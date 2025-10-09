using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;

namespace AdminProjectSem4.Controllers
{
    public class CourseSubjectController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        public bool fail { get; private set; }
        // GET: CourseSubjectController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;

            var courseSubjectJson = await client.GetStringAsync("CourseSubjects");
            var courseSubjects = JsonConvert.DeserializeObject<List<CourseSubject>>(courseSubjectJson) ?? new List<CourseSubject>();
            var subjectsJson = await client.GetStringAsync("admin/Subjects");
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson) ?? new List<Subject>();
            var coursesJson = await client.GetStringAsync("Courses");
            var courses = JsonConvert.DeserializeObject<List<Course>>(coursesJson) ?? new List<Course>();

            foreach (var cs in courseSubjects)
            {
                cs.Subject = subjects.FirstOrDefault(s => s.SubjectId == cs.SubjectId);
                cs.Course = courses.FirstOrDefault(c => c.CourseId == cs.CourseId);
            }

            var totalPage = (int)Math.Ceiling((double)courseSubjects.Count / pageSize);
            var pagedData = courseSubjects
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }



        // GET: CourseSubjectController/Details/5
        public async Task<ActionResult> Detail(int id)
        {
            client.BaseAddress = new Uri(uri);

            var courseSubject = JsonConvert.DeserializeObject<CourseSubject>(await client.GetStringAsync("CourseSubjects/" + id));

            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));

            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", courseSubject.SubjectId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseSubject.CourseId);

            return View(courseSubject);
        }

        // GET: CourseSubjectController/Create
        public async Task<ActionResult> Create()
        {
            client.BaseAddress = new Uri(uri);

            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));
            Console.WriteLine(courses);
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Course = new SelectList(courses, "CourseId", "Name");
            return View();
        }

        // POST: CourseSubjectController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CourseSubject courseSubject)
        {
            try
            {
                client.BaseAddress = new Uri(uri);
                var response = await client.PostAsJsonAsync("CourseSubjects", courseSubject);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể thêm CourseSubject. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi gửi dữ liệu: " + ex.Message;
            }

            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", courseSubject.SubjectId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseSubject.CourseId);

            return View(courseSubject);
        }


        // GET: CourseSubjectController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);

            var courseSubject = JsonConvert.DeserializeObject<CourseSubject>(await client.GetStringAsync("CourseSubjects/" + id));

            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));

            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", courseSubject.SubjectId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseSubject.CourseId);

            return View(courseSubject);
        }


        // POST: CourseSubjectController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CourseSubject courseSubject)
        {
            try
            {
                client.BaseAddress = new Uri(uri);

                var response = await client.PutAsJsonAsync($"CourseSubjects/{id}", courseSubject);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể cập nhật CourseSubject. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi cập nhật dữ liệu: " + ex.Message;
            }

            // 🔹 Nếu lỗi, load lại danh sách để dropdown không bị rỗng
            var courses = JsonConvert.DeserializeObject<List<Course>>(
                await client.GetStringAsync("Courses"));
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", courseSubject.SubjectId);
            ViewBag.Course = new SelectList(courses, "CourseId", "Name", courseSubject.CourseId);

            return View(courseSubject);
        }



       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);

            // Lấy room theo id
            var room = JsonConvert.DeserializeObject<CourseSubject>(
                await client.GetStringAsync($"CourseSubjects/{id}")
            );

            if (room == null)
            {
                TempData["msg"] = "Course Subject not found!";
                return RedirectToAction("Index");
            }
            room.Status = fail;

            var response = await client.PutAsJsonAsync($"CourseSubjects/{id}", room);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "CourseS ubject has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling room!";
            }

            return RedirectToAction("Index");
        }
    }
}
