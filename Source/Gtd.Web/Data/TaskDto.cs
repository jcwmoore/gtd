using System;

namespace Gtd.Web.Data
{
    public class TaskDto
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public int CompletionStatus { get; set; }

        public bool Important { get; set; }

        public bool Urgent { get; set; }
        
        public DateTime? DueDate { get; set; }

        public Guid? ProjectId { get; set; }

        public ProjectDto Project { get; set; }
        
        public string UserId { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }

        public double Sort { get; set; }
    }
}
