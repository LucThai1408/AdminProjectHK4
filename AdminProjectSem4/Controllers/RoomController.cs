using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Security.Principal;
using System.Xml.Linq;

namespace AdminProjectSem4.Controllers
{
    public class RoomController : Controller
    {
        string uri = "https://localhost:44341/api/admin/";
        HttpClient client = new HttpClient();

        // GET: RoomControler
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;
            var RoomAll = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            var Rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms/?currentPage=" + currentPage));
            if (!name.Equals(""))
            {
                Rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms/?name=" + name));
                RoomAll = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms/?name=" + name));
            }
            else
            {
                Rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms/?currentPage=" + currentPage));
            }
            ViewBag.TotalPage = RoomAll.Count() % pageSize == 0 ? RoomAll.Count() / pageSize : RoomAll.Count() / pageSize + 1;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;
            return View(Rooms);
        }

        // GET: RoomControler/Details/5
        public async Task<ActionResult> Details(int id)
        {
            client.BaseAddress = new Uri(uri + id);
            var Room= JsonConvert.DeserializeObject<Room>(await client.GetStringAsync("Rooms/" + id));
            return View(Room);
        }

        // GET: RoomControler/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RoomControler/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Room room)
        {
            client.BaseAddress = new Uri(uri);
            var rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            if (room.Name == null || room.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Category name can not empty!";
                return View(room);
            }
            if (room != null)
            {
                foreach (var item in rooms)
                {
                    if (item.Name.ToLower().Equals(room.Name.Trim().ToLower()))
                    {
                        ViewBag.ErrorName = "Category name is already exist!";
                        return View(room);
                    }
                }
            }
            var response = await client.PostAsJsonAsync("Rooms", new { Name = room.Name, Status = room.Status });
            TempData["msg"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");

        }

        // GET: RoomControler/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            client.BaseAddress = new Uri(uri);
            var room = JsonConvert.DeserializeObject<Room>(await client.GetStringAsync("Rooms/" + id));
            return View(room);
        }

        // POST: RoomControler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Room room)
        {
            client.BaseAddress = new Uri(uri);
            var rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            if (room.Name == null || room.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Category name can not empty!";
                return View(room);
            }
            if (room != null)
            {
                foreach (var item in rooms)
                {
                    if (item.Name.ToLower().Equals(room.Name.Trim().ToLower()) && item.RoomId != room.RoomId)
                    {
                        ViewBag.ErrorName = "Room name is already exist!";
                        return View(room);
                    }
                }
            }
            var response = await client.PutAsJsonAsync("Rooms/" + id, room);
            TempData["msg"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");
        }

        // GET: RoomControler/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: RoomControler/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);
            bool check = true;
            var rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            if (rooms != null)
            {
                foreach (var room in rooms)
                {
                    if (room.RoomId == id)
                    {
                        check = false;
                        break;
                    }
                }
            }
            if (!check)
            {
                TempData["msg"] = "Can not delete this room because has item related!";
                return RedirectToAction("Index");
            }
            var responsd = await client.DeleteAsync("");
            TempData["msg"] = responsd.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");
        }
    }
}
