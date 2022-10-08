﻿// <auto-generated />
using System;
using FinerFettle.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    [DbContext(typeof(CoreContext))]
    partial class CoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EquipmentEquipmentGroup", b =>
                {
                    b.Property<int>("EquipmentGroupsId")
                        .HasColumnType("integer");

                    b.Property<int>("EquipmentId")
                        .HasColumnType("integer");

                    b.HasKey("EquipmentGroupsId", "EquipmentId");

                    b.HasIndex("EquipmentId");

                    b.ToTable("EquipmentEquipmentGroup");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Equipment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DisabledReason")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Equipment");

                    b.HasComment("Equipment used in an exercise");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.EquipmentGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Instruction")
                        .HasColumnType("text");

                    b.Property<int>("IntensityId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsWeight")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Required")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("IntensityId");

                    b.ToTable("EquipmentGroup");

                    b.HasComment("Equipment that can be switched out for one another");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Exercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DisabledReason")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PrimaryMuscles")
                        .HasColumnType("integer");

                    b.Property<int>("Proficiency")
                        .HasColumnType("integer");

                    b.Property<int>("SecondaryMuscles")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Exercise");

                    b.HasComment("Exercises listed on the website");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.ExercisePrerequisite", b =>
                {
                    b.Property<int>("ExerciseId")
                        .HasColumnType("integer");

                    b.Property<int>("PrerequisiteExerciseId")
                        .HasColumnType("integer");

                    b.HasKey("ExerciseId", "PrerequisiteExerciseId");

                    b.HasIndex("PrerequisiteExerciseId");

                    b.ToTable("exercise_prerequisite");

                    b.HasComment("Pre-requisite exercises for other exercises");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Intensity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DisabledReason")
                        .HasColumnType("text");

                    b.Property<int>("IntensityLevel")
                        .HasColumnType("integer");

                    b.Property<int>("MuscleContractions")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("VariationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VariationId");

                    b.ToTable("Intensity");

                    b.HasComment("Intensity level of an exercise variation");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.IntensityPreference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("IntensityId")
                        .HasColumnType("integer");

                    b.Property<int>("StrengtheningPreference")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("IntensityId");

                    b.ToTable("IntensityPreference");

                    b.HasComment("Intensity level of an exercise variation per user's strengthing preference");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Variation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisabledReason")
                        .HasColumnType("text");

                    b.Property<int>("ExerciseId")
                        .HasColumnType("integer");

                    b.Property<int>("ExerciseType")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SportsFocus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ExerciseId");

                    b.ToTable("Variation");

                    b.HasComment("Progressions of an exercise");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Footnotes.Footnote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Footnote");

                    b.HasComment("Sage advice");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Newsletter.Newsletter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<bool>("IsDeloadWeek")
                        .HasColumnType("boolean");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Newsletter");

                    b.HasComment("A day's workout routine");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.EquipmentUser", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("EquipmentId")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "EquipmentId");

                    b.HasIndex("EquipmentId");

                    b.ToTable("EquipmentUser");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.ExerciseUserProgression", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("ExerciseId")
                        .HasColumnType("integer");

                    b.Property<bool>("Ignore")
                        .HasColumnType("boolean");

                    b.Property<int>("Progression")
                        .HasColumnType("integer");

                    b.Property<int>("SeenCount")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "ExerciseId");

                    b.HasIndex("ExerciseId");

                    b.ToTable("ExerciseUserProgression");

                    b.HasComment("User's progression level of an exercise");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AcceptedTerms")
                        .HasColumnType("boolean");

                    b.Property<bool>("Disabled")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("EmailVerbosity")
                        .HasColumnType("integer");

                    b.Property<bool>("PrefersWeights")
                        .HasColumnType("boolean");

                    b.Property<int>("RecoveryMuscle")
                        .HasColumnType("integer");

                    b.Property<int>("RestDays")
                        .HasColumnType("integer");

                    b.Property<int>("SportsFocus")
                        .HasColumnType("integer");

                    b.Property<int>("StrengtheningPreference")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("User");

                    b.HasComment("User who signed up for the newsletter");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.UserIntensity", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("IntensityId")
                        .HasColumnType("integer");

                    b.Property<int>("SeenCount")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "IntensityId");

                    b.HasIndex("IntensityId");

                    b.ToTable("UserIntensity");

                    b.HasComment("User's intensity stats");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.UserVariation", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("VariationId")
                        .HasColumnType("integer");

                    b.Property<int>("SeenCount")
                        .HasColumnType("integer");

                    b.HasKey("UserId", "VariationId");

                    b.HasIndex("VariationId");

                    b.ToTable("UserVariation");

                    b.HasComment("User's variation excluding");
                });

            modelBuilder.Entity("EquipmentEquipmentGroup", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.EquipmentGroup", null)
                        .WithMany()
                        .HasForeignKey("EquipmentGroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.Exercise.Equipment", null)
                        .WithMany()
                        .HasForeignKey("EquipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.EquipmentGroup", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Intensity", "Intensity")
                        .WithMany("EquipmentGroups")
                        .HasForeignKey("IntensityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Intensity");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.ExercisePrerequisite", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Exercise", "Exercise")
                        .WithMany("Prerequisites")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.Exercise.Exercise", "PrerequisiteExercise")
                        .WithMany("Exercises")
                        .HasForeignKey("PrerequisiteExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("PrerequisiteExercise");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Intensity", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Variation", "Variation")
                        .WithMany("Intensities")
                        .HasForeignKey("VariationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("FinerFettle.Web.Models.Exercise.Proficiency", "Proficiency", b1 =>
                        {
                            b1.Property<int>("IntensityId")
                                .HasColumnType("integer");

                            b1.Property<int?>("MaxReps")
                                .HasColumnType("integer");

                            b1.Property<int?>("MinReps")
                                .HasColumnType("integer");

                            b1.Property<int?>("Secs")
                                .HasColumnType("integer");

                            b1.Property<int>("Sets")
                                .HasColumnType("integer");

                            b1.HasKey("IntensityId");

                            b1.ToTable("Intensity");

                            b1.WithOwner()
                                .HasForeignKey("IntensityId");
                        });

                    b.OwnsOne("FinerFettle.Web.Models.Exercise.Progression", "Progression", b1 =>
                        {
                            b1.Property<int>("IntensityId")
                                .HasColumnType("integer");

                            b1.Property<int?>("Max")
                                .HasColumnType("integer");

                            b1.Property<int?>("Min")
                                .HasColumnType("integer");

                            b1.HasKey("IntensityId");

                            b1.ToTable("Intensity");

                            b1.WithOwner()
                                .HasForeignKey("IntensityId");
                        });

                    b.Navigation("Proficiency")
                        .IsRequired();

                    b.Navigation("Progression")
                        .IsRequired();

                    b.Navigation("Variation");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.IntensityPreference", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Intensity", "Intensity")
                        .WithMany("IntensityPreferences")
                        .HasForeignKey("IntensityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("FinerFettle.Web.Models.Exercise.Proficiency", "Proficiency", b1 =>
                        {
                            b1.Property<int>("IntensityPreferenceId")
                                .HasColumnType("integer");

                            b1.Property<int?>("MaxReps")
                                .HasColumnType("integer");

                            b1.Property<int?>("MinReps")
                                .HasColumnType("integer");

                            b1.Property<int?>("Secs")
                                .HasColumnType("integer");

                            b1.Property<int>("Sets")
                                .HasColumnType("integer");

                            b1.HasKey("IntensityPreferenceId");

                            b1.ToTable("IntensityPreference");

                            b1.WithOwner()
                                .HasForeignKey("IntensityPreferenceId");
                        });

                    b.Navigation("Intensity");

                    b.Navigation("Proficiency")
                        .IsRequired();
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Variation", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Exercise", "Exercise")
                        .WithMany("Variations")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Newsletter.Newsletter", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.User.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.OwnsOne("FinerFettle.Web.Models.Exercise.ExerciseRotaion", "ExerciseRotation", b1 =>
                        {
                            b1.Property<int>("NewsletterId")
                                .HasColumnType("integer");

                            b1.Property<int>("ExerciseType")
                                .HasColumnType("integer");

                            b1.Property<int>("MuscleGroups")
                                .HasColumnType("integer");

                            b1.Property<int>("id")
                                .HasColumnType("integer");

                            b1.HasKey("NewsletterId");

                            b1.ToTable("Newsletter");

                            b1.WithOwner()
                                .HasForeignKey("NewsletterId");
                        });

                    b.Navigation("ExerciseRotation")
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.EquipmentUser", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Equipment", "Equipment")
                        .WithMany("EquipmentUsers")
                        .HasForeignKey("EquipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.User.User", "User")
                        .WithMany("EquipmentUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Equipment");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.ExerciseUserProgression", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Exercise", "Exercise")
                        .WithMany("UserProgressions")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.User.User", "User")
                        .WithMany("ExerciseProgressions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.UserIntensity", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Intensity", "Intensity")
                        .WithMany("UserIntensities")
                        .HasForeignKey("IntensityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.User.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Intensity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.UserVariation", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.User.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FinerFettle.Web.Models.Exercise.Variation", "Variation")
                        .WithMany("UserVariations")
                        .HasForeignKey("VariationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Variation");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Equipment", b =>
                {
                    b.Navigation("EquipmentUsers");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Exercise", b =>
                {
                    b.Navigation("Exercises");

                    b.Navigation("Prerequisites");

                    b.Navigation("UserProgressions");

                    b.Navigation("Variations");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Intensity", b =>
                {
                    b.Navigation("EquipmentGroups");

                    b.Navigation("IntensityPreferences");

                    b.Navigation("UserIntensities");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Variation", b =>
                {
                    b.Navigation("Intensities");

                    b.Navigation("UserVariations");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.User", b =>
                {
                    b.Navigation("EquipmentUsers");

                    b.Navigation("ExerciseProgressions");
                });
#pragma warning restore 612, 618
        }
    }
}
