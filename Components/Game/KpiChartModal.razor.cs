using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Services.Plans;
using USPSimGame.Services.Simulation;

namespace USPSimGame.Components.Game;

public partial class KpiChartModal : ComponentBase
{
    [Inject]
    public IKpiChartDataService KpiChartDataService { get; set; } = default!;

    [Inject]
    public IPlanService PlanService { get; set; } = default!;

    [Parameter, EditorRequired]
    public int GameSessionId { get; set; }

    [Parameter]
    public int? ScopeTeamId { get; set; }

    [Parameter, EditorRequired]
    public int StartYear { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected List<KpiDatasetOption> AvailableDatasets { get; set; } = new();
    protected KpiDatasetOption? SelectedOption { get; set; }
    protected bool IsLoadingDatasets { get; set; } = true;
    protected bool IsLoadingSeries { get; set; } = false;
    protected bool HasData { get; set; } = false;
    protected bool ShowChart => SelectedOption != null && !IsLoadingSeries && HasData;
    protected bool HasPlanMarkers { get; set; } = false;

    private LineChart lineChart = default!;
    private LineChartOptions lineChartOptions = default!;
    private ChartData chartData = default!;
    private List<Plan> _implementedPlans = new();

    protected override async Task OnInitializedAsync()
    {
        lineChartOptions = new LineChartOptions { Responsive = true };
        lineChartOptions.Scales.X!.Title = new ChartAxesTitle { Text = "Month", Display = true };
        lineChartOptions.Scales.Y!.Title = new ChartAxesTitle { Text = string.Empty, Display = true };

        chartData = new ChartData { Labels = new List<string>(), Datasets = new List<IChartDataset>() };

        try
        {
            AvailableDatasets = await KpiChartDataService.GetAvailableDatasetsAsync(GameSessionId, ScopeTeamId);

            var sessionPlans = await PlanService.GetSessionPlansAsync(GameSessionId);
            _implementedPlans = sessionPlans.Where(p => p.State == PlanState.Implemented).ToList();
        }
        finally
        {
            IsLoadingDatasets = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await lineChart.InitializeAsync(chartData, lineChartOptions);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected bool IsSelected(KpiDatasetOption option) =>
        SelectedOption != null
        && SelectedOption.SimulatorKey == option.SimulatorKey
        && SelectedOption.KpiName == option.KpiName
        && SelectedOption.TeamId == option.TeamId;

    protected async Task SelectDatasetAsync(KpiDatasetOption option)
    {
        SelectedOption = option;
        IsLoadingSeries = true;
        HasData = false;
        StateHasChanged();

        var points = await KpiChartDataService.GetTimeSeriesAsync(GameSessionId, option.SimulatorKey, option.KpiName, option.TeamId, StartYear);

        IsLoadingSeries = false;
        HasData = points.Count > 0;

        if (HasData)
        {
            var datasets = new List<IChartDataset>
            {
                new LineChartDataset
                {
                    Label = option.DisplayLabel,
                    Data = points.Select(p => (double?)p.Value).ToList(),
                    BorderColor = "#0d6efd",
                    BackgroundColor = "rgba(13, 110, 253, 0.15)",
                    BorderWidth = 2,
                    PointRadius = new List<double> { 3 }
                }
            };

            // Overlay one marker dataset per implemented plan whose start month falls within this
            // KPI's recorded months, so hovering a marker shows the plan name (and team) via the
            // chart's native tooltip - no custom JS/annotation plugin required.
            var plansInRange = _implementedPlans
                .Where(plan => points.Any(p => p.SimulatedMonth == plan.StartMonth))
                .ToList();

            foreach (var plan in plansInRange)
            {
                var markerData = points
                    .Select(p => p.SimulatedMonth == plan.StartMonth ? (double?)p.Value : null)
                    .ToList();

                datasets.Add(new LineChartDataset
                {
                    Label = $"{plan.Name} ({plan.Team?.Name ?? "Unknown team"})",
                    Data = markerData,
                    BorderWidth = 0,
                    Fill = false,
                    PointRadius = new List<double> { 7 },
                    PointHoverRadius = new List<double> { 9 },
                    PointBackgroundColor = new List<string> { "#dc3545" }
                });
            }

            HasPlanMarkers = plansInRange.Count > 0;

            chartData = new ChartData
            {
                Labels = points.Select(p => p.Label).ToList(),
                Datasets = datasets
            };

            lineChartOptions.Scales.Y!.Title = new ChartAxesTitle { Text = option.Unit, Display = true };

            await lineChart.UpdateAsync(chartData, lineChartOptions);
        }

        StateHasChanged();
    }

    protected async Task CloseAsync() => await OnClose.InvokeAsync();
}

