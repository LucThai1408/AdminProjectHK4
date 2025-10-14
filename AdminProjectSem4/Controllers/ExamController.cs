using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Policy;
using System.Xml.Linq;

//namespace AdminProjectSem4.Controllers
//{
//    public class ExamController : Controller
//    {
//        string uri = "https://localhost:44341/api/admin/";
//        HttpClient client = new HttpClient();

        // GET: ExamController1
        public bool fail { get; private set; }
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;

            // Gọi API exams
            string endpoint = string.IsNullOrEmpty(name)
                ? $"Exams/?currentPage={currentPage}"
                : $"Exams/?name={name}";
            var json = await client.GetStringAsync(endpoint);
            var data = JArray.Parse(json);

            // Gọi thêm API accounts và rooms
            var accountsJson = await client.GetStringAsync("Accounts");
            var roomsJson = await client.GetStringAsync("Rooms");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsJson) ?? new();
            var rooms = JsonConvert.DeserializeObject<List<Room>>(roomsJson) ?? new();

            // Parse dữ liệu Exam
            var exams = data.Select(x => new Exam
            {
                ExamId = (int)x["examId"],
                Name = (string)x["name"],
                AccountId = (int)x["accountId"],
                RoomId = (int)x["roomId"],
                ExamDay = DateTime.TryParse((string?)x["examDayString"], out var day) ? day : DateTime.MinValue,
                ExamTime = TimeSpan.TryParse((string?)x["examTimeString"], out var time) ? time : TimeSpan.Zero,
                Status = (bool)(x["status"] ?? true),
                Fee = (float)(x["fee"] ?? 0),
                CreatedAt = DateTime.TryParse((string?)x["createdAt"], out var created) ? created : DateTime.Now,
                Account = accounts.FirstOrDefault(a => a.AccountId == (int)x["accountId"]),
                Room = rooms.FirstOrDefault(r => r.RoomId == (int)x["roomId"])
            }).ToList();

           

            // Phân trang
            int totalPage = (int)Math.Ceiling((double)exams.Count / pageSize);
            var pagedData = exams.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }


        // GET: ExamController1/Details/5

//        public async Task<ActionResult> Detail(int id)
//        {
//            client.BaseAddress = new Uri(uri);

            var exam = JsonConvert.DeserializeObject<Exam>(await client.GetStringAsync("Exams/" + id));
            var accounts = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("Accounts"));
            var rooms = JsonConvert.DeserializeObject<List<Room>>(
                await client.GetStringAsync("Rooms"));

            ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
            ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//            return View(exam);
//        }


//        // GET: ExamController1/Create
//        public async Task<ActionResult> Create()
//        {
//            client.BaseAddress = new Uri(uri);

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();

            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("Subjects"));
            var rooms = JsonConvert.DeserializeObject<List<Room>>(
                await client.GetStringAsync("Rooms"));

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Room = new SelectList(rooms, "RoomId", "Name");
            ViewBag.Account = new SelectList(students, "AccountId", "FullName");
            return View();
        }

//        // POST: ExamController1/Create
//        [ValidateAntiForgeryToken]
//        [HttpPost]
//        public async Task<ActionResult> Create(Exam exam)
//        {
//            bool isValid = true;

            if (string.IsNullOrWhiteSpace(exam.Name))
            {
                ViewBag.ErrorName = "Exam name cannot be empty!";
                isValid = false;
            }
            if (exam.RoomId <= 0)
            {
                ViewBag.ErrorRoom = "Room is required!";
                isValid = false;
            }
            if (exam.AccountId <= 0)
            {
                ViewBag.ErrorAccount = "Account is required!";
                isValid = false;
            }
            if (exam.ExamDay == default)
            {
                ViewBag.ErrorDay = "Exam day is required!";
                isValid = false;
            }
            if (exam.ExamTime == default)
            {
                ViewBag.ErrorTime = "Exam time is required!";
                isValid = false;
            }
            if (exam.Fee < 0)
            {
                ViewBag.ErrorFee = "Fee must be >= 0!";
                isValid = false;
            }

            if (!isValid)
            {
                client.BaseAddress = new Uri(uri);

                var allStudent = JsonConvert.DeserializeObject<List<Account>>(
              await client.GetStringAsync("Accounts")) ?? new List<Account>();
                var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();
                var rooms = JsonConvert.DeserializeObject<List<Room>>(
                    await client.GetStringAsync("Rooms"));

                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
                ViewBag.Account = new SelectList(students, "AccountId", "FullName", exam.AccountId);

//                return View(exam);
//            }

            try
            {
                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        name = exam.Name,
                        roomId = exam.RoomId,
                        accountId = exam.AccountId,
                        examDay = exam.ExamDay.ToString("yyyy-MM-ddTHH:mm:ss"),
                        examTime = exam.ExamTime.ToString(@"hh\:mm\:ss"),
                        status = exam.Status,
                        fee = exam.Fee
                    });

