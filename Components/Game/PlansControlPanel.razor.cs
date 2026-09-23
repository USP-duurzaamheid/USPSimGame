using Microsoft.AspNetCore.Components;
using USPSimGame.Data.Entities;
using USPSimGame.Data.Enums;
using USPSimGame.Services;
using USPSimGame.Services.Plans;

namespace USPSimGame.Components.Game;

public partial class PlansControlPanel : ComponentBase, IDisposable
{
    [Inject]
    public IPlanService PlanService { get; set; } = default!;

    [Inject]
    public IPlanNotifierService PlanNotifier { get; set; } = default!;

    [Parameter, EditorRequired]
    public int GameSessionId { get; set; }

    [Parameter]
    public int StartYear { get; set; } = 2026;

    [Parameter]
    public GameState GameState { get; set; } = GameState.Setup;

    [Parameter]
    public bool IsAdmin { get; set; }

    [Parameter]
    public int CurrentTeamId { get; set; }

    [Parameter]
    public Plan? ActivePlan { get; set; }

    [Parameter]
    public EventCallback OnOpenNewPlan { get; set; }

    [Parameter]
    public EventCallback<Plan?> OnPlanSelected { get; set; }

    [Parameter]
    public bool IsCollapsed { get; set; } = true;

    [Parameter]
    public EventCallback<bool> OnToggleCollapse { get; set; }

    protected List<Plan> Plans { get; set; } = new();
    protected bool IsLoading { get; set; } = true;

    protected Dictionary<PlanState, bool> ExpandedStateGroups { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        foreach (PlanState state in Enum.GetValues<PlanState>())
        {
            ExpandedStateGroups[state] = true;
        }

        PlanNotifier.OnPlansChanged += HandlePlansChangedAsync;
        PlanNotifier.OnPlanLockChanged += HandlePlanLockChangedAsync;
        await LoadPlansAsync();
    }

    private async Task HandlePlansChangedAsync(int sessionId)
    {
        if (sessionId == GameSessionId)
        {
            await RefreshPlansAsync();
        }
    }

    private async Task HandlePlanLockChangedAsync(int planId, int sessionId)
    {
        if (sessionId == GameSessionId)
        {
            await RefreshPlansAsync();
        }
    }

    public async Task RefreshPlansAsync()
    {
        await LoadPlansAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadPlansAsync()
    {
        IsLoading = true;
        try
        {
            Plans = await PlanService.GetSessionPlansAsync(GameSessionId, CurrentTeamId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PlansControlPanel] Error loading plans: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task ToggleCollapse()
    {
        IsCollapsed = !IsCollapsed;
        await OnToggleCollapse.InvokeAsync(IsCollapsed);
    }

    protected void ToggleStateGroup(PlanState state)
    {
        if (ExpandedStateGroups.ContainsKey(state))
        {
            ExpandedStateGroups[state] = !ExpandedStateGroups[state];
        }
        else
        {
            ExpandedStateGroups[state] = false;
        }
    }

    protected bool IsStateExpanded(PlanState state)
    {
        return !ExpandedStateGroups.TryGetValue(state, out bool expanded) || expanded;
    }

    protected async Task SelectPlanAsync(Plan plan)
    {
        if (ActivePlan?.Id == plan.Id)
        {
            await OnPlanSelected.InvokeAsync(null);
        }
        else
        {
            await OnPlanSelected.InvokeAsync(plan);
        }
    }

    protected async Task TriggerOpenNewPlanAsync()
    {
        await OnOpenNewPlan.InvokeAsync();
    }

    protected string FormatMonthYear(int startMonth)
    {
        return USPSimGame.Utils.CommonGameUtils.FormatMonthYear(startMonth, StartYear);
    }

    public void Dispose()
    {
        PlanNotifier.OnPlansChanged -= HandlePlansChangedAsync;
        PlanNotifier.OnPlanLockChanged -= HandlePlanLockChangedAsync;
    }
}
