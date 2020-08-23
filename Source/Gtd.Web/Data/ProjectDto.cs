using System;
using System.Collections.Generic;

namespace Gtd.Web.Data
{
    public class ProjectDto
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Title { get; set; }
        
        public string UserId { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }

        public ICollection<TaskDto> Tasks { get; set; }
        
        public int CompletionStatus { get; set; }
    }
}
