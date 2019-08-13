﻿// <auto-generated />
using System;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20190813090010_prva")]
    partial class prva
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity("API.Models.Post", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("date");

                    b.Property<string>("post");

                    b.Property<int?>("userIdid");

                    b.HasKey("id");

                    b.HasIndex("userIdid");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("API.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("email");

                    b.Property<string>("firstName");

                    b.Property<string>("image");

                    b.Property<string>("lastName");

                    b.Property<string>("password");

                    b.Property<string>("token");

                    b.Property<string>("username");

                    b.HasKey("id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("API.Models.Post", b =>
                {
                    b.HasOne("API.Models.User", "userId")
                        .WithMany()
                        .HasForeignKey("userIdid");
                });
#pragma warning restore 612, 618
        }
    }
}