//                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//                    var response = await client.PostAsync($"{uri}Exams", content);

//                    if (!response.IsSuccessStatusCode)
//                    {
//                        TempData["msg"] = "Create exam failed: " + await response.Content.ReadAsStringAsync();

                        // Khi API lỗi → load lại dropdown để form không trống
                        var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                        await client.GetStringAsync("Accounts")) ?? new List<Account>();
                        var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();
                        var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                            await this.client.GetStringAsync("Subjects"));
                        var rooms = JsonConvert.DeserializeObject<List<Room>>(
                            await this.client.GetStringAsync("Rooms"));
                        ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
                        ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
                        ViewBag.Account = new SelectList(students, "AccountId", "FullName", exam.AccountId);

//                        return View(exam);
//                    }
//                }
//                TempData["msg"] = "Exam created successfully!";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                TempData["msg"] = "Error: " + ex.Message;

                var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                 await client.GetStringAsync("Accounts")) ?? new List<Account>();
                var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();
                var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                    await client.GetStringAsync("Subjects"));
                var rooms = JsonConvert.DeserializeObject<List<Room>>(
                    await client.GetStringAsync("Rooms"));

                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
                ViewBag.Account = new SelectList(students, "AccountId", "FullName", exam.AccountId);

//                return View(exam);
//            }
//        }




//        // GET: ExamController1/Edit/5
//        public async Task<ActionResult> Edit(int id)
//        {
//            client.BaseAddress = new Uri(uri);

//            var exam = JsonConvert.DeserializeObject<Exam>(await client.GetStringAsync("Exams/" + id));

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                 await client.GetStringAsync("Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("Subjects"));

            var rooms = JsonConvert.DeserializeObject<List<Room>>(
                await client.GetStringAsync("Rooms"));
            ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
            ViewBag.Account = new SelectList(students, "AccountId", "FullName", exam.AccountId);

//            return View(exam);
//        }


//        // POST: ExamController1/Edit/5
//        [HttpPost]
//        public async Task<ActionResult> Edit(int id, Exam exam)
//        {
//            if (id != exam.ExamId)
//            {
//                TempData["msg"] = "Exam ID mismatch!";
//                return View(exam);
//            }

            // validate cơ bản
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(exam.Name))
            {
                ViewBag.ErrorName = "Exam name cannot be empty!";
                isValid = false;
            }
            if (exam.RoomId <= 0) { ViewBag.ErrorRoom = "Room is required!"; isValid = false; }
            if (exam.AccountId <= 0) { ViewBag.ErrorAccount = "Account is required!"; isValid = false; }
            if (exam.ExamDay == default) { ViewBag.ErrorDay = "Exam day is required!"; isValid = false; }
            if (exam.ExamTime == default) { ViewBag.ErrorTime = "Exam time is required!"; isValid = false; }
            if (exam.Fee < 0) { ViewBag.ErrorFee = "Fee must be >= 0!"; isValid = false; }

//            if (!isValid)
//            {
//                client.BaseAddress = new Uri(uri);

                var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                 await client.GetStringAsync("Accounts")) ?? new List<Account>();
                var students = allStudent.Where(c => c.Status == true && c.Role == 2).ToList();
                var rooms = JsonConvert.DeserializeObject<List<Room>>(
                    await client.GetStringAsync("Rooms"));

                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
                ViewBag.Account = new SelectList(students, "AccountId", "FullName", exam.AccountId);

                return View(exam);
            }
            try
            {
                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        examId = exam.ExamId,   // bắt buộc để API map đúng record
                        name = exam.Name,
                        roomId = exam.RoomId,
                        accountId = exam.AccountId,
                        examDay = exam.ExamDay.ToString("yyyy-MM-ddTHH:mm:ss"),
                        examTime = exam.ExamTime.ToString(@"hh\:mm\:ss"),
                        status = exam.Status,
                        fee = exam.Fee
                    });

//                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//                    // Gọi PUT API
//                    var response = await client.PutAsync($"{uri}Exams/{id}", content);

//                    if (!response.IsSuccessStatusCode)
//                    {
//                        TempData["msg"] = "Update exam failed: " + await response.Content.ReadAsStringAsync();
//                        return View(exam);
//                    }
//                }

//                TempData["msg"] = "Exam updated successfully!";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                TempData["msg"] = "Error: " + ex.Message;
//                return View(exam);
//            }
//        }


        // GET: ExamController1/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);
            var exam = JsonConvert.DeserializeObject<Exam>(
                await client.GetStringAsync($"Exams/{id}")
            );

            if (exam == null)
            {
                TempData["msg"] = "Exam not found!";
                return RedirectToAction("Index");
            }
            exam.Status = fail;

            var response = await client.PutAsJsonAsync($"Exams/{id}", exam);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "Exam ubject has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling exam!";
            }

            return RedirectToAction("Index");
        }
    }
}
