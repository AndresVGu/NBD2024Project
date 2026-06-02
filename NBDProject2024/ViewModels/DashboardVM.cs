using System;
using System.Collections.Generic;

namespace NBDProject2024.ViewModels
{
    public class DashboardVM
    {
        public bool IsAdmin { get; set; }
        public bool IsSupervisor { get; set; }
        public bool IsDesigner { get; set; }
        public bool IsSales { get; set; }

        public bool CanToggleScope { get; set; }
        public bool UseMyScope { get; set; }

        public int TotalClients { get; set; }
        public int TotalProjects { get; set; }
        public int TotalBids { get; set; }
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }

        public int ScopedClients { get; set; }
        public int ScopedProjects { get; set; }

        public int MyCreatedClients { get; set; }
        public int MyCreatedProjects { get; set; }
        public int MyCreatedBids { get; set; }

        public int ProjectsWithoutBids { get; set; }
        public int BidsThisMonth { get; set; }

        public List<ActivityItemVM> RecentProjects { get; set; } = new List<ActivityItemVM>();
        public List<ActivityItemVM> RecentBids { get; set; } = new List<ActivityItemVM>();
        public List<ActivityItemVM> RecentClients { get; set; } = new List<ActivityItemVM>();
    }

    public class ActivityItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
