using System;
using System.Collections.Generic;

namespace Gtd.Web.Models
{
    public class ProjectViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Title { get; set; }

        public int OutstandingTasks { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }
        
        public CompletionStatus CompletionStatus { get; set; }
    }

    public class ProjectDetailsViewModel : ProjectViewModel
    {
        public ICollection<TaskViewModel> Tasks { get; set; }
    }
}
