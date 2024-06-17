using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SquashMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SkillType = table.Column<int>(type: "integer", nullable: false),
                    Skills = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
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
                    Note = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
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
                    ShowStaticImages = table.Column<bool>(type: "boolean", nullable: false),
                    Equipment = table.Column<int>(type: "integer", nullable: false),
                    IncludeMobilityWorkouts = table.Column<bool>(type: "boolean", nullable: false),
                    SeasonedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FootnoteType = table.Column<int>(type: "integer", nullable: false),
                    PrehabFocus = table.Column<long>(type: "bigint", nullable: false),
                    RehabFocus = table.Column<long>(type: "bigint", nullable: false),
                    RehabSkills = table.Column<int>(type: "integer", nullable: false),
                    SportsFocus = table.Column<int>(type: "integer", nullable: false),
                    SendDays = table.Column<int>(type: "integer", nullable: false),
                    SendHour = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Intensity = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    DeloadAfterXWeeks = table.Column<int>(type: "integer", nullable: false),
                    Verbosity = table.Column<int>(type: "integer", nullable: false),
                    LastActive = table.Column<DateOnly>(type: "date", nullable: true),
                    NewsletterDisabledReason = table.Column<string>(type: "text", nullable: true),
                    Features = table.Column<int>(type: "integer", nullable: false),
                    IgnorePrerequisites = table.Column<bool>(type: "boolean", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Mobility = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Flexibility = table.Column<int>(type: "integer", nullable: false),
                    AtLeastXUniqueMusclesPerExercise_Accessory = table.Column<int>(type: "integer", nullable: false),
                    FootnoteCountTop = table.Column<int>(type: "integer", nullable: false),
                    FootnoteCountBottom = table.Column<int>(type: "integer", nullable: false),
                    WeightIsolationXTimesMore = table.Column<double>(type: "double precision", nullable: false),
                    WeightSecondaryMusclesXTimesLess = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                },
                comment: "User who signed up for the newsletter");

            migrationBuilder.CreateTable(
                name: "exercise_prerequisite",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    PrerequisiteExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Proficiency = table.Column<int>(type: "integer", nullable: false)
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
                name: "variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StaticImage = table.Column<string>(type: "text", nullable: false),
                    AnimatedImage = table.Column<string>(type: "text", nullable: true),
                    Unilateral = table.Column<bool>(type: "boolean", nullable: false),
                    UseCaution = table.Column<bool>(type: "boolean", nullable: false),
                    IsWeighted = table.Column<bool>(type: "boolean", nullable: false),
                    PauseReps = table.Column<bool>(type: "boolean", nullable: true),
                    MuscleMovement = table.Column<int>(type: "integer", nullable: false),
                    MovementPattern = table.Column<int>(type: "integer", nullable: false),
                    MobilityJoints = table.Column<long>(type: "bigint", nullable: false),
                    StrengthMuscles = table.Column<long>(type: "bigint", nullable: false),
                    StretchMuscles = table.Column<long>(type: "bigint", nullable: false),
                    SecondaryMuscles = table.Column<long>(type: "bigint", nullable: false),
                    ExerciseFocus = table.Column<int>(type: "integer", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Progression_Min = table.Column<int>(type: "integer", nullable: true),
                    Progression_Max = table.Column<int>(type: "integer", nullable: true),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    SportsFocus = table.Column<int>(type: "integer", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DefaultInstruction = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_variation_exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Variations of exercises");

            migrationBuilder.CreateTable(
                name: "user_email",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    SendAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    EmailStatus = table.Column<int>(type: "integer", nullable: false),
                    SendAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_email", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_email_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateTable(
                name: "user_exercise",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExerciseId = table.Column<int>(type: "integer", nullable: false),
                    Progression = table.Column<int>(type: "integer", nullable: false),
                    Ignore = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeen = table.Column<DateOnly>(type: "date", nullable: false),
                    LastVisible = table.Column<DateOnly>(type: "date", nullable: false)
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
                name: "user_footnote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserLastSeen = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_footnote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_footnote_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Sage advice");

            migrationBuilder.CreateTable(
                name: "user_frequency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Rotation_Id = table.Column<int>(type: "integer", nullable: false),
                    Rotation_MuscleGroups = table.Column<string>(type: "text", nullable: false),
                    Rotation_MovementPatterns = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_frequency", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_user_frequency_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_muscle_flexibility",
                columns: table => new
                {
                    MuscleGroup = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_muscle_flexibility", x => new { x.UserId, x.MuscleGroup });
                    table.ForeignKey(
                        name: "FK_user_muscle_flexibility_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_muscle_mobility",
                columns: table => new
                {
                    MuscleGroup = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_muscle_mobility", x => new { x.UserId, x.MuscleGroup });
                    table.ForeignKey(
                        name: "FK_user_muscle_mobility_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_muscle_strength",
                columns: table => new
                {
                    MuscleGroup = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<int>(type: "integer", nullable: false),
                    End = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_muscle_strength", x => new { x.UserId, x.MuscleGroup });
                    table.ForeignKey(
                        name: "FK_user_muscle_strength_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Auth tokens for a user");

            migrationBuilder.CreateTable(
                name: "user_workout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Rotation_Id = table.Column<int>(type: "integer", nullable: false),
                    Rotation_MuscleGroups = table.Column<string>(type: "text", nullable: false),
                    Rotation_MovementPatterns = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    Intensity = table.Column<int>(type: "integer", nullable: false),
                    IsDeloadWeek = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_workout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_workout_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateTable(
                name: "instruction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Equipment = table.Column<int>(type: "integer", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    VariationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instruction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_instruction_instruction_ParentId",
                        column: x => x.ParentId,
                        principalTable: "instruction",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_instruction_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Equipment that can be switched out for one another");

            migrationBuilder.CreateTable(
                name: "user_variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    Ignore = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeen = table.Column<DateOnly>(type: "date", nullable: false),
                    RefreshAfter = table.Column<DateOnly>(type: "date", nullable: true),
                    LagRefreshXWeeks = table.Column<int>(type: "integer", nullable: false),
                    PadRefreshXWeeks = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_variation", x => x.Id);
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
                name: "user_workout_variation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserWorkoutId = table.Column<int>(type: "integer", nullable: false),
                    VariationId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Section = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_workout_variation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_workout_variation_user_workout_UserWorkoutId",
                        column: x => x.UserWorkoutId,
                        principalTable: "user_workout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_workout_variation_variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "A day's workout routine");

            migrationBuilder.CreateTable(
                name: "user_variation_weight",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<int>(type: "integer", nullable: false),
                    UserVariationId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_variation_weight", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_variation_weight_user_variation_UserVariationId",
                        column: x => x.UserVariationId,
                        principalTable: "user_variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User variation weight log");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_prerequisite_PrerequisiteExerciseId",
                table: "exercise_prerequisite",
                column: "PrerequisiteExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_instruction_ParentId",
                table: "instruction",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_instruction_VariationId",
                table: "instruction",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_email_UserId",
                table: "user_email",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_ExerciseId",
                table: "user_exercise",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_user_footnote_UserId",
                table: "user_footnote",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_token_UserId_Token",
                table: "user_token",
                columns: new[] { "UserId", "Token" });

            migrationBuilder.CreateIndex(
                name: "IX_user_variation_UserId_VariationId_Section",
                table: "user_variation",
                columns: new[] { "UserId", "VariationId", "Section" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_variation_VariationId",
                table: "user_variation",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_user_variation_weight_UserVariationId",
                table: "user_variation_weight",
                column: "UserVariationId");

            migrationBuilder.CreateIndex(
                name: "IX_user_workout_UserId",
                table: "user_workout",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_workout_variation_UserWorkoutId",
                table: "user_workout_variation",
                column: "UserWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_user_workout_variation_VariationId",
                table: "user_workout_variation",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_variation_ExerciseId",
                table: "variation",
                column: "ExerciseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_prerequisite");

            migrationBuilder.DropTable(
                name: "footnote");

            migrationBuilder.DropTable(
                name: "instruction");

            migrationBuilder.DropTable(
                name: "user_email");

            migrationBuilder.DropTable(
                name: "user_exercise");

            migrationBuilder.DropTable(
                name: "user_footnote");

            migrationBuilder.DropTable(
                name: "user_frequency");

            migrationBuilder.DropTable(
                name: "user_muscle_flexibility");

            migrationBuilder.DropTable(
                name: "user_muscle_mobility");

            migrationBuilder.DropTable(
                name: "user_muscle_strength");

            migrationBuilder.DropTable(
                name: "user_token");

            migrationBuilder.DropTable(
                name: "user_variation_weight");

            migrationBuilder.DropTable(
                name: "user_workout_variation");

            migrationBuilder.DropTable(
                name: "user_variation");

            migrationBuilder.DropTable(
                name: "user_workout");

            migrationBuilder.DropTable(
                name: "variation");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "exercise");
        }
    }
}
