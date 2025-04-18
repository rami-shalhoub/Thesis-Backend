﻿using System;
using System.Collections.Generic;
using Backend.models;
using Microsoft.EntityFrameworkCore;

namespace Backend.data;

public partial class ThesisDappDBContext : DbContext
{
    public ThesisDappDBContext()
    {
    }

    public ThesisDappDBContext(DbContextOptions<ThesisDappDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessLog> AccessLog { get; set; }

    public virtual DbSet<Document> Document { get; set; }

    public virtual DbSet<LegalResource> LegalResource { get; set; }

    public virtual DbSet<RevokedToken> RevokedToken { get; set; }

    public virtual DbSet<Session> Session { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=thesisappdb;Username=rami;Password=710037802");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.logID).HasName("AccessLog_pkey");

            entity.Property(e => e.logID).ValueGeneratedNever();
            entity.Property(e => e.action).HasMaxLength(255);
            entity.Property(e => e.timeStamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");

            entity.HasOne(d => d.document).WithMany(p => p.AccessLog)
                .HasForeignKey(d => d.documentID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessed_documentID");

            entity.HasOne(d => d.session).WithMany(p => p.AccessLog)
                .HasForeignKey(d => d.sessionID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessed_sessionID");

            entity.HasOne(d => d.user).WithMany(p => p.AccessLog)
                .HasForeignKey(d => d.userID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessed_userID");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.documentID).HasName("Document_pkey");

            entity.Property(e => e.documentID).ValueGeneratedNever();
            entity.Property(e => e.accessControl).HasColumnType("jsonb");
            entity.Property(e => e.blockchainTxID).HasMaxLength(255);
            entity.Property(e => e.createdAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");
            entity.Property(e => e.documentType)
                .HasMaxLength(255)
                .HasDefaultValueSql("'other'::character varying");
            entity.Property(e => e.ipfsCID).HasMaxLength(255);
            entity.Property(e => e.jurisdiction)
                .HasMaxLength(255)
                .HasDefaultValueSql("'GB'::character varying")
                .HasComment("Focus on UK");
            entity.Property(e => e.title).HasMaxLength(255);
            entity.Property(e => e.updatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");

            entity.HasOne(d => d.owner).WithMany(p => p.Document)
                .HasForeignKey(d => d.ownerID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_document_ownerID");
        });

        modelBuilder.Entity<LegalResource>(entity =>
        {
            entity.HasKey(e => e.resourceID).HasName("LegalResources_pkey");

            entity.Property(e => e.resourceID).ValueGeneratedNever();
            entity.Property(e => e.contentType).HasMaxLength(255);
            entity.Property(e => e.effectiveDate).HasComment("when the law was enacted");
            entity.Property(e => e.ipfsCID).HasMaxLength(255);
            entity.Property(e => e.jurisdiction)
                .HasMaxLength(255)
                .HasDefaultValueSql("'GB'::character varying")
                .HasComment("Focus on UK");
            entity.Property(e => e.lastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.title).HasMaxLength(255);
            entity.Property(e => e.vectorEmbedding).HasComment("VECTOR(1536)");
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.HasKey(e => e.tokenID).HasName("RevokedToken_pkey");

            entity.Property(e => e.tokenID).HasMaxLength(255);
            entity.Property(e => e.expiry).HasColumnType("timestamp(2) without time zone");
            entity.Property(e => e.revokedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.RevokedToken)
                .HasForeignKey(d => d.userID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_userID_revoked_token");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.sessionID).HasName("Session_pkey");

            entity.Property(e => e.sessionID).ValueGeneratedNever();
            entity.Property(e => e.analysisParameter).HasColumnType("jsonb");
            entity.Property(e => e.contextWindow).HasColumnType("jsonb");
            entity.Property(e => e.createdAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");

            entity.HasOne(d => d.document).WithMany(p => p.Session)
                .HasForeignKey(d => d.documentID)
                .HasConstraintName("FK_session_documentID");

            entity.HasOne(d => d.user).WithMany(p => p.Session)
                .HasForeignKey(d => d.userID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_session_userID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.userID).HasName("User_pkey");

            entity.HasIndex(e => e.email, "user_email_unique").IsUnique();

            entity.Property(e => e.userID).ValueGeneratedNever();
            entity.Property(e => e.createdAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.isActive).HasDefaultValue(false);
            entity.Property(e => e.lastLogin)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(2) without time zone");
            entity.Property(e => e.name).HasMaxLength(255);
            entity.Property(e => e.organisationID)
                .HasMaxLength(255)
                .HasDefaultValueSql("'ClientOrgMSP'::character varying");
            entity.Property(e => e.password)
                .HasMaxLength(255)
                .HasComment("hased");
            entity.Property(e => e.refreshToken).HasMaxLength(255);
            entity.Property(e => e.role)
                .HasMaxLength(255)
                .HasDefaultValueSql("'client'::character varying");
            entity.Property(e => e.tokenExpiry).HasColumnType("timestamp(2) without time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
