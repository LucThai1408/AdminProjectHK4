using AdminProjectSem4.Models;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AdminProjectSem4.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        string url = "https://localhost:44341/api/admin/";
        HttpClient client = new HttpClient();

        public AccountsController()
        {
            client.BaseAddress = new Uri(url);
        }

        // GET: AccountsController
        public async Task<ActionResult> Index(string name = "", int? role = null, string? status = null, int currentPage = 1)
        {
            try
            {
                int pageSize = 5;

                // Gọi API theo filter
                string apiUrl = "accounts?";
                if (!string.IsNullOrEmpty(name))
                    apiUrl += $"name={name}&";
                if (role.HasValue)
                    apiUrl += $"role={role}&";
                if (!string.IsNullOrEmpty(status))
                    apiUrl += $"status={status}&";

                var allAccounts = JsonConvert.DeserializeObject<List<Account>>(
                    await client.GetStringAsync(apiUrl)
                );

                int totalPage = (int)Math.Ceiling((double)allAccounts.Count / pageSize);

                var accounts = allAccounts
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // ✅ Thêm đường dẫn gốc của API
                ViewBag.Url = "https://localhost:44341";

                // Role list
                ViewBag.RoleList = new SelectList(new[]
                {
                    new { Value = "", Text = "-- Tất cả Role --" },
                    new { Value = "0", Text = "Sinh viên" },
                    new { Value = "1", Text = "Admin" },
                    new { Value = "2", Text = "Giảng viên" }
                }, "Value", "Text", role.HasValue ? role.Value.ToString() : "");

                // Status list
                ViewBag.StatusList = new SelectList(new[]
                {
                    new { Value = "", Text = "-- Tất cả trạng thái --" },
                    new { Value = "true", Text = "Hoạt động" },
                    new { Value = "false", Text = "Không hoạt động" }
                }, "Value", "Text", string.IsNullOrEmpty(status) ? "" : status);

                // Paging info
                ViewBag.TotalPage = totalPage;
                ViewBag.CurrentPage = currentPage;
                ViewBag.Name = name;
                ViewBag.Role = role;
                ViewBag.Status = status;

                return View(accounts);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return View(new List<Account>());
            }
        }



        // GET: AccountsController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                ViewBag.Url = "https://localhost:44341";
                var account = JsonConvert.DeserializeObject<Account>(await client.GetStringAsync("accounts/" + id));
                if (account != null && account.RoomId > 0)
                {
                    var roomJson = await client.GetStringAsync("rooms/" + account.RoomId);
                    var room = JsonConvert.DeserializeObject<Room>(roomJson);
                    account.Room = room; // Gán lại cho Account
                }
                ViewBag.Url = "https://localhost:44341";
                return View(account);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: AccountsController/Create
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Create()
        {
            try
            {
                var response = await client.GetAsync("Rooms");
                List<Room> rooms = new();

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    rooms = JsonConvert.DeserializeObject<List<Room>>(jsonData) ?? new List<Room>();
                }

                // ✅ Nếu không có dữ liệu, tạo list trống để tránh NullReferenceException
                ViewBag.RoomList = new SelectList(rooms, "RoomId", "Name");

                return View();
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                ViewBag.RoomList = new SelectList(new List<Room>(), "RoomId", "Name");
                return View();
            }
        }


        // POST: AccountsController/Create
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<ActionResult> Create(Account account, IFormFile fileImage, string confirmPassword = "")
        {
            try
            {
                bool check = true;
                var accounts = JsonConvert.DeserializeObject<List<Account>>(await client.GetStringAsync("accounts"));

                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(account.FullName))
                {
                    ViewBag.ErrorFullName = "Account fullname cannot be empty!";
                    check = false;
                }
                if (string.IsNullOrWhiteSpace(account.Name))
                {
                    ViewBag.ErrorName = "Account name cannot be empty!";
                    check = false;
                }
                if (string.IsNullOrWhiteSpace(account.Password))
                {
                    ViewBag.ErrorPassword = "Password cannot be empty!";
                    check = false;
                }
                if (account.DateOfBirth == null || account.DateOfBirth == default(DateTime))
                {
                    ViewBag.ErrorDateOfBirth = "Date of birth cannot be empty!";
                    check = false;
                }
                if (!string.IsNullOrEmpty(confirmPassword) && !string.IsNullOrEmpty(account.Password))
                {
                    if (!account.Password.Equals(confirmPassword))
                    {
                        ViewBag.ErrorConfirmPassword = "Confirm password is not correct!";
                        check = false;
                    }
                    else
                    {
                        account.Password = Cipher.GenerateMD5(account.Password);
                    }
                }
                if (string.IsNullOrWhiteSpace(account.Email))
                {
                    ViewBag.ErrorEmail = "Email cannot be empty!";
                    check = false;
                }
                if (account.Phone == null) account.Phone = "";

                if (accounts != null && accounts.Any(x => x.Email == account.Email))
                {
                    ViewBag.ErrorEmail = "Email already exists!";
                    check = false;
                }

                // ===== Nếu validation lỗi thì load lại danh sách lớp học =====
                if (!check)
                {
                    await LoadRoomList(); // ✅ Thêm dòng này
                    return View(account);
                }

                // ===== GỬI DỮ LIỆU LÊN API =====
                var formData = new MultipartFormDataContent
        {
            { new StringContent(account.Name ?? ""), "name" },
            { new StringContent(account.FullName ?? ""), "fullName" },
            { new StringContent(account.Password ?? ""), "password" },
            { new StringContent(account.Email ?? ""), "email" },
            { new StringContent(account.Phone ?? ""), "phone" },
            { new StringContent(account.Role.ToString()), "role" },
            { new StringContent(account.RoomId.ToString()), "roomId" },
            { new StringContent(account.Address ?? ""), "address" },
            { new StringContent(account.DateOfBirth.ToString("yyyy-MM-dd")), "dateOfBirth" },
            { new StringContent(account.Status.ToString()), "status" }
        };

                if (fileImage != null && fileImage.Length > 0)
                {
                    formData.Add(new StreamContent(fileImage.OpenReadStream()), "image", fileImage.FileName);
                }

                var response = await client.PostAsync("accounts", formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["msg"] = "Account created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "API Error: " + responseContent);
                    await LoadRoomList(); // ✅ Load lại để dropdown không mất
                    return View(account);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                await LoadRoomList(); // ✅ Load lại danh sách khi có lỗi
                return View(account);
            }
        }

        // 🧩 Hàm phụ nạp danh sách lớp học
        private async Task LoadRoomList()
        {
            var response = await client.GetAsync("Rooms");
            List<Room> rooms = new();

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                rooms = JsonConvert.DeserializeObject<List<Room>>(jsonData) ?? new List<Room>();
            }

            ViewBag.RoomList = new SelectList(rooms, "RoomId", "Name");
        }


        // GET: AccountsController/Edit/5
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var response = await client.GetAsync("Rooms");
                List<Room> rooms = new();

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    rooms = JsonConvert.DeserializeObject<List<Room>>(jsonData) ?? new List<Room>();
                }

                // ✅ Lấy account cần sửa
                var accountResponse = await client.GetAsync("accounts/" + id);
                if (!accountResponse.IsSuccessStatusCode)
                {
                    TempData["msg"] = "Không tìm thấy tài khoản!";
                    return RedirectToAction("Index");
                }

                var accountJson = await accountResponse.Content.ReadAsStringAsync();
                var account = JsonConvert.DeserializeObject<Account>(accountJson);

                // ✅ Gán danh sách phòng + phòng hiện tại được chọn
                ViewBag.RoomList = new SelectList(rooms, "RoomId", "Name", account.RoomId);

                ViewBag.Url = "https://localhost:44341";
                return View(account);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        // POST: AccountsController/Edit/5
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Account account, IFormFile fileImage, string oldImage = "")
        {
            try
            {
                if (account.Phone == null) account.Phone = "";

                using var client = new HttpClient();
                var formData = new MultipartFormDataContent
        {
            { new StringContent(account.AccountId.ToString()), "accountId" },
            { new StringContent(account.Name), "name" },
            { new StringContent(account.FullName), "fullName" },
            { new StringContent(account.RoomId.ToString()), "roomId" },
            { new StringContent(account.Password), "password" },
            { new StringContent(account.Address), "address" },
            { new StringContent(account.Email), "email" },
            { new StringContent(account.DateOfBirth.ToString("yyyy-MM-dd")), "dateOfBirth" },
            { new StringContent(account.Status.ToString()), "status" },
            { new StringContent(account.Phone), "phone" },
            { new StringContent(account.Role.ToString()), "role" }
        };

                // File ảnh
                if (fileImage != null && fileImage.Length > 0)
                {
                    formData.Add(new StreamContent(fileImage.OpenReadStream()), "image", fileImage.FileName);
                }
                else
                {
                    formData.Add(new StringContent(oldImage ?? ""), "oldImage");
                }

                var response = await client.PutAsync("https://localhost:44341/api/admin/accounts/" + id, formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = responseContent;
                    return View(account);
                }

                TempData["msg"] = responseContent;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(account);
            }
        }


        // GET: AccountsController/Delete/5
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var account = JsonConvert.DeserializeObject<Account>(await client.GetStringAsync("accounts/" + id));
                ViewBag.Url = "https://localhost:44316";
                return View(account);
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: AccountsController/Deactive/5
        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<ActionResult> Deactive(int id)
        {
            try
            {
                // 🔹 Lấy ID tài khoản hiện đang đăng nhập
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (currentUserId != null && id.ToString() == currentUserId)
                {
                    // 🚫 Không cho vô hiệu hóa tài khoản đang đăng nhập
                    TempData["msg"] = "❌ You cannot deactivate your own account!";
                    return RedirectToAction("Index");
                }

                // 🔹 Lấy thông tin tài khoản cần vô hiệu hóa
                var accountJson = await client.GetStringAsync("accounts/" + id);
                var account = JsonConvert.DeserializeObject<Account>(accountJson);

                if (account == null)
                {
                    TempData["msg"] = "❌ Account not found!";
                    return RedirectToAction("Index");
                }

                // 🔹 Chuẩn hóa dữ liệu
                if (account.Phone == null) account.Phone = "";
                if (account.Image == null) account.Image = "";

                account.Status = false; // 🔒 Vô hiệu hóa tài khoản

                // ⚙️ Gửi yêu cầu DELETE tới API (vì API đang là [HttpDelete])
                var response = await client.DeleteAsync("accounts/" + id);

                if (response.IsSuccessStatusCode)
                {
                    TempData["msg"] = "Account deactivated successfully!";
                }
                else
                {
                    string apiMsg = await response.Content.ReadAsStringAsync();
                    TempData["msg"] = $"Failed to deactivate account: {apiMsg}";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["msg"] = "💥 Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}
