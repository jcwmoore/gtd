using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gtd.Web
{
    public static class AutoMapperConfiguration
    {
        public static void Configuration(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Models.TaskViewModel, Data.TaskDto>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => (int)s.CompletionStatus));
            cfg.CreateMap<Data.TaskDto, Models.TaskViewModel>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => s.CompletionStatus));
        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(Configuration);
            services.AddSingleton(config.CreateMapper());
        }
    }
}
