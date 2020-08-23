using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Gtd.Web
{
    public static class AutoMapperConfiguration
    {
        public static void Configuration(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Models.TaskViewModel, Data.TaskDto>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => (int)s.CompletionStatus));
            cfg.CreateMap<Data.TaskDto, Models.TaskViewModel>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => s.CompletionStatus));
            
            cfg.CreateMap<Models.ProjectViewModel, Data.ProjectDto>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => (int)s.CompletionStatus));
            cfg.CreateMap<Data.ProjectDto, Models.ProjectViewModel>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => s.CompletionStatus))
                                                                     .ForMember(d => d.OutstandingTasks, op => op.MapFrom(s => s.Tasks.Count(t => t.CompletionStatus < 100)));
            cfg.CreateMap<Models.ProjectDetailsViewModel, Data.ProjectDto>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => (int)s.CompletionStatus));
            cfg.CreateMap<Data.ProjectDto, Models.ProjectDetailsViewModel>().ForMember(d => d.CompletionStatus, op => op.MapFrom(s => s.CompletionStatus));

        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(Configuration);
            services.AddSingleton(config.CreateMapper());
        }
    }
}
