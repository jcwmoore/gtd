using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Gtd.Web.Data;
using Gtd.Web.Models;

namespace Gtd.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("/Tasks")]
        public async Task<IActionResult> Index()
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if(user == null)
            {
                return NotFound();
            }
            var applicationDbContext = _context.Tasks.Include(t => t.User).Where(t => t.UserId == user.Id && t.CompletionStatus != TaskCompletionStatus.Completed);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TaskModel/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskModel == null || taskModel.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }

            return View(taskModel);
        }

        // GET: TaskModel/Create
        public async Task<IActionResult> Create()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            ViewData["UserId"] = user.Id;
            return View();
        }

        // POST: TaskModel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CompletionStatus,Important,Urgent,DueDate")] TaskModel taskModel)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if (ModelState.IsValid)
            {
                taskModel.Id = Guid.NewGuid();
                taskModel.Created = DateTime.UtcNow;
                taskModel.Updated = DateTime.UtcNow;
                taskModel.UserId = user.Id;
                _context.Add(taskModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = user.Id;
            return View(taskModel);
        }

        // GET: TaskModel/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (taskModel == null || taskModel.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", taskModel.UserId);
            return View(taskModel);
        }


        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Complete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (taskModel == null)
            {
                return NotFound();
            }
            if(taskModel.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }
            taskModel.CompletionStatus = TaskCompletionStatus.Completed;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: TaskModel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Created,Updated,Title,Description,CompletionStatus,Importance,Urgency,DueDate,UserId")] TaskModel taskModel)
        {
            if (id != taskModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.User.Email == this.ControllerContext.HttpContext.User.Identity.Name);
                if(existing == null)
                {
                    return NotFound();
                }
                existing.Updated = DateTime.UtcNow;
                existing.Title = taskModel.Title;
                existing.DueDate = taskModel.DueDate;
                existing.Description = taskModel.Description;
                existing.Important = taskModel.Important;
                existing.Urgent = taskModel.Urgent;
                existing.CompletionStatus = taskModel.CompletionStatus;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", taskModel.UserId);
            return View(taskModel);
        }

        // GET: TaskModel/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id && m.User.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }

        // POST: TaskModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var taskModel = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id && m.User.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if (taskModel == null)
            {
                return NotFound();
            }
            _context.Tasks.Remove(taskModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
