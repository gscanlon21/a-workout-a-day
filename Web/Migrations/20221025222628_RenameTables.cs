using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    public partial class RenameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.Id);
                },
                comment: "Equipment used in an exercise");

            migrationBuilder.CreateTable(
                name: "exercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Proficiency = table.Column<int>(type: "integer", nullable: false),
                    Muscles = table.Column<int>(type: "integer", nullable: false),
                    IsRecovery = table.Column<bool>(type: "boolean", nullable: false),
                    SportsFocus = table.Column<int>(type: "integer", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise", x => x.Id);
                },
                comment: "Exercises listed on the website");

            migrationBuilder.CreateTable(
                name: "footnote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_footnote", x => x.Id);
                },
                comment: "Sage advice");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    AcceptedTerms = table.Column<bool>(type: "boolean", nullable: false),
                    PrefersWeights = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeBonus = table.Column<bool>(type: "boolean", nullable: false),
                    RecoveryMuscle = table.Column<int>(type: "integer", nullable: false),
                    SportsFocus = table.Column<int>(type: "integer", nullable: false),
                    RestDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StrengtheningPreference = table.Column<int>(type: "integer", nullable: false),
                    EmailVerbosity = table.Column<int>(type: "integer", nullable: false),
                    LastActive = table.Column<DateOnly>(type: "date", nullable: true),
                    DisabledReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                },
                comment: "User who signed up for the newsletter");

            migrationBuilder.CreateTable(
                name: "variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    ExerciseType = table.Column<int>(type: "integer", nullable: false),
                    MuscleContractions = table.Column<int>(type: "integer", nullable: false),
                    PrimaryMuscles = table.Column<int>(type: "integer", nullable: false),
                    SecondaryMuscles = table.Column<int>(type: "integer", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variation", x => x.Id);
                },
                comment: "Variations of exercises");

            migrationBuilder.CreateTable(
                name: "exercise_prerequisite",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    PrerequisiteExerciseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_prerequisite", x => new { x.ExerciseId, x.PrerequisiteExerciseId });
                    table.ForeignKey(
                        name: "FK_exercise_prerequisite_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_prerequisite_exercise_PrerequisiteExerciseId",
                        column: x => x.PrerequisiteExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Pre-requisite exercises for other exercises");

            migrationBuilder.CreateTable(
                name: "newsletter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    NewsletterRotation_Id = table.Column<int>(type: "integer", nullable: false),
                    NewsletterRotation_ExerciseType = table.Column<int>(type: "integer", nullable: false),
                    NewsletterRotation_IntensityLevel = table.Column<int>(type: "integer", nullable: false),
                    NewsletterRotation_MuscleGroups = table.Column<int>(type: "integer", nullable: false),
                    IsDeloadWeek = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_newsletter_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateTable(
                name: "user_equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_equipment", x => new { x.UserId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_user_equipment_equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_equipment_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_exercise",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Progression = table.Column<int>(type: "integer", nullable: false),
                    Ignore = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeen = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exercise", x => new { x.UserId, x.ExerciseId });
                    table.ForeignKey(
                        name: "FK_user_exercise_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_exercise_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's progression level of an exercise");

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Expires = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => new { x.UserId, x.Token });
                    table.ForeignKey(
                        name: "FK_user_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth tokens for a user");

            migrationBuilder.CreateTable(
                name: "equipment_group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    IsWeight = table.Column<bool>(type: "boolean", nullable: false),
                    Instruction = table.Column<string>(type: "text", nullable: true),
                    DisabledReason = table.Column<string>(type: "text", nullable: true),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_equipment_group_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Equipment that can be switched out for one another");

            migrationBuilder.CreateTable(
                name: "exercise_variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Progression_Min = table.Column<int>(type: "integer", nullable: true),
                    Progression_Max = table.Column<int>(type: "integer", nullable: true),
                    IsBonus = table.Column<bool>(type: "boolean", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_variation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_variation_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_variation_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Variation progressions for an exercise track");

            migrationBuilder.CreateTable(
                name: "intensity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisabledReason = table.Column<string>(type: "text", nullable: true),
                    Proficiency_Secs = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_MinReps = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_MaxReps = table.Column<int>(type: "integer", nullable: true),
                    Proficiency_Sets = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false),
                    IntensityLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intensity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_intensity_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Intensity level of an exercise variation per user's strengthing preference");

            migrationBuilder.CreateTable(
                name: "user_variation",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false),
                    LastSeen = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_variation", x => new { x.UserId, x.VariationId });
                    table.ForeignKey(
                        name: "FK_user_variation_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_variation_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's intensity stats");

            migrationBuilder.CreateTable(
                name: "equipment_group_equipment",
                columns: table => new
                {
                    EquipmentGroupsId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_group_equipment", x => new { x.EquipmentGroupsId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_equipment_group_equipment_equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_equipment_group_equipment_equipment_group_EquipmentGroupsId",
                        column: x => x.EquipmentGroupsId,
                        principalTable: "equipment_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_exercise_variation",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExerciseVariationId = table.Column<int>(type: "integer", nullable: false),
                    LastSeen = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exercise_variation", x => new { x.UserId, x.ExerciseVariationId });
                    table.ForeignKey(
                        name: "FK_user_exercise_variation_exercise_variation_ExerciseVariatio~",
                        column: x => x.ExerciseVariationId,
                        principalTable: "exercise_variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_exercise_variation_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User's progression level of an exercise variation");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_VariationId",
                table: "equipment_group",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_group_equipment_EquipmentId",
                table: "equipment_group_equipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_prerequisite_PrerequisiteExerciseId",
                table: "exercise_prerequisite",
                column: "PrerequisiteExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_variation_ExerciseId_VariationId",
                table: "exercise_variation",
                columns: new[] { "ExerciseId", "VariationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exercise_variation_VariationId",
                table: "exercise_variation",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_intensity_VariationId",
                table: "intensity",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_newsletter_UserId",
                table: "newsletter",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_equipment_EquipmentId",
                table: "user_equipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_ExerciseId",
                table: "user_exercise",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_variation_ExerciseVariationId",
                table: "user_exercise_variation",
                column: "ExerciseVariationId");

            migrationBuilder.CreateIndex(
                name: "IX_user_variation_VariationId",
                table: "user_variation",
                column: "VariationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_group_equipment");

            migrationBuilder.DropTable(
                name: "exercise_prerequisite");

            migrationBuilder.DropTable(
                name: "footnote");

            migrationBuilder.DropTable(
                name: "intensity");

            migrationBuilder.DropTable(
                name: "newsletter");

            migrationBuilder.DropTable(
                name: "user_equipment");

            migrationBuilder.DropTable(
                name: "user_exercise");

            migrationBuilder.DropTable(
                name: "user_exercise_variation");

            migrationBuilder.DropTable(
                name: "user_token");

            migrationBuilder.DropTable(
                name: "user_variation");

            migrationBuilder.DropTable(
                name: "equipment_group");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "exercise_variation");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "exercise");

            migrationBuilder.DropTable(
                name: "variation");
        }
    }
}
