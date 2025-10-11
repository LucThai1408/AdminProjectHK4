//using API.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Newtonsoft.Json;
//using System;
//using System.Security.Policy;
//using System.Xml.Linq;

//namespace AdminProjectSem4.Controllers
//{
//    public class ExamController : Controller
//    {
//        string uri = "https://localhost:44341/api/admin/";
//        HttpClient client = new HttpClient();

//        // GET: ExamController1
//        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
//        {
//            client.BaseAddress = new Uri(uri);
//            int pageSize = 6;
//            var ExamAll = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams"));
//            var Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?currentPage=" + currentPage));
//            if (!name.Equals(""))
//            {
//                Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?name=" + name));
//                ExamAll = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?name=" + name));
//            }
//            else
//            {
//                Exams = JsonConvert.DeserializeObject<List<Exam>>(await client.GetStringAsync("Exams/?currentPage=" + currentPage));
//            }
//            ViewBag.TotalPage = ExamAll.Count() % pageSize == 0 ? ExamAll.Count() / pageSize : ExamAll.Count() / pageSize + 1;
//            ViewBag.CurrentPage = currentPage;
//            ViewBag.Name = name;
//            return View(Exams);
//        }

//        // GET: ExamController1/Details/5

//        public async Task<ActionResult> Detail(int id)
//        {
//            client.BaseAddress = new Uri(uri);

//            var exam = JsonConvert.DeserializeObject<Exam>(await client.GetStringAsync("Exams/" + id));
//            var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                await client.GetStringAsync("Accounts"));
//            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                await client.GetStringAsync("Subjects"));
//            var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                await client.GetStringAsync("Rooms"));

//            // gán vào ViewBag, có chọn sẵn giá trị cũ
//            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubject);
//            ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//            ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//            return View(exam);
//        }


//        // GET: ExamController1/Create
//        public async Task<ActionResult> Create()
//        {
//            client.BaseAddress = new Uri(uri);

//            var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                await client.GetStringAsync("Accounts"));
//            Console.WriteLine(accounts);
//            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                await client.GetStringAsync("Subjects"));
//            var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                await client.GetStringAsync("Rooms"));

//            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
//            ViewBag.Room = new SelectList(rooms, "RoomId", "Name");
//            ViewBag.Account = new SelectList(accounts, "AccountId", "FullName");
//            return View();
//        }

//        // POST: ExamController1/Create
//        [ValidateAntiForgeryToken]
//        [HttpPost]
//        public async Task<ActionResult> Create(Exam exam)
//        {
//            bool isValid = true;

//            if (string.IsNullOrWhiteSpace(exam.Name))
//            {
//                ViewBag.ErrorName = "Exam name cannot be empty!";
//                isValid = false;
//            }
//            if (exam.CourseSubjectId <= 0)
//            {
//                ViewBag.ErrorSubject = "Subject is required!";
//                isValid = false;
//            }
//            if (exam.RoomId <= 0)
//            {
//                ViewBag.ErrorRoom = "Room is required!";
//                isValid = false;
//            }
//            if (exam.AccountId <= 0)
//            {
//                ViewBag.ErrorAccount = "Account is required!";
//                isValid = false;
//            }
//            if (exam.ExamDay == default)
//            {
//                ViewBag.ErrorDay = "Exam day is required!";
//                isValid = false;
//            }
//            if (exam.ExamTime == default)
//            {
//                ViewBag.ErrorTime = "Exam time is required!";
//                isValid = false;
//            }
//            if (exam.Fee < 0)
//            {
//                ViewBag.ErrorFee = "Fee must be >= 0!";
//                isValid = false;
//            }

//            // Nếu dữ liệu không hợp lệ → load lại dropdown và trả về View
//            if (!isValid)
//            {
//                client.BaseAddress = new Uri(uri);

//                var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                    await client.GetStringAsync("Accounts"));
//                var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                    await client.GetStringAsync("Subjects"));
//                var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                    await client.GetStringAsync("Rooms"));

//                ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubjectId);
//                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//                ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//                return View(exam);
//            }

//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    var json = JsonConvert.SerializeObject(new
//                    {
//                        name = exam.Name,
//                        subjectId = exam.CourseSubjectId,
//                        roomId = exam.RoomId,
//                        accountId = exam.AccountId,
//                        examDay = exam.ExamDay.ToString("yyyy-MM-ddTHH:mm:ss"),
//                        examTime = exam.ExamTime.ToString(@"hh\:mm\:ss"),
//                        status = exam.Status,
//                        fee = exam.Fee
//                    });

