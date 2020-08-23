using System;
using System.Security.Claims;
using System.Security.Principal;
using AutoMapper;
using Gtd.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Gtd.Web.Controllers.Tests
{
    public class ControllerTestBase
    {
        protected static readonly string USER_NAME = "test user";
        protected static readonly string USER_ID = Guid.NewGuid().ToString();

        protected ApplicationDbContext GenerateInMemDbContext(string method)
        {
            DbContextOptions<ApplicationDbContext> options;
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(string.Format("{0}-{1}", this.GetType().Name, method));
            options = builder.Options;
            var dataContext = new ApplicationDbContext(options);
            dataContext.Database.EnsureDeleted();
            dataContext.Database.EnsureCreated();
            return dataContext;
        }

        protected ControllerContext GenerateFakeControllerContext()
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

        protected IMapper GetMapper()
        {
            return new MapperConfiguration(AutoMapperConfiguration.Configuration).CreateMapper();
        }
    }
}
