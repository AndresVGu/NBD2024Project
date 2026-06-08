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
        public int TotalWorkOrders { get; set; }
        public int PendingWorkOrders { get; set; }
        public int InProgressWorkOrders { get; set; }
        public int CompletedWorkOrders { get; set; }

        public int TodayRouteStops { get; set; }
        public double TodayEstimatedHours { get; set; }
        public double TodayActualHours { get; set; }
        public double TodayHoursVariance { get; set; }

        public int EmployeesWithSkills { get; set; }
        public int TotalSkillAssignments { get; set; }

        public int ScopedClients { get; set; }
        public int ScopedProjects { get; set; }

        public int MyCreatedClients { get; set; }
        public int MyCreatedProjects { get; set; }
        public int MyCreatedBids { get; set; }

        public int ProjectsWithoutBids { get; set; }
        public int BidsThisMonth { get; set; }
        public int ReorderAlerts { get; set; }
        public List<ReorderAlertVM> ReorderAlertItems { get; set; } = new List<ReorderAlertVM>();

        public List<ActivityItemVM> RecentProjects { get; set; } = new List<ActivityItemVM>();
        public List<ActivityItemVM> RecentBids { get; set; } = new List<ActivityItemVM>();
        public List<ActivityItemVM> RecentClients { get; set; } = new List<ActivityItemVM>();
        public List<ActivityItemVM> RecentWorkOrders { get; set; } = new List<ActivityItemVM>();
        public List<SkillUsageItemVM> SkillUsage { get; set; } = new List<SkillUsageItemVM>();
        public List<StatusChartItemVM> WorkOrderStatusChart { get; set; } = new List<StatusChartItemVM>();
        public List<HoursTrendPointVM> WeeklyHoursTrend { get; set; } = new List<HoursTrendPointVM>();
    }

    public class ActivityItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }

    public class SkillUsageItemVM
    {
        public string Skill { get; set; }
        public int Count { get; set; }
    }

    public class StatusChartItemVM
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class HoursTrendPointVM
    {
        public string Label { get; set; }
        public double EstimatedHours { get; set; }
        public double ActualHours { get; set; }
    }

    public class ReorderAlertVM
    {
        public string Material { get; set; }
        public string Location { get; set; }
        public double OnHand { get; set; }
        public double Minimum { get; set; }
    }
}
