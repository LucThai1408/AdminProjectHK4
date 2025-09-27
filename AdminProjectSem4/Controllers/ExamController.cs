using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Security.Policy;
using System.Xml.Linq;

namespace AdminProjectSem4.Controllers
{
    public class ExamController : Controller
    {
        string uri = "https://localhost:44341/api/admin/";
        HttpClient client = new HttpClient();

        // GET: ExamController1
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;
            var ExamAll = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams"));
            var Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?currentPage=" + currentPage));
            if (!name.Equals(""))
            {
                Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?name=" + name));
                ExamAll = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?name=" + name));
            }
            else
            {
                Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?currentPage=" + currentPage));
            }
            ViewBag.TotalPage = ExamAll.Count() % pageSize == 0 ? ExamAll.Count() / pageSize : ExamAll.Count() / pageSize + 1;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;
            return View(Exams);
        }

        // GET: ExamController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExamController1/Create
        public async Task<ActionResult> Create()
        {
            client.BaseAddress = new Uri(uri);

            var accounts = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("Accounts"));
            Console.WriteLine(accounts);
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("Subjects"));
            var rooms = JsonConvert.DeserializeObject<List<Room>>(
                await client.GetStringAsync("Rooms"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Room = new SelectList(rooms, "RoomId", "Name");
            ViewBag.Account = new SelectList(accounts, "AccountId", "FullName");
            return View();
        }

        // POST: ExamController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    client.BaseAddress = new Uri(uri);

            
        //}

        [HttpPost]
        public async Task<ActionResult> Create(Exam exam)
        {
            client.BaseAddress = new Uri(uri);
            var exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams"));
            // 1. Validate cơ bản
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(exam.Name))
            {
                ViewBag.ErrorName = "Exam name cannot be empty!";
                isValid = false;
            }
            if (exam.SubjectId <= 0) { ViewBag.ErrorSubject = "Subject is required!"; isValid = false; }
            if (exam.RoomId <= 0) { ViewBag.ErrorRoom = "Room is required!"; isValid = false; }
            if (exam.AccountId <= 0) { ViewBag.ErrorAccount = "Account is required!"; isValid = false; }
            if (exam.ExamDay == default) { ViewBag.ErrorDay = "Exam day is required!"; isValid = false; }
            if (exam.ExamTime == default) { ViewBag.ErrorTime = "Exam time is required!"; isValid = false; }
            if (exam.Fee < 0) { ViewBag.ErrorFee = "Fee must be >= 0!"; isValid = false; }

            if (!isValid)
                return View(exam);

            try
            {
                // 2. Chuẩn bị dữ liệu gửi lên API
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(exam.Name.Trim()), "name");
                formData.Add(new StringContent(exam.SubjectId.ToString()), "subjectId");
                formData.Add(new StringContent(exam.RoomId.ToString()), "roomId");
                formData.Add(new StringContent(exam.AccountId.ToString()), "accountId");
                formData.Add(new StringContent(exam.ExamDay.ToString("yyyy-MM-dd")), "examDay");
                formData.Add(new StringContent(exam.ExamTime.ToString(@"hh\:mm")), "examTime");
                formData.Add(new StringContent(exam.Status.ToString()), "status");
                formData.Add(new StringContent(exam.Fee.ToString()), "fee");

                // 3. Gửi request
                var response = await client.PostAsync("exams", formData);

                //if (!response.IsSuccessStatusCode)
                //{
                //    TempData["msg"] = "Create exam failed: " + await response.Content.ReadAsStringAsync();
                //    return View(exam);
                //}

                TempData["msg"] = "Exam created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return View(exam);
            }
        }


        // GET: ExamController1/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExamController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ExamController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExamController1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
