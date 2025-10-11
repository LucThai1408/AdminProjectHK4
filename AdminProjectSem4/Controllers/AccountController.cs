using AdminProjectSem4.Models;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using WebEcom.Models;

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

                // Tạo SelectList cho Role
                ViewBag.RoleList = new SelectList(new[]
                {
                    new { Value = "", Text = "-- Tất cả Role --" },
                    new { Value = "0", Text = "Sinh viên" },
                    new { Value = "1", Text = "Admin" },
                    new { Value = "2", Text = "Giảng viên" }
                }, "Value", "Text", role.HasValue ? role.Value.ToString() : "");

                // Tạo SelectList cho Status
                ViewBag.StatusList = new SelectList(new[]
                {
                    new { Value = "", Text = "-- Tất cả trạng thái --" },
                    new { Value = "true", Text = "Hoạt động" },
                    new { Value = "false", Text = "Không hoạt động" }
                }, "Value", "Text", string.IsNullOrEmpty(status) ? "" : status);

                // Truyền thêm dữ liệu sang View
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: AccountsController/Create
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Account account, IFormFile fileImage, string confirmPassword = "")
        {
            try
            {
                bool check = true;
                var accounts = JsonConvert.DeserializeObject<List<Account>>(await client.GetStringAsync("accounts/getAll"));

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

                if (!check)
                {
                    return View(account);
                }

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(account.Name), "name" },
                    { new StringContent(account.Password), "password" },
                    { new StringContent(account.Email), "email" },
                    { new StringContent(account.Phone), "phone" },
                    { new StringContent(account.Role.ToString()), "role" }
                };

                if (fileImage != null && fileImage.Length > 0)
                {
                    formData.Add(new StreamContent(fileImage.OpenReadStream()), "image", fileImage.FileName);
                }

                var response = await client.PostAsync("accounts", formData);
                TempData["msg"] = await response.Content.ReadAsStringAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
                return View(account);
            }
        }

        // GET: AccountsController/Edit/5
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Edit(int id)
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

        // POST: AccountsController/Edit/5
        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Account account, IFormFile fileImage, string confirmPassword = "", string oldImage = "")
        {
            try
            {
                if (account.Phone == null) account.Phone = "";

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(account.AccountId.ToString()), "accountId" },
                    { new StringContent(account.Name), "name" },
                    { new StringContent(account.Password), "password" },
                    { new StringContent(account.Email), "email" },
                    { new StringContent(account.Phone), "phone" },
                    { new StringContent(account.Role.ToString()), "role" }
                };

                if (fileImage != null && fileImage.Length > 0)
                {
                    formData.Add(new StreamContent(fileImage.OpenReadStream()), "image", fileImage.FileName);
                }
                else
                {
                    formData.Add(new StringContent(oldImage ?? ""), "oldImage");
                }

                var response = await client.PutAsync("accounts/" + id, formData);
                TempData["msg"] = await response.Content.ReadAsStringAsync();

                if (TempData["msg"].ToString().ToLower().Contains("warning"))
                {
                    ViewBag.Error = "Email already exists!";
                    return View(account);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
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
