using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using USPSimGame.Data.Models;

namespace USPSimGame.Components.Game;

public partial class MapFeaturePropertiesPanel : ComponentBase
{
    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public EventCallback<InspectedFeatureModel> OnFeaturePropertiesUpdated { get; set; }

    public bool IsOpen { get; set; } = false;
    public double TopPx { get; set; } = 120;
    public double LeftPx { get; set; } = 120;

    public List<InspectedFeatureModel> Candidates { get; set; } = new();
    public InspectedFeatureModel? SelectedFeature { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("uspsim2d5.makeElementDraggable", "map-feature-properties-panel");
            }
            catch { }
        }
    }

    public void Open(List<InspectedFeatureModel> candidateList, double clickX, double clickY)
    {
        if (candidateList == null || !candidateList.Any())
        {
            ClosePanel();
            return;
        }

        Candidates = candidateList;
        LeftPx = Math.Max(16, Math.Min(clickX + 15, 1200));
        TopPx = Math.Max(16, Math.Min(clickY + 15, 600));

        if (Candidates.Count == 1)
        {
            SelectedFeature = Candidates[0];
            InitCustomEntries(SelectedFeature);
        }
        else
        {
            SelectedFeature = null;
        }

        IsOpen = true;
        StateHasChanged();
    }

    public void ClosePanel()
    {
        IsOpen = false;
        SelectedFeature = null;
        Candidates.Clear();
        try
        {
            _ = JSRuntime.InvokeVoidAsync("uspsim2d5.clearFeatureHighlight");
        }
        catch { }
        StateHasChanged();
    }

    protected void SelectCandidate(InspectedFeatureModel candidate)
    {
        SelectedFeature = candidate;
        InitCustomEntries(SelectedFeature);
        try
        {
            _ = JSRuntime.InvokeVoidAsync("uspsim2d5.highlightInspectedFeature", candidate.LayerId, candidate.FeatureId);
        }
        catch { }
    }

    protected void BackToCandidateList()
    {
        SelectedFeature = null;
        try
        {
            _ = JSRuntime.InvokeVoidAsync("uspsim2d5.clearFeatureHighlight");
        }
        catch { }
    }

    private void InitCustomEntries(InspectedFeatureModel feature)
    {
        if (feature.IsEditable && !feature.CustomEntries.Any() && feature.Properties.Any())
        {
            feature.CustomEntries = feature.Properties.Select(kvp => new FeaturePropertyEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
        }
    }

    protected void AddVariableRow()
    {
        if (SelectedFeature != null)
        {
            SelectedFeature.CustomEntries.Add(new FeaturePropertyEntry { Key = string.Empty, Value = string.Empty });
            NotifyPropertiesUpdated();
        }
    }

    protected void RemoveVariableRow(FeaturePropertyEntry entry)
    {
        if (SelectedFeature != null)
        {
            SelectedFeature.CustomEntries.Remove(entry);
            NotifyPropertiesUpdated();
        }
    }

    protected void UpdateEntryKey(FeaturePropertyEntry entry, string? newKey)
    {
        entry.Key = newKey ?? string.Empty;
        NotifyPropertiesUpdated();
    }

    protected void UpdateEntryValue(FeaturePropertyEntry entry, string? newValue)
    {
        entry.Value = newValue ?? string.Empty;
        NotifyPropertiesUpdated();
    }

    private void NotifyPropertiesUpdated()
    {
        if (SelectedFeature != null && SelectedFeature.IsEditable)
        {
            // Sync entries dictionary
            SelectedFeature.Properties = SelectedFeature.CustomEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.Key))
                .ToDictionary(e => e.Key.Trim(), e => e.Value?.Trim() ?? string.Empty);

            var options = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = System.Text.Json.JsonSerializer.Serialize(SelectedFeature.Properties, options);

            try
            {
                _ = JSRuntime.InvokeVoidAsync("uspsim2d5.updateDraftFeatureProperties", SelectedFeature.LayerId, json);
            }
            catch { }

            OnFeaturePropertiesUpdated.InvokeAsync(SelectedFeature);
        }
    }

    protected void StartDrag()
    {
        // Drag functionality placeholder (panel position can be updated via mouse listeners)
    }
}
