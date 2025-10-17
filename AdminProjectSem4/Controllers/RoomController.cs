using API.Models;
using Microsoft.AspNetCore.Authorization;
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

        public bool fail { get; private set; }

        // GET: RoomControler
        public async Task<ActionResult> Index(string name = "", int currentPage = 1)
        {
            client.BaseAddress = new Uri(uri);
            int pageSize = 6;
            var allRooms = JsonConvert.DeserializeObject<List<Room>>(
                await client.GetStringAsync("Rooms"));

            if (!string.IsNullOrEmpty(name))
            {
                allRooms = allRooms
                    .Where(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalPage = (int)Math.Ceiling((double)allRooms.Count / pageSize);

            var rooms = allRooms
                .OrderByDescending(r => r.RoomId)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = currentPage;
            ViewBag.Name = name;

            return View(rooms);
        }



        // GET: RoomControler/Details/5
        public async Task<ActionResult> Detail(int id)
        {
            client.BaseAddress = new Uri(uri + id);
            var Room= JsonConvert.DeserializeObject<Room>(await client.GetStringAsync("Rooms/" + id));
            return View(Room);
        }

        // GET: RoomControler/Create
        [Authorize(Roles = "1")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: RoomControler/Create
        [Authorize(Roles = "1")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Room room)
        {
            client.BaseAddress = new Uri(uri);
            var rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            if (room.Name == null || room.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Room name can not empty!";
                return View(room);
            }
            var response = await client.PostAsJsonAsync("Rooms", new { Name = room.Name, Status = room.Status });
            TempData["msg"] = "Room created successfully!";
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
        [Authorize(Roles = "1")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Room room)
        {
            client.BaseAddress = new Uri(uri);
            var rooms = JsonConvert.DeserializeObject<List<Room>>(await client.GetStringAsync("Rooms"));
            if (room.Name == null || room.Name.Trim().Equals(""))
            {
                ViewBag.ErrorName = "Room name can not empty!";
                return View(room);
            }
            var response = await client.PutAsJsonAsync("Rooms/" + id, room);
            TempData["msg"] = response.Content.ReadAsStringAsync().Result;
            return RedirectToAction("Index");
        }


        // POST: RoomControler/Delete/5
        [HttpPost]
        [Authorize(Roles = "1")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            client.BaseAddress = new Uri(uri);

            // Lấy room theo id
            var room = JsonConvert.DeserializeObject<Room>(
                await client.GetStringAsync($"Rooms/{id}")
            );

            if (room == null)
            {
                TempData["msg"] = "Room not found!";
                return RedirectToAction("Index");
            }
            room.Status = fail;

            var response = await client.PutAsJsonAsync($"Rooms/{id}", room);

            if (response.IsSuccessStatusCode)
            {
                TempData["msg"] = "Room has been disabled successfully!";
            }
            else
            {
                TempData["msg"] = "Error when disabling room!";
            }

            return RedirectToAction("Index");
        }

    }
}