//                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

//                    var response = await client.PostAsync($"{uri}Exams", content);

//                    if (!response.IsSuccessStatusCode)
//                    {
//                        TempData["msg"] = "Create exam failed: " + await response.Content.ReadAsStringAsync();

//                        // Khi API lỗi → load lại dropdown để form không trống
//                        var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                            await this.client.GetStringAsync("Accounts"));
//                        var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                            await this.client.GetStringAsync("Subjects"));
//                        var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                            await this.client.GetStringAsync("Rooms"));

//                        ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubjectId);
//                        ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//                        ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//                        return View(exam);
//                    }
//                }
//                TempData["msg"] = "Exam created successfully!";
//                return RedirectToAction("Index");
//            }
//            catch (Exception ex)
//            {
//                TempData["msg"] = "Error: " + ex.Message;

//                var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                    await client.GetStringAsync("Accounts"));
//                var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                    await client.GetStringAsync("Subjects"));
//                var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                    await client.GetStringAsync("Rooms"));

//                ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubjectId);
//                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//                ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//                return View(exam);
//            }
//        }




//        // GET: ExamController1/Edit/5
//        public async Task<ActionResult> Edit(int id)
//        {
//            client.BaseAddress = new Uri(uri);

//            var exam = JsonConvert.DeserializeObject<Exam>(await client.GetStringAsync("Exams/" + id));

//            var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                await client.GetStringAsync("Accounts"));

//            var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                await client.GetStringAsync("Subjects"));

//            var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                await client.GetStringAsync("Rooms"));
//            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubjectId);
//            ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//            ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

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

//            // validate cơ bản
//            bool isValid = true;
//            if (string.IsNullOrWhiteSpace(exam.Name))
//            {
//                ViewBag.ErrorName = "Exam name cannot be empty!";
//                isValid = false;
//            }
//            if (exam.CourseSubjectId <= 0) { ViewBag.ErrorSubject = "Subject is required!"; isValid = false; }
//            if (exam.RoomId <= 0) { ViewBag.ErrorRoom = "Room is required!"; isValid = false; }
//            if (exam.AccountId <= 0) { ViewBag.ErrorAccount = "Account is required!"; isValid = false; }
//            if (exam.ExamDay == default) { ViewBag.ErrorDay = "Exam day is required!"; isValid = false; }
//            if (exam.ExamTime == default) { ViewBag.ErrorTime = "Exam time is required!"; isValid = false; }
//            if (exam.Fee < 0) { ViewBag.ErrorFee = "Fee must be >= 0!"; isValid = false; }

//            if (!isValid)
//            {
//                client.BaseAddress = new Uri(uri);

//                var accounts = JsonConvert.DeserializeObject<List<Account>>(
//                    await client.GetStringAsync("Accounts"));
//                var subjects = JsonConvert.DeserializeObject<List<Subject>>(
//                    await client.GetStringAsync("Subjects"));
//                var rooms = JsonConvert.DeserializeObject<List<Room>>(
//                    await client.GetStringAsync("Rooms"));

//                ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name", exam.CourseSubjectId);
//                ViewBag.Room = new SelectList(rooms, "RoomId", "Name", exam.RoomId);
//                ViewBag.Account = new SelectList(accounts, "AccountId", "FullName", exam.AccountId);

//                return View(exam);
//            }
//            try
//            {
//                using (var client = new HttpClient())
//                {
//                    var json = JsonConvert.SerializeObject(new
//                    {
//                        examId = exam.ExamId,   // bắt buộc để API map đúng record
//                        name = exam.Name,
//                        subjectId = exam.CourseSubjectId,
//                        roomId = exam.RoomId,
//                        accountId = exam.AccountId,
//                        examDay = exam.ExamDay.ToString("yyyy-MM-ddTHH:mm:ss"),
//                        examTime = exam.ExamTime.ToString(@"hh\:mm\:ss"),
//                        status = exam.Status,
//                        fee = exam.Fee
//                    });

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


//        // GET: ExamController1/Delete/5
//        public ActionResult Delete(int id)
//        {
//            return View();
//        }

//        // POST: ExamController1/Delete/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Delete(int id, IFormCollection collection)
//        {
//            try
//            {
//                return RedirectToAction(nameof(Index));
//            }
//            catch
//            {
//                return View();
//            }
//        }
//    }
//}
