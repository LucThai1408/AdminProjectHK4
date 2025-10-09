using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace AdminProjectSem4.Controllers
{
    public class SubjectController : Controller
    {
        string uri = "https://localhost:44341/api/admin/";
        HttpClient client = new HttpClient();
        public bool fail { get; private set; }
        // GET: SubjectController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;
            var all = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("Subjects"));

            if (!string.IsNullOrEmpty(name))
            {
                all = all
                    .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalPage = (int)Math.Ceiling((double)all.Count / pageSize);

            var subject = all
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(subject);
        }

        // GET: SubjectController/Details/5
        public async Task<ActionResult> Detail(int id)
        {
            client.BaseAddress = new Uri(uri + id);
            var subject = JsonConvert.DeserializeObject<Subject>(await client.GetStringAsync("Subjects/" + id));
            return View(subject);
        }

        // GET: SubjectController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SubjectController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Subject subject)
        {
            client.BaseAddress = new Uri(uri);
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(await client.GetStringAsync("Subjects"));
            if (subject.Name == null || subject.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Subject name can not empty!";
                return View(subject);
            }
            if (subject != null)
            {
                foreach (var item in subjects)
                {
                    if (item.Name.ToLower().Equals(subject.Name.Trim().ToLower()))
                    {
                        ViewBag.ErrorName = "Subject name is already exist!";
                        return View(subject);
                    }
                }
            }
            var response = await client.PostAsJsonAsync("Subjects", new { Name = subject.Name, MaxScore = subject.MaxScore, PassScore = subject.PassScore, Status = subject.Status });
            TempData["msg"] = "Subject created successfully!";
            return RedirectToAction("Index");

        }

        // GET: SubjectController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);
            var subject = JsonConvert.DeserializeObject<Subject>(await client.GetStringAsync("Subjects/" + id));
            return View(subject);
        }

        // POST: SubjectController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Subject subject)
        {
            client.BaseAddress = new Uri(uri);
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(await client.GetStringAsync("Subjects"));
            if (subject.Name == null || subject.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Subject name can not empty!";
                return View(subject);
            }
            if (subject != null)
            {
                foreach (var item in subjects)
                {
                    if (item.Name.ToLower().Equals(subject.Name.Trim().ToLower()) && item.SubjectId != subject.SubjectId)
                    {
                        ViewBag.ErrorName = "Subject name is already exist!";
                        return View(subject);
                    }
                }
            }
            var response = await client.PutAsJsonAsync("Subjects/" + id, subject);
            TempData["msg"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");
        }

        // GET: SubjectController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);

            // Lấy room theo id
            var subject = JsonConvert.DeserializeObject<Subject>(
                await client.GetStringAsync($"Subjects/{id}")
            );

            if (subject == null)
            {
                TempData["msg"] = "Subject not found!";
                return RedirectToAction("Index");
            }
            subject.Status = fail;

            var response = await client.PutAsJsonAsync($"Subjects/{id}", subject);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "Subject has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling room!";
            }

            return RedirectToAction("Index");
        }
    }
}
