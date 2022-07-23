﻿// <auto-generated />
using System;
using FinerFettle.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinerFettle.Web.Migrations
{
    [DbContext(typeof(CoreContext))]
    [Migration("20220723152939_MoveProgressionsIntoIntensity")]
    partial class MoveProgressionsIntoIntensity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Exercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ExerciseType")
                        .HasColumnType("integer");

                    b.Property<int>("Muscles")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Exercise");

                    b.HasComment("Exercises listed on the website");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Intensity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("IntensityLevel")
                        .HasColumnType("integer");

                    b.Property<int>("MuscleContractions")
                        .HasColumnType("integer");

                    b.Property<int?>("Progression")
                        .HasColumnType("integer");

                    b.Property<int?>("VariationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VariationId");

                    b.ToTable("Intensity");

                    b.HasComment("Intensity level of an exercise variation");
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

                    b.Property<int>("Equipment")
                        .HasColumnType("integer");

                    b.Property<int?>("ExerciseId")
                        .HasColumnType("integer");

                    b.Property<string>("Instruction")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

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

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Newsletter");

                    b.HasComment("A day's workout routine");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.User.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Equipment")
                        .HasColumnType("integer");

                    b.Property<bool>("NeedsRest")
                        .HasColumnType("boolean");

                    b.Property<bool>("OverMinimumAge")
                        .HasColumnType("boolean");

                    b.Property<int?>("Progression")
                        .HasColumnType("integer");

                    b.Property<int>("RestDays")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("User");

                    b.HasComment("User who signed up for the newsletter");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Intensity", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Variation", null)
                        .WithMany("Intensities")
                        .HasForeignKey("VariationId");

                    b.OwnsOne("FinerFettle.Web.Models.Exercise.Proficiency", "Proficiency", b1 =>
                        {
                            b1.Property<int>("IntensityId")
                                .HasColumnType("integer");

                            b1.Property<int?>("Reps")
                                .HasColumnType("integer");

                            b1.Property<int?>("Secs")
                                .HasColumnType("integer");

                            b1.Property<int?>("Sets")
                                .HasColumnType("integer");

                            b1.HasKey("IntensityId");

                            b1.ToTable("Intensity");

                            b1.WithOwner()
                                .HasForeignKey("IntensityId");
                        });

                    b.Navigation("Proficiency")
                        .IsRequired();
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Variation", b =>
                {
                    b.HasOne("FinerFettle.Web.Models.Exercise.Exercise", null)
                        .WithMany("Variations")
                        .HasForeignKey("ExerciseId");
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

                            b1.Property<int?>("MuscleGroups")
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

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Exercise", b =>
                {
                    b.Navigation("Variations");
                });

            modelBuilder.Entity("FinerFettle.Web.Models.Exercise.Variation", b =>
                {
                    b.Navigation("Intensities");
                });
#pragma warning restore 612, 618
        }
    }
}
