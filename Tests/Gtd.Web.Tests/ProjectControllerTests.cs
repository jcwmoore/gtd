using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Gtd.Web.Data;
using Gtd.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gtd.Web.Controllers.Tests
{
    public class ProjectControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task IndexReturnsOnlyMyActiveProjects()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(IndexReturnsOnlyMyActiveProjects));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[2], ProjectId = projects[2].Id },
            };
            
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();
            
            //Then
            var obj = await controller.Index();
            var result = (obj) as ViewResult;
            result.Should().NotBeNull(because: "We expect a ViewResult not a " + obj.GetType().Name);
            var model = result.Model as IEnumerable<ProjectViewModel>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);
            model.All(pm => pm.CompletionStatus != CompletionStatus.Completed).Should().BeTrue();
            model.All(tm => tm.User.Id == USER_ID).Should().BeTrue();
            model.All(pm => pm.OutstandingTasks == 2).Should().BeTrue();
        }

        [Fact]
        public async Task CreateRedirectsToIndexOnSuccess()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CreateRedirectsToIndexOnSuccess));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var model = new ProjectViewModel();
            model.Title = "test project";
            model.CompletionStatus = CompletionStatus.InProgress;
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Create(model);
            var result = (obj) as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect result, not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            var projData = await db.Projects.FirstOrDefaultAsync();
            projData.Title.Should().Be(model.Title);
            projData.CompletionStatus.Should().Be((int)model.CompletionStatus);
            projData.UserId.Should().Be(USER_ID);
            projData.Created.Should().BeCloseTo(DateTime.UtcNow, 50);
            projData.Updated.Should().BeCloseTo(DateTime.UtcNow, 50);
        }

        [Fact]
        public async Task DetailsReturnsNotFoundForNonExistant()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DetailsReturnsNotFoundForNonExistant));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.SaveChangesAsync();
        
            //Then
            var obj = await controller.Details(projects[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect result, not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task DetailsReturnsViewResult()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DetailsReturnsViewResult));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.SaveChangesAsync();
        
            //Then
            var obj = await controller.Details(projects[2].Id);
            var result = obj as ViewResult;
            result.Should().NotBeNull(because: "We expect a View result, not a " + obj.GetType().Name);
            var model = result.Model as ProjectDetailsViewModel;
            model.Should().NotBeNull();
            model.Id.Should().Be(projects[2].Id);
        }

        [Fact]
        public async Task EditReturnsNotFoundForNonExistant()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(EditReturnsNotFoundForNonExistant));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.SaveChangesAsync();
        
            //Then
            var obj = await controller.Edit(projects[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect result, not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task EditReturnsViewResult()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(EditReturnsViewResult));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.SaveChangesAsync();
        
            //Then
            var obj = await controller.Edit(projects[2].Id);
            var result = obj as ViewResult;
            result.Should().NotBeNull(because: "We expect a View result, not a " + obj.GetType().Name);
            var model = result.Model as ProjectViewModel;
            model.Should().NotBeNull();
            model.Id.Should().Be(projects[2].Id);
        }
        
        [Fact]
        public async Task EditSaveReturnsViewResult()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(EditReturnsViewResult));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.SaveChangesAsync();
            var model = new ProjectViewModel
            {
                Title = "Update Title",
                CompletionStatus = CompletionStatus.Completed,
                Id = projects[2].Id
            };

            //Then
            
            var obj = await controller.Edit(projects[2].Id, model);
            var result = obj as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect result, not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            projects[2].Title.Should().Be(model.Title);
            projects[2].CompletionStatus.Should().Be((int)model.CompletionStatus);
        }

        [Fact]
        public async Task CompleteReturnsNotFoundForNonExistingProject()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CompleteReturnsNotFoundForNonExistingProject));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[2], ProjectId = projects[2].Id },
            };
            
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();
            
            //Then
            var obj = await controller.Complete(projects[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a NotFound result, not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task CompleteCompletesProjectAndChildTasks()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CompleteCompletesProjectAndChildTasks));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new ProjectController(db, mapper);

            //When            
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var projects = new []
            {
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new ProjectDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new ProjectDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[1], ProjectId = projects[1].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 0, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 50, Project = projects[2], ProjectId = projects[2].Id },
                new TaskDto { Id = Guid.NewGuid(), CompletionStatus = 100, Project = projects[2], ProjectId = projects[2].Id },
            };
            
            await db.Users.AddAsync(user);
            await db.Projects.AddRangeAsync(projects);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();
            
            //Then
            var obj = await controller.Complete(projects[2].Id);
            var result = obj as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect result, not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            projects[2].CompletionStatus.Should().Be((int)CompletionStatus.Completed);
            tasks.Where(t => t.ProjectId == projects[2].Id).All(t => t.CompletionStatus == (int)CompletionStatus.Completed).Should().BeTrue();
        }
    }
}
