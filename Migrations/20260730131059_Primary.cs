using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace USPSimGame.Migrations
{
    /// <inheritdoc />
    public partial class Primary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CenterLatLong = table.Column<string>(type: "text", nullable: false),
                    Zoom = table.Column<int>(type: "integer", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    CurrentMonth = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    MonthDurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    TargetMonthEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemainingSecondsOnPause = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MapLayerDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LayerType = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    IsEnabledByDefault = table.Column<bool>(type: "boolean", nullable: false),
                    TranslatorTags = table.Column<string>(type: "text", nullable: true),
                    SimulatorTags = table.Column<string>(type: "text", nullable: true),
                    DefaultStyleConfigJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapLayerDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannableLayerDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: false),
                    GeometryType = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    DefaultColor = table.Column<string>(type: "text", nullable: true),
                    DefaultLineWidthPx = table.Column<double>(type: "double precision", nullable: true),
                    TranslatorTags = table.Column<string>(type: "text", nullable: true),
                    SimulatorTags = table.Column<string>(type: "text", nullable: true),
                    IsEnabledByDefault = table.Column<bool>(type: "boolean", nullable: false),
                    BaseInvestmentPoints = table.Column<double>(type: "double precision", nullable: false),
                    InvestmentPointsPerUnit = table.Column<double>(type: "double precision", nullable: false),
                    BaseMonthlyExpensePoints = table.Column<double>(type: "double precision", nullable: false),
                    MonthlyExpensePointsPerUnit = table.Column<double>(type: "double precision", nullable: false),
                    BaseConstructionTimeMonths = table.Column<int>(type: "integer", nullable: false),
                    ConstructionTimeModifierPerUnit = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannableLayerDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActive = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulationModuleDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SimulatorType = table.Column<int>(type: "integer", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "integer", nullable: false),
                    EndpointUrlOrPath = table.Column<string>(type: "text", nullable: true),
                    RequiredTags = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationModuleDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulationMapOutputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    SimulatedMonth = table.Column<int>(type: "integer", nullable: false),
                    SimulatorKey = table.Column<string>(type: "text", nullable: false),
                    LayerName = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    GeoJsonOrImageData = table.Column<string>(type: "text", nullable: false),
                    BoundingBoxJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationMapOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationMapOutputs_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    AreaDefinition = table.Column<string>(type: "text", nullable: true),
                    LockedBySessionId = table.Column<string>(type: "text", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestmentPointsBalance = table.Column<double>(type: "double precision", nullable: false),
                    AnnualBudgetAllowance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionMapLayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    MapLayerDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CachedDataContent = table.Column<string>(type: "text", nullable: true),
                    TranslatorTags = table.Column<string>(type: "text", nullable: true),
                    SimulatorTags = table.Column<string>(type: "text", nullable: true),
                    LastFetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionMapLayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionMapLayers_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSessionMapLayers_MapLayerDefinitions_MapLayerDefinition~",
                        column: x => x.MapLayerDefinitionId,
                        principalTable: "MapLayerDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionPlannableLayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    PlannableLayerDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionPlannableLayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionPlannableLayers_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSessionPlannableLayers_PlannableLayerDefinitions_Planna~",
                        column: x => x.PlannableLayerDefinitionId,
                        principalTable: "PlannableLayerDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartMonth = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    TotalCalculatedInvestmentPoints = table.Column<double>(type: "double precision", nullable: false),
                    TotalCalculatedMonthlyExpensePoints = table.Column<double>(type: "double precision", nullable: false),
                    ExpenseDurationMonths = table.Column<int>(type: "integer", nullable: false),
                    LockedBySessionId = table.Column<string>(type: "text", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimulationKpiOutputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    SimulatedMonth = table.Column<int>(type: "integer", nullable: false),
                    SimulatorKey = table.Column<string>(type: "text", nullable: false),
                    KpiName = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationKpiOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationKpiOutputs_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SimulationKpiOutputs_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    GameSessionPlannableLayerId = table.Column<int>(type: "integer", nullable: false),
                    TargetFeatureId = table.Column<string>(type: "text", nullable: true),
                    GeoJsonGeometry = table.Column<string>(type: "text", nullable: true),
                    PropertiesJson = table.Column<string>(type: "text", nullable: true),
                    IsDemolition = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_GameSessionPlannableLayers_GameSessionPlannabl~",
                        column: x => x.GameSessionPlannableLayerId,
                        principalTable: "GameSessionPlannableLayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanTeamJudgments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Judgment = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanTeamJudgments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanTeamJudgments_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanTeamJudgments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MapLayerDefinitions",
                columns: new[] { "Id", "Category", "DefaultStyleConfigJson", "Description", "IsEnabledByDefault", "Key", "LayerType", "Name", "SimulatorTags", "TranslatorTags" },
                values: new object[,]
                {
                    { 1, "Buildings", null, "3D building footprints, roof shapes, heights, and volumes from 3D BAG (Kadaster/TU Delft).", true, "pdok-3dbag-buildings", "VectorGeoJson", "3D BAG Buildings", null, null },
                    { 2, "Infrastructure", null, "Low-, medium-, and high-voltage electricity grid network for Liander service territory.", false, "liander-open-data-elektra", "VectorGeoJson", "Liander Electricity Grid", null, null },
                    { 3, "Infrastructure", null, "Complete low-, medium-, and high-voltage electricity cables and transformer stations for Stedin service territory.", false, "stedin-open-data-elektra", "VectorGeoJson", "Stedin Regional Electricity Grid", null, null },
                    { 4, "Infrastructure", null, "Municipal urban water, sewage pipes, inspection manholes, and pumping stations from Stichting Rioned GWSW.", false, "pdok-gwsw-sewage", "VectorGeoJson", "Urban Sewage & Drainage Network (PDOK GWSW)", null, null },
                    { 5, "Environment", null, "Official municipal boundaries and administrative jurisdiction borders derived from Kadaster BRK.", false, "pdok-brk-bestuurlijkegebieden", "VectorGeoJson", "Municipal Boundaries (BRK Bestuurlijke Gebieden)", null, null },
                    { 6, "Environment", null, "Official cadastral land parcel boundaries, plot sizes, section codes, and parcel numbers from Kadaster BRK.", false, "pdok-brk-kadastralekaart", "VectorGeoJson", "Cadastral Parcels (BRK Kadastrale Kaart WFS)", null, null }
                });

            migrationBuilder.InsertData(
                table: "PlannableLayerDefinitions",
                columns: new[] { "Id", "BaseConstructionTimeMonths", "BaseInvestmentPoints", "BaseMonthlyExpensePoints", "Category", "ConstructionTimeModifierPerUnit", "DefaultColor", "DefaultLineWidthPx", "Description", "GeometryType", "Icon", "InvestmentPointsPerUnit", "IsEnabledByDefault", "Key", "MonthlyExpensePointsPerUnit", "Name", "SimulatorTags", "TranslatorTags" },
                values: new object[,]
                {
                    { 1, 6, 50.0, 2.0, "Infrastructure", 0.0001, "#f59e0b", 2.5, "Zoned land polygon area designated for ground-mounted solar PV development.", "Polygon", "bi-sun-fill", 30.0, true, "solar-farm", 1.0, "Solar Farm Area", null, null },
                    { 2, 12, 100.0, 5.0, "Infrastructure", 0.00020000000000000001, "#06b6d4", 2.5, "Zoned land polygon area designated for onshore wind turbine installations.", "Polygon", "bi-wind", 50.0, true, "wind-farm", 2.0, "Wind Farm Zone", null, null },
                    { 3, 1, 10.0, 1.0, "Infrastructure", 0.5, "#10b981", 2.0, "Public or commercial electric vehicle charging station hub point location.", "Point", "bi-ev-station-fill", 30.0, true, "ev-charger-hub", 0.5, "EV Charging Station Hub", null, null },
                    { 4, 2, 15.0, 0.5, "Infrastructure", 0.0050000000000000001, "#3b82f6", 3.5, "High, medium, or low voltage power transmission or distribution line.", "Line", "bi-lightning-charge-fill", 20.0, true, "power-cable", 0.5, "Electricity Connection Cable", null, null },
                    { 5, 4, 40.0, 3.0, "Infrastructure", 1.0, "#ef4444", 2.0, "Electrical grid transformer station or substations for voltage step-down/step-up.", "Point", "bi-box-seam", 40.0, true, "transformer-substation", 1.0, "Transformer Substation", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHash", "Username" },
                values: new object[] { 1, "harald.warmelink@hu.nl", "AQAAAAIAAYagAAAAEKstQTVmO/0bmR5/P2B+mTIYP9Ju76yHdGFRYt7uq9Im2XkmV3pwZpvDAMTmlgzY3w==", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionMapLayers_GameSessionId",
                table: "GameSessionMapLayers",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionMapLayers_MapLayerDefinitionId",
                table: "GameSessionMapLayers",
                column: "MapLayerDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlannableLayers_GameSessionId",
                table: "GameSessionPlannableLayers",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionPlannableLayers_PlannableLayerDefinitionId",
                table: "GameSessionPlannableLayers",
                column: "PlannableLayerDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_GameSessionPlannableLayerId",
                table: "PlanFeatures",
                column: "GameSessionPlannableLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId",
                table: "PlanFeatures",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_GameSessionId",
                table: "Plans",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_TeamId",
                table: "Plans",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanTeamJudgments_PlanId",
                table: "PlanTeamJudgments",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanTeamJudgments_TeamId",
                table: "PlanTeamJudgments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationKpiOutputs_GameSessionId",
                table: "SimulationKpiOutputs",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationKpiOutputs_TeamId",
                table: "SimulationKpiOutputs",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationMapOutputs_GameSessionId",
                table: "SimulationMapOutputs",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_GameSessionId",
                table: "Teams",
                column: "GameSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSessionMapLayers");

            migrationBuilder.DropTable(
                name: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "PlanTeamJudgments");

            migrationBuilder.DropTable(
                name: "PlayerSessions");

            migrationBuilder.DropTable(
                name: "SimulationKpiOutputs");

            migrationBuilder.DropTable(
                name: "SimulationMapOutputs");

            migrationBuilder.DropTable(
                name: "SimulationModuleDefinitions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MapLayerDefinitions");

            migrationBuilder.DropTable(
                name: "GameSessionPlannableLayers");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "PlannableLayerDefinitions");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
