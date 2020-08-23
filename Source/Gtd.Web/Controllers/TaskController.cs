using AutoMapper;
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
        private readonly IMapper _mapper;

        public TaskController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            var data = await _context.Tasks.Include(t => t.User).Where(t => t.UserId == user.Id && t.CompletionStatus != (int)CompletionStatus.Completed).ToListAsync();
            data = data.OrderByDescending(t => t.Urgent)
                       .ThenByDescending(t => t.Important)
                       .ThenBy(t => t.DueDate)
                       .ToList();
            var model = data.Select(d => _mapper.Map<TaskViewModel>(d)).ToList();
            return View(model);
        }

        [HttpGet]
        [Route("/Tasks/DueDate")]
        public async Task<IActionResult> DueDate()
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if(user == null)
            {
                return NotFound();
            }
            var data = await _context.Tasks.Include(t => t.User).Where(t => t.UserId == user.Id && t.CompletionStatus != (int)CompletionStatus.Completed).ToListAsync();
            var model = data.Select(d => _mapper.Map<TaskViewModel>(d)).ToList();
            return View("Index", data);
        }

        // GET: TaskModel/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null || task.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }
            var model = _mapper.Map<TaskViewModel>(task);
            return View(model);
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CompletionStatus,Important,Urgent,DueDate")] TaskViewModel taskModel)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if (ModelState.IsValid)
            {
                taskModel.Id = Guid.NewGuid();
                taskModel.Created = DateTime.UtcNow;
                taskModel.Updated = DateTime.UtcNow;
                taskModel.UserId = user.Id;
                var task = _mapper.Map<TaskDto>(taskModel);
                _context.Add(task);
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

            var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null || task.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", task.UserId);
            var model = _mapper.Map<TaskViewModel>(task);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult>Complete(Guid? id, string redirect)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            if(task.User.Email != this.ControllerContext.HttpContext.User.Identity.Name)
            {
                return NotFound();
            }
            task.CompletionStatus = (int)CompletionStatus.Completed;
            await _context.SaveChangesAsync();
            if(string.IsNullOrEmpty(redirect))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Redirect(redirect);
            }
        }

        // POST: TaskModel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Created,Updated,Title,Description,CompletionStatus,Importance,Urgency,DueDate,UserId")] TaskViewModel taskModel)
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
                existing.CompletionStatus = (int)taskModel.CompletionStatus;
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

            var task = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id && m.User.Email == this.ControllerContext.HttpContext.User.Identity.Name);
            if (task == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<TaskViewModel>(task);
            return View(model);
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
