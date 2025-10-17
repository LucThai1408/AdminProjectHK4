using AdminProjectSem4.Models;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    [Authorize]
    public class AccountExamController : Controller
    {
        string uri = "https://localhost:44341/api/";
        HttpClient client = new HttpClient();

        // GET: AccountExamController
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 5;

            // Lấy dữ liệu
            var accountExamsJson = await client.GetStringAsync("admin/AccountExams");
            var accountExams = JsonConvert.DeserializeObject<List<AccountExamDto>>(accountExamsJson) ?? new List<AccountExamDto>();

            var subjectsJson = await client.GetStringAsync("admin/Subjects");
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(subjectsJson) ?? new List<Subject>();

            var examsJson = await client.GetStringAsync("admin/Exams");
            var exams = JsonConvert.DeserializeObject<List<ExamDto>>(examsJson) ?? new List<ExamDto>();

            var accountsJson = await client.GetStringAsync("admin/Accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsJson) ?? new List<Account>();

            if (!string.IsNullOrEmpty(name))
            {
                accountExams = accountExams
                    .Where(ac =>
                        // 🔍 Tìm theo tên bài thi
                        (!string.IsNullOrEmpty(ac.ExamName) && ac.ExamName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||

                        // 👨‍🎓 Tìm theo tên sinh viên
                        (ac.StudentName != null &&
                         !string.IsNullOrEmpty(ac.StudentName) &&
                         ac.StudentName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||

                        // 📚 Tìm theo tên môn học
                        (ac.Subject != null &&
                         !string.IsNullOrEmpty(ac.Subject) &&
                         ac.Subject.Contains(name, StringComparison.OrdinalIgnoreCase))
                    )
                    //.OrderByDescending(ac => ac.AccountExamId)
                    .ToList();
            }


            var totalPage = (int)Math.Ceiling((double)accountExams.Count / pageSize);
            var pagedData = accountExams
                .OrderByDescending(ac => ac.AccountExamId)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(pagedData);
        }


        // GET: AccountExamController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                client.BaseAddress = new Uri(uri);

                var accountExamsJson = await client.GetStringAsync("admin/AccountExams/" + id);
                var accountExam = JsonConvert.DeserializeObject<AccountExamDto>(accountExamsJson);

                return View(accountExam);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        [Authorize(Roles = "1")]
        // GET: AccountExamController/Create
        public async Task<ActionResult> Create()
        {
            client.BaseAddress = new Uri(uri);

            // ✅ 1. Lấy danh sách CourseSubjects (chứa cả Course + Subject)
            var courseSubjectsJson = await client.GetStringAsync("CourseSubjects");
            var courseSubjects = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(courseSubjectsJson) ?? new List<CourseSubjectDTO>();

            // Lọc những CourseSubject đang active
            var activeCourseSubjects = courseSubjects.Where(cs => cs.Status == true).ToList();

            // ✅ 2. Lấy danh sách Student (Role = 0 => sinh viên)
            var allStudentsJson = await client.GetStringAsync("admin/Accounts");
            var allStudents = JsonConvert.DeserializeObject<List<Account>>(allStudentsJson) ?? new List<Account>();
            var students = allStudents.Where(c => c.Status == true && c.Role == 0).ToList();

            // ✅ 3. Tạo dropdown cho CourseSubject (hiển thị "CourseName - SubjectName")
            ViewBag.Subject = new SelectList(
                activeCourseSubjects.Select(cs => new
                {
                    cs.CourseSubjectId,
                    DisplayName = $"{cs.CourseName} - {cs.SubjectName}"
                }),
                "CourseSubjectId",
                "DisplayName"
            );

            var allExams = JsonConvert.DeserializeObject<List<ExamDto>>(
                            await client.GetStringAsync("admin/Exams")) ?? new List<ExamDto>();

            var exams = allExams
                .Select(e => new
                {
                    e.ExamId,
                    DisplayName = $"{e.ExamDayString} - {e.ExamTimeString}"
                })
                .ToList();
            ViewBag.Exam = new SelectList(exams, "ExamId", "DisplayName");
            // ✅ 4. Dropdown Student
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");
            return View();
        }


        // POST: AccountExamController/Create
        // ✅ POST: AccountExamController/Create
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AccountExam accountExam)
        {
            try
            {
                client.BaseAddress = new Uri(uri);

                

                // ✅ 1. Kiểm tra xem sinh viên có trong khóa học chưa
                var courseStudentsResponse = await client.GetAsync("CourseStudents");
                var courseStudentsJson = await courseStudentsResponse.Content.ReadAsStringAsync();
                var courseStudents = JsonConvert.DeserializeObject<List<CourseStudentDTO>>(courseStudentsJson) ?? new List<CourseStudentDTO>();

                // ✅ Lấy thông tin khóa học của môn
                var allCourseSubjectsJson = await client.GetStringAsync("CourseSubjects");
                var courseSubjects = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(allCourseSubjectsJson) ?? new List<CourseSubjectDTO>();
                var courseSubject = courseSubjects.FirstOrDefault(cs => cs.CourseSubjectId == accountExam.CourseSubjectId);

                // ✅ Lấy thông tin khóa học của môn
                var allSubjectsJson = await client.GetStringAsync("admin/subjects");
                var subjects = JsonConvert.DeserializeObject<List<Subject>>(allSubjectsJson) ?? new List<Subject>();
                var subject = subjects.FirstOrDefault(s => s.SubjectId == courseSubject.SubjectId);

                var isStudentInCourse = courseStudents.Any(cs =>
                    cs.StudentId == accountExam.StudentId &&
                    cs.CourseId == courseSubject.CourseId &&
                    cs.Status == true
                );

                if (!isStudentInCourse)
                {
                    ViewBag.Error = "❌ Sinh viên này chưa được ghi danh trong khóa học, không thể tạo bài thi!";
                    await LoadDropdowns(accountExam); // 🟢 Gửi lại object để giữ giá trị
                    return View(accountExam);
                }

                // ✅ 2. Lấy danh sách tất cả AccountExam
                var allExamsResponse = await client.GetAsync("admin/AccountExams");
                var existingExamsJson = await allExamsResponse.Content.ReadAsStringAsync();
                var existingExams = JsonConvert.DeserializeObject<List<AccountExamDto>>(existingExamsJson) ?? new List<AccountExamDto>();

                var existingRecord = existingExams
                    .Where(ae => ae.ExamId == accountExam.ExamId
                              && ae.StudentId == accountExam.StudentId
                              && ae.SubjectId == courseSubject.SubjectId
                              && ae.CourseId == courseSubject.CourseId)
                    .OrderByDescending(ae => ae.Record) // sắp xếp theo Record giảm dần
                    .FirstOrDefault(); // lấy bản ghi Record lớn nhất


                // ✅ 4. Kiểm tra số lần thi
                if (existingRecord != null)
                {
                    if (existingRecord.Record >= 3)
                    {
                        ViewBag.Error = "❌ Sinh viên này đã thi 3 lần cho bài thi này, không thể thi thêm!";
                    }
                    else
                    {
                        accountExam.Record = existingRecord.Record + 1;
                    }
                }
                else
                {
                    accountExam.Record = 1;
                }

                if(accountExam.Score > subject.MaxScore)
                {
                    ViewBag.Error = $"❌ Điểm nhập vào lớn hơn điểm tối đa môn học! Xin hãy nhập lại!";
                    await LoadDropdowns(accountExam); // 🟢 giữ lại giá trị đã chọn
                    return View(accountExam);
                }


                // ✅ 5. Nếu có lỗi ở trên thì return view
                if (!string.IsNullOrEmpty(ViewBag.Error))
                {
                    await LoadDropdowns(accountExam); // 🟢 giữ lại giá trị đã chọn
                    return View(accountExam);
                }

                // ✅ 6. Xác định IsPass
                accountExam.IsPass = accountExam.Score >= subject.PassScore;

                // ✅ 7. Gửi dữ liệu lên API
                var response = await client.PostAsJsonAsync("admin/AccountExams", accountExam);

                if (response.IsSuccessStatusCode)
                {
                    TempData["msg"] = "✅ Thêm mới thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"❌ Không thể thêm AccountExams. Mã lỗi: {response.StatusCode}. Chi tiết: {errorBody}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi gửi dữ liệu: " + ex.Message;
            }

            await LoadDropdowns(accountExam); // 🟢 vẫn giữ khi có lỗi
            return View(accountExam);
        }




        // ===========================================
        // ✅ HÀM RIÊNG: LOAD DROPDOWN
        // ===========================================
        private async Task LoadDropdowns(AccountExam? accountExam = null)
        {
            try
            {
                var allExams = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(
                    await client.GetStringAsync("CourseSubjects"));
                var exams = allExams.Where(c => c.Status == true).ToList();

                var allSubject = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(
                    await client.GetStringAsync("CourseSubjects"));
                var subjects = allSubject.Where(c => c.Status == true).ToList();

                var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                    await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
                var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

                // 🟢 Truyền selectedValue nếu có dữ liệu từ form
                ViewBag.Subject = new SelectList(subjects, "CourseSubjectId", "SubjectName", accountExam?.CourseSubjectId);
                ViewBag.Exam = new SelectList(exams, "CourseSubjectId", "CourseName", accountExam?.CourseSubjectId);
                ViewBag.Student = new SelectList(students, "AccountId", "FullName", accountExam?.StudentId);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dropdown: " + ex.Message;
            }
        }




        // GET: AccountExamController/Edit/5
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);

            // ✅ Lấy dữ liệu hiện tại
            var accountExam = JsonConvert.DeserializeObject<AccountExam>(
                await client.GetStringAsync($"admin/AccountExams/{id}")
            );

            var courseSubjectsJson = await client.GetStringAsync("CourseSubjects");
            var courseSubjects = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(courseSubjectsJson) ?? new List<CourseSubjectDTO>();

            // Lọc những CourseSubject đang active
            var activeCourseSubjects = courseSubjects.Where(cs => cs.Status == true).ToList();

            // Load danh sách cho dropdown
            var allExams = JsonConvert.DeserializeObject<List<ExamDto>>(
                await client.GetStringAsync("admin/Exams"));

            var allSubject = JsonConvert.DeserializeObject<List<Subject>>(
                await client.GetStringAsync("admin/Subjects"));
            var subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();
            ViewBag.Subject = new SelectList(
                activeCourseSubjects.Select(cs => new
                {
                    cs.CourseSubjectId,
                    DisplayName = $"{cs.CourseName} - {cs.SubjectName} - {cs.CourseSubjectId}"
                }),
                "CourseSubjectId",
                "DisplayName",
                accountExam.CourseSubjectId
            );

            var exams = allExams
                .Select(e => new
                {
                    e.ExamId,
                    DisplayName = $"{e.ExamDayString} - {e.ExamTimeString}"
                })
                .ToList();
            ViewBag.Exam = new SelectList(exams, "ExamId", "DisplayName", accountExam.ExamId);
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");

            return View(accountExam);
        }

        // POST: AccountExamController/Edit/5
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, AccountExam accountExam)
        {
            client.BaseAddress = new Uri(uri);

            // ✅ Lấy thông tin khóa học của môn
            var allCourseSubjectsJson = await client.GetStringAsync("CourseSubjects");
            var courseSubjects = JsonConvert.DeserializeObject<List<CourseSubjectDTO>>(allCourseSubjectsJson) ?? new List<CourseSubjectDTO>();
            var courseSubject = courseSubjects.FirstOrDefault(cs => cs.CourseSubjectId == accountExam.CourseSubjectId);

            // ✅ Lấy thông tin khóa học của môn
            var allSubjectsJson = await client.GetStringAsync("admin/subjects");
            var subjects = JsonConvert.DeserializeObject<List<Subject>>(allSubjectsJson) ?? new List<Subject>();
            var subject = subjects.FirstOrDefault(s => s.SubjectId == courseSubject.SubjectId);

            try
            {
                // ✅ Kiểm tra Score để set IsPass
                if (accountExam.Score >= subject.PassScore)
                    accountExam.IsPass = true;
                else
                    accountExam.IsPass = false;
                if (accountExam.Score > subject.MaxScore)
                {
                    ViewBag.Error = $"❌ Điểm nhập vào lớn hơn điểm tối đa môn học! Xin hãy nhập lại!";
                    await LoadDropdowns(accountExam); // 🟢 giữ lại giá trị đã chọn
                    return View(accountExam);
                }
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
            subjects = allSubject.Where(c => c.Status == true).ToList();

            var allStudent = JsonConvert.DeserializeObject<List<Account>>(
                await client.GetStringAsync("admin/Accounts")) ?? new List<Account>();
            var students = allStudent.Where(c => c.Status == true && c.Role == 0).ToList();

            ViewBag.Subject = new SelectList(subjects, "SubjectId", "Name");
            ViewBag.Exam = new SelectList(exams, "ExamId", "Name");
            ViewBag.Student = new SelectList(students, "AccountId", "FullName");

            return View(accountExam);
        }


        // GET: AccountExamController/Delete/5
        [Authorize(Roles = "1")]
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
            accountExam.Status = false;

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
