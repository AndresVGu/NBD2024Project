(function () {
  var salesChart;
  var trafficChart;

  var dashboardPage = document.querySelector(".dashboard-page");
  var sidebarToggleBtn = document.querySelector("#sidebarToggleBtn");
  var mobileSidebarElement = document.querySelector("#mobileSidebar");
  if (dashboardPage && sidebarToggleBtn) {
    sidebarToggleBtn.addEventListener("click", function () {
      if (
        window.innerWidth < 992 &&
        mobileSidebarElement &&
        window.bootstrap &&
        window.bootstrap.Offcanvas
      ) {
        var sidebar =
          window.bootstrap.Offcanvas.getOrCreateInstance(mobileSidebarElement);
        sidebar.toggle();
      } else if (window.innerWidth >= 992) {
        dashboardPage.classList.toggle("sidebar-collapsed");
      }
    });
  }

  function isDarkTheme() {
    return document.documentElement.getAttribute("data-theme") === "dark";
  }

  function getSalesOptions() {
    var dark = isDarkTheme();
    return {
      chart: {
        type: "line",
        height: 400,
        toolbar: { show: false },
        fontFamily: "Inter, Segoe UI, sans-serif",
      },
      series: [
        {
          name: "Income",
          type: "column",
          data: [180, 220, 170, 210, 260, 300, 280, 250, 320, 340, 310, 360],
        },
        {
          name: "Forecast",
          type: "line",
          data: [140, 170, 160, 180, 190, 210, 205, 215, 220, 230, 225, 240],
        },
        {
          name: "Actual",
          type: "line",
          data: [120, 150, 135, 175, 230, 280, 260, 235, 295, 315, 300, 330],
        },
      ],
      stroke: {
        width: [0, 3, 3],
        curve: "smooth",
      },
      colors: ["#0d6efd", "#9aa3b2", "#ff9f43"],
      plotOptions: {
        bar: {
          columnWidth: "45%",
          borderRadius: 6,
        },
      },
      xaxis: {
        labels: {
          style: {
            colors: dark ? "#9fb0cb" : "#667a99",
          },
        },
        categories: [
          "Jan",
          "Feb",
          "Mar",
          "Apr",
          "May",
          "Jun",
          "Jul",
          "Aug",
          "Sep",
          "Oct",
          "Nov",
          "Dec",
        ],
      },
      yaxis: {
        labels: {
          style: {
            colors: dark ? "#9fb0cb" : "#667a99",
          },
          formatter: function (val) {
            return "$" + val;
          },
        },
      },
      tooltip: {
        theme: dark ? "dark" : "light",
      },
      dataLabels: { enabled: false },
      legend: {
        position: "top",
        horizontalAlign: "right",
        labels: {
          colors: dark ? "#cfdcf5" : "#3d4d67",
        },
      },
      grid: {
        borderColor: dark ? "#2f3f5f" : "#edf1f7",
        strokeDashArray: 4,
      },
    };
  }

  function getTrafficOptions() {
    var dark = isDarkTheme();
    return {
      chart: {
        type: "donut",
        height: 210,
        fontFamily: "Inter, Segoe UI, sans-serif",
      },
      labels: ["Direct", "Organic Search", "Social Media", "Referrals"],
      series: [42, 28, 18, 12],
      colors: ["#0d6efd", "#6ea8fe", "#ff9f43", "#9aa3b2"],
      dataLabels: {
        enabled: false,
      },
      legend: {
        show: false,
      },
      stroke: {
        width: 0,
      },
      tooltip: {
        theme: dark ? "dark" : "light",
      },
      plotOptions: {
        pie: {
          donut: {
            size: "72%",
          },
        },
      },
    };
  }

  function renderSalesChart() {
    var salesChartElement = document.querySelector("#salesAnalyticsChart");
    if (!salesChartElement) {
      return;
    }

    if (salesChart) {
      salesChart.destroy();
    }

    salesChart = new ApexCharts(salesChartElement, getSalesOptions());
    salesChart.render();
  }

  function renderTrafficChart() {
    var trafficChartElement = document.querySelector("#trafficSourcesChart");
    if (!trafficChartElement) {
      return;
    }

    if (trafficChart) {
      trafficChart.destroy();
    }

    trafficChart = new ApexCharts(trafficChartElement, getTrafficOptions());
    trafficChart.render();
  }

  renderSalesChart();
  renderTrafficChart();

  document.addEventListener("nbd:themeChanged", function () {
    renderSalesChart();
    renderTrafficChart();
  });
})();
