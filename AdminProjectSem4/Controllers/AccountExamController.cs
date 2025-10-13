using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    public class AccountExamController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        public bool fail { get; private set; }
        // GET: AccountExamController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;

            // Lấy dữ liệu
            var accountExamsJson = await client.GetStringAsync("admin/AccountExams");
            var accountExams = JsonConvert.DeserializeObject<List<AccountExam>>(accountExamsJson) ?? new List<AccountExam>();

            var subjectsJson = await client.GetStringAsync("admin/Subjects");
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson) ?? new List<Subject>();

            var examsJson = await client.GetStringAsync("admin/Exams");
            var exams = JsonConvert.DeserializeObject<List<Exam>>(examsJson) ?? new List<Exam>();

            var accountsJson = await client.GetStringAsync("admin/Accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsJson) ?? new List<Account>();

            foreach (var ae in accountExams)
            {
                ae.Subject = subjects.FirstOrDefault(s => s.SubjectId == ae.SubjectId);
                ae.Exam = exams.FirstOrDefault(e => e.ExamId == ae.ExamId);
                ae.Student = accounts.FirstOrDefault(a => a.AccountId == ae.StudentId);
            }

            accountExams = accountExams
                //.Where(ae =>
                //    (string.IsNullOrEmpty(name) ||
                //     (ae.Student != null && ae.Student.FullName.Contains(name, StringComparison.OrdinalIgnoreCase))) &&
                //    ae.Subject != null && ae.Subject.Status == true &&
                //    ae.Exam != null && ae.Exam.Status == true)
                .ToList();

            var totalPage = (int)Math.Ceiling((double)accountExams.Count / pageSize);
            var pagedData = accountExams
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }


        // GET: AccountExamController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AccountExamController/Create
        public async Task<ActionResult> Create()
        {
            client.BaseAddress = new Uri(uri);

            var allExams = JsonConvert.DeserializeObject<List<Exam>>(
                await client.GetStringAsync("admin/Exams"));
            var exams = allExams.Where(c => c.Status == true).ToList();

            var allSubject = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));
            var subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Exam = new SelectList(exams, "ExamId", "Name");
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");
            return View();
        }

        // POST: AccountExamController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AccountExam accountExam)
        {
            try
            {
                client.BaseAddress = new Uri(uri);
                var response = await client.PostAsJsonAsync("admin/AccountExams", accountExam);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể thêm AccountExams. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi gửi dữ liệu: " + ex.Message;
            }

            var allExams = JsonConvert.DeserializeObject<List<Exam>>(
                 await client.GetStringAsync("admin/Exams"));
            var exams = allExams.Where(c => c.Status == true).ToList();

            var allSubject = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));
            var subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Exam = new SelectList(exams, "ExamId", "Name");
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");

            return View(accountExam);
        }

        // GET: AccountExamController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);

            var accountExam = JsonConvert.DeserializeObject<AccountExam>(await client.GetStringAsync("admin/AccountExams/" + id));

            var allExams = JsonConvert.DeserializeObject<List<Exam>>(
                  await client.GetStringAsync("admin/Exams"));
            var exams = allExams.Where(c => c.Status == true).ToList();

            var allSubject = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));
            var subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Exam = new SelectList(exams, "ExamId", "Name");
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");

            return View(accountExam);
        }

        // POST: AccountExamController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, AccountExam accountExam)
        {
            try
            {
                client.BaseAddress = new Uri(uri);

                var response = await client.PutAsJsonAsync($"admin/AccountExams/{id}", accountExam);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Không thể cập nhật AccountExams. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi cập nhật dữ liệu: " + ex.Message;
            }

            // 🔹 Nếu lỗi, load lại danh sách để dropdown không bị rỗng
            var allExams = JsonConvert.DeserializeObject<List<Exam>>(
                  await client.GetStringAsync("admin/Exams"));
            var exams = allExams.Where(c => c.Status == true).ToList();

            var allSubject = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));
            var subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Exam = new SelectList(exams, "ExamId", "Name");
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");

            return View(accountExam);
        }

        // GET: AccountExamController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);
            var accountExam = JsonConvert.DeserializeObject<AccountExam>(
                await client.GetStringAsync($"admin/AccountExams/{id}")
            );

            if (accountExam == null)
            {
                TempData["msg"] = "Account Exam not found!";
                return RedirectToAction("Index");
            }
            accountExam.Status = fail;

            var response = await client.PutAsJsonAsync($"admin/AccountExams/{id}", accountExam);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "AccountExam ubject has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling courseStudent!";
            }

            return RedirectToAction("Index");
        }
    }
}
