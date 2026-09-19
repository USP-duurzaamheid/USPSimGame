# Backlog

1. Design a simulation plugin SDK when a second real simulator exists; replace EF entities in the contract with stable records.
2. Unsubscribe `MapControlPanel` from `OnPlansChanged` by implementing `IDisposable`.
3. Replace `TeamService` uses of `Directory.GetCurrentDirectory()` with an injected team-preset store based on the content root.
4. Remove the duplicate `ITeamBudgetService` resolution in `GameLoopBackgroundService`.
5. Implement or remove the missing `highlightInspectedFeature` JavaScript function.
6. Pass `targetMonth` to `loadSessionImplementedFeatures` consistently.
7. Introduce typed JavaScript interop and consider TypeScript/Vite for the map client.
8. Move the pure costing and plan-approval calculations into Domain.
9. Split `PlanAddEditPanel.razor.cs` after introducing an explicit draft-state model.
10. Consider the migration squash described in ADR 0002 only after team approval.
11. Promote `scratch/` to a supported tool project or remove it.
12. Remove the unused `IGameSessionService` injection from `Game.razor.cs`.

Known deployment issue: team preset reads currently depend on the process working directory even though the JSON files are copied to publish output. Fix this through the preset-store abstraction rather than another path special case.
