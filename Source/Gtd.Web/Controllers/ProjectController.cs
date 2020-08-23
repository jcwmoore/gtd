using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gtd.Web.Data;
using Gtd.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gtd.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;


        public ProjectController(ApplicationDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        [Route("/Projects")]
        public async Task<IActionResult> Index()
        {
            var data = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.CompletionStatus != (int)CompletionStatus.Completed).ToListAsync();
            var model = data.Select(d => _mapper.Map<ProjectViewModel>(d)).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Title,CompletionStatus")] ProjectViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == ControllerContext.HttpContext.User.Identity.Name);
                if(user == null)
                {
                    return NotFound();
                }
                var data = _mapper.Map<ProjectDto>(model);
                data.Created = DateTime.UtcNow;
                data.Updated = DateTime.UtcNow;
                data.User = user;
                data.UserId = user.Id;
                await _context.Projects.AddAsync(data);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            var project = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<ProjectDetailsViewModel>(project);
            model.Tasks = model.Tasks.Where(t => t.CompletionStatus != CompletionStatus.Completed).ToList();
            return View(model);

        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            var project = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<ProjectDetailsViewModel>(project);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var project = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            _context.Tasks.RemoveRange(project.Tasks);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            var project = await _context.Projects.Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<ProjectViewModel>(project);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, ProjectViewModel model)
        {
            if(id != model.Id)
            {
                return NotFound();
            }
            var project = await _context.Projects.Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            project.Updated = DateTime.UtcNow;
            project.Title = model.Title;
            project.CompletionStatus = (int)model.CompletionStatus;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Complete(Guid id)
        {
            var project = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            project.CompletionStatus = (int)CompletionStatus.Completed;
            foreach(var task in project.Tasks)
            {
                task.CompletionStatus = (int)CompletionStatus.Completed;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(Guid id, [Bind("Id,Title,Description,CompletionStatus,Important,Urgent,DueDate,ProjectId")] TaskViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == ControllerContext.HttpContext.User.Identity.Name);
            if(user == null)
            {
                return NotFound();
            }
            var project = await _context.Projects.Include(p => p.Tasks).Where(p => p.User.Email == ControllerContext.HttpContext.User.Identity.Name && p.Id == id).FirstOrDefaultAsync();
            if(project == null)
            {
                return NotFound();
            }
            var data = _mapper.Map<TaskDto>(model);
            data.Created = DateTime.UtcNow;
            data.Updated = DateTime.UtcNow;
            data.Id = Guid.NewGuid();
            data.ProjectId = id;
            data.Project = project;
            data.UserId =  user.Id;
            data.User = user;
            await _context.Tasks.AddAsync(data);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
