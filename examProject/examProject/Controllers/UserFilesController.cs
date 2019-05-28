using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using examProject.Models;
using examProject.Db;

namespace VSZANAL.Controllers
{
    public class UserFilesController : Controller
    {
        private readonly DataContext _context;
        private readonly IHostingEnvironment _appEnvironment;

        public UserFilesController(DataContext context, IHostingEnvironment appEnvironment)
        {
            _context = context;
            _appEnvironment = appEnvironment;
        }

        // GET: UserFiles/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: UserFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Path,Time,UserId")] UserFile userFile, string text)
        {
            if (ModelState.IsValid)
            {
                var login = HttpContext.Response.HttpContext.User.Identity.Name;
                User user = await _context.Users.FirstOrDefaultAsync();
                var time = DateTime.Now;
                var filename = userFile.Name + "_" + time.ToShortDateString().Replace('.', '-') + '-' + time.ToLongTimeString().Replace(':', '-') + ".txt";
                if (userFile.Previous == 0)
                {
                    userFile.Time = time;
                    userFile.Path = "/Files/" + filename;
                    userFile.UserId = user.Id;
                    userFile.Previous = -1;
                }
                _context.Add(userFile);//в бд
                SaveToFile(text, filename);//в папку
                await _context.SaveChangesAsync();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFile.UserId);
            return View(userFile);
        }

        private void SaveToFile(string text, string filename)
        {
            string path = "/Files/" + filename;

            try
            {
                using (StreamWriter sw = new StreamWriter(_appEnvironment.WebRootPath + path, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // GET: UserFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFile = await _context.Files.FindAsync(id);
            var realname = userFile.Path.Remove(0, 7);
            GetFile(realname);
            if (userFile == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFile.UserId);
            return View(userFile);
        }

        private string ToShortName(string fileName, int count)
        {
            return fileName.Remove(fileName.Length - count, count);
        }

        private void GetFile(string fileName)
        {
            ViewData["fileName"] = fileName;
            try
            {
                var path = "/Files/" + fileName;
                using (StreamReader sr = new StreamReader(_appEnvironment.WebRootPath + path))
                {
                    ViewData["fileTXT"] = sr.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // POST: UserFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Path,Time,UserId")] UserFile userFile,
            string text, string oldName)
        {
            if (id != userFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var login = HttpContext.Response.HttpContext.User.Identity.Name;
                    User user = await _context.Users.FirstOrDefaultAsync();
                    var time = DateTime.Now;
                    var filename = userFile.Name + "_" + time.ToShortDateString().Replace('.', '-') + '-' + time.ToLongTimeString().Replace(':', '-') + ".txt";
                    var newUs = new UserFile { Name = userFile.Name, Path = "/Files/" + filename, Time = time, UserId = user.Id, Previous = userFile.Id };

                    //_context.Update(userFile);
                    await Create(newUs, text);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserFileExists(userFile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userFile.UserId);
            return View(userFile);
        }

        // GET: UserFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userFile = await _context.Files
                .Include(u => u.UserId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userFile == null)
            {
                return NotFound();
            }

            return View(userFile);
        }

        // POST: UserFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userFile = await _context.Files.FindAsync(id);
            var path = _appEnvironment.WebRootPath + userFile.Path;
            RemoveFile(path);
            _context.Files.Remove(userFile);
            await _context.SaveChangesAsync();
            return View(userFile);
        }

        public FileResult Download(string filename)
        {
            var path = _appEnvironment.WebRootPath + filename;
            // Объект Stream
            FileStream fs = new FileStream(path, FileMode.Open);
            string file_type = "application/txt";
            string file_name = ToShortName(filename) + ".txt";
            return File(fs, file_type, file_name);
        }

        private string ToShortName(string fileName)
        {
            return fileName.Remove(fileName.Length - 24, 24).Remove(0, 7);
        }

        private bool UserFileExists(int id)
        {
            return _context.Files.Any(e => e.Id == id);
        }

        private void RemoveFile(string path) => System.IO.File.Delete(path);
    }
}
