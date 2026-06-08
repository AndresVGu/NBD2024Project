namespace NBDProject2024.ViewModels
{
    public class RoutePlanViewModel
    {
        public DateTime Date { get; set; }
        public List<RouteStopViewModel> Stops { get; set; } = new List<RouteStopViewModel>();
    }

    public class RouteStopViewModel
    {
        public int Sequence { get; set; }
        public int WorkOrderID { get; set; }
        public string WorkOrderTitle { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string CityName { get; set; }
        public string ProjectSite { get; set; }
        public double DistanceFromPreviousKm { get; set; }
        public double CumulativeDistanceKm { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}
