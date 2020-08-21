using System;
using Xunit;
using Gtd.Web.Data;
using Gtd.Web.Models;
using Gtd.Web.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

namespace Gtd.Web.Tests.Controllers
{
    public class TaskControllerTests
    {
        private static readonly string USER_NAME = "test user";
        private static readonly string USER_ID = Guid.NewGuid().ToString();

        [Fact]
        public async Task IndexOnlyIncludesMyUncompletedTasks()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(IndexOnlyIncludesMyUncompletedTasks));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Index();
            var result = (obj) as ViewResult;
            result.Should().NotBeNull(because: "We expect a ViewResult not a " + obj.GetType().Name);
            var model = result.Model as IEnumerable<TaskViewModel>;
            model.Should().NotBeNull();
            model.Count().Should().Be(2);
            model.All(tm => tm.CompletionStatus != TaskCompletionStatus.Completed).Should().BeTrue();
            model.All(tm => tm.UserId == USER_ID).Should().BeTrue();
        }

        [Fact]
        public async Task DetailsReturnsNotFoundForNonexistantRecord()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DetailsReturnsNotFoundForNonexistantRecord));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Details(Guid.NewGuid());
            var result = (obj) as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a NotFoundResult not a " + obj.GetType().Name);
        }


        [Fact]
        public async Task DetailsReturnsNotFoundForNonUsersRecord()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DetailsReturnsNotFoundForNonUsersRecord));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Details(tasks[3].Id);
            var result = (obj) as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a NotFoundResult not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task CreateTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CreateTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Create();
            var result = (obj) as ViewResult;
            result.Should().NotBeNull(because: "We expect a ViewResult not a " + obj.GetType().Name);
            controller.ViewData["UserId"].Should().Be(USER_ID);
        }

        [Fact]
        public async Task CreateSaveTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CreateSaveTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            var model = new TaskViewModel
            {
                CompletionStatus = TaskCompletionStatus.InProgress,
                Title = "my task",
                DueDate = DateTime.Today.AddDays(5),                
            };

            //Then
            var obj = await controller.Create(model);
            var result = (obj) as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a ViewResult not a " + obj.GetType().Name);
            var task = await db.Tasks.SingleOrDefaultAsync();
            task.Should().NotBeNull();
            task.Created.Should().BeCloseTo(DateTime.UtcNow, 50);
            task.Updated.Should().BeCloseTo(DateTime.UtcNow, 50);
            task.User.Should().Be(user);
            task.CompletionStatus.Should().Be((int)model.CompletionStatus);
            task.Title.Should().Be(model.Title);
            task.DueDate.Should().Be(model.DueDate);
        }

        [Fact]
        public async Task EditNonExistingTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DetailsReturnsNotFoundForNonUsersRecord));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Edit(tasks[3].Id);
            var result = (obj) as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a NotFoundResult not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task EditSuccessTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(EditSuccessTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Edit(tasks[2].Id);
            var result = obj as ViewResult;
            result.Should().NotBeNull(because: "We expect a ViewResult not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task CompleteNonExistingTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CompleteNonExistingTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Complete(tasks[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            
        }

        [Fact]
        public async Task CompleteSuccessTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CompleteSuccessTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Complete(tasks[2].Id);
            var result = obj as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            tasks[2].CompletionStatus.Should().Be((int)TaskCompletionStatus.Completed);
        }

        [Fact]
        public async Task EditSaveNonExistingTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(CompleteNonExistingTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            await Task.Delay(500);
            var model = new TaskViewModel
            {
                Id = tasks[3].Id,
                CompletionStatus = TaskCompletionStatus.InProgress,
                Description = "TEST DESCRIPTION",
                Title = "New Title",
                Urgent = true,
                Important = true
            };
            var obj = await controller.Edit(tasks[3].Id, model);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            
        }

        [Fact]
        public async Task EditSaveSuccessTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(EditSaveSuccessTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            await Task.Delay(500);
            var model = new TaskViewModel
            {
                Id = tasks[2].Id,
                CompletionStatus = TaskCompletionStatus.InProgress,
                Description = "TEST DESCRIPTION",
                Title = "New Title",
                Urgent = true,
                Important = true
            };
            var obj = await controller.Edit(tasks[2].Id, model);
            var result = obj as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            tasks[2].CompletionStatus.Should().Be((int)model.CompletionStatus);
            tasks[2].Title.Should().Be(model.Title);
            tasks[2].Description.Should().Be(model.Description);
            tasks[2].Urgent.Should().Be(model.Important);
            tasks[2].Important.Should().Be(model.Important);
            tasks[2].Updated.Should().BeCloseTo(DateTime.UtcNow, 50);
            tasks[2].Updated.Should().NotBeCloseTo(tasks[2].Created);
        }

        [Fact]
        public async Task DeleteNonExistingTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DeleteNonExistingTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Delete(tasks[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            
        }

        [Fact]
        public async Task DeleteSuccessTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DeleteSuccessTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.Delete(tasks[2].Id);
            var result = obj as ViewResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
        }

        [Fact]
        public async Task DeleteConfirmedNonExistingTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DeleteConfirmedNonExistingTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then
            var obj = await controller.DeleteConfirmed(tasks[3].Id);
            var result = obj as NotFoundResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            
        }

        [Fact]
        public async Task DeleteConfirmedSaveSuccessTest()
        {
            //Given
            var db = GenerateInMemDbContext(nameof(DeleteConfirmedSaveSuccessTest));
            var cctx = GenerateFakeControllerContext();
            var mapper = GetMapper();
            var controller = new TaskController(db, mapper);

            //When
            controller.ControllerContext = cctx;
            var user = new IdentityUser();
            user.Id = USER_ID;
            user.Email = USER_NAME;
            var other = new IdentityUser();
            other.Id = Guid.NewGuid().ToString();
            other.Email = "someone else";
            var tasks = new []
            {
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 100 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 50 },
                new TaskDto { Id = Guid.NewGuid(), User = user, UserId = USER_ID, CompletionStatus = 0 },
                new TaskDto { Id = Guid.NewGuid(), User = other, UserId = other.Id, CompletionStatus = 0 },
            };
            await db.Users.AddAsync(user);
            await db.Tasks.AddRangeAsync(tasks);
            await db.SaveChangesAsync();

            //Then         
            var obj = await controller.DeleteConfirmed(tasks[2].Id);
            var result = obj as RedirectToActionResult;
            result.Should().NotBeNull(because: "We expect a Redirect not a " + obj.GetType().Name);
            result.ActionName.Should().Be("Index");
            var missing = await db.Tasks.FirstOrDefaultAsync(t => t.Id == tasks[2].Id);
            missing.Should().BeNull();
        }

        private ApplicationDbContext GenerateInMemDbContext(string method)
        {
            DbContextOptions<ApplicationDbContext> options;
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(string.Format("{0}-{1}", nameof(TaskControllerTests), method));
            options = builder.Options;
            var dataContext = new ApplicationDbContext(options);
            dataContext.Database.EnsureDeleted();
            dataContext.Database.EnsureCreated();
            return dataContext;
        }

        private ControllerContext GenerateFakeControllerContext()
        {
            var ctx = new ControllerContext();
            var hctx = Mock.Of<HttpContext>();
            ctx.HttpContext = hctx;
            var cp = Mock.Of<ClaimsPrincipal>();
            var iden = Mock.Of<IIdentity>();
            Mock.Get(iden).Setup(m => m.Name).Returns(USER_NAME);
            Mock.Get(cp).Setup(m => m.Identity).Returns(iden);
            Mock.Get(hctx).Setup(m => m.User).Returns(cp);            
            return ctx;
        }

        private IMapper GetMapper()
        {
            return new MapperConfiguration(AutoMapperConfiguration.Configuration).CreateMapper();
        }
    }
}
