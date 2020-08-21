using System;
using System.ComponentModel.DataAnnotations;

namespace Gtd.Web.Models
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public TaskCompletionStatus CompletionStatus { get; set; }

        public bool Important { get; set; }

        public bool Urgent { get; set; }
        
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? DueDate { get; set; }

        public string UserId { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }
    }

    public enum TaskCompletionStatus
    {
        [Display(Name = "Not Started")]
        NotStarted = 0,

        [Display(Name = "In Progress")]
        InProgress = 50,

        [Display(Name = "Completed")]
        Completed = 100,
    }
}
