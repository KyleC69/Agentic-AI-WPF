// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RAGContext.cs
// Author: Kyle L. Crowder
// Build Num: 051920



using DataIngestionLib.RAGModels;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;




namespace DataIngestionLib.Data;





public class RAGContext : DbContext
{

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            _ = optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CONN_STRING"));
        }
    }








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelBuilder unused33 = modelBuilder.Entity<Document>(entity =>
                {
                    KeyBuilder unused32 = entity.HasKey(e => e.DocId);

                    var unused31 = entity.Property(e => e.DocId).HasDefaultValueSql("(newid())");
                    var unused30 = entity.Property(e => e.Breadcrumb).HasMaxLength(350);
                    var unused29 = entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                    var unused28 = entity.Property(e => e.DocHtml).HasColumnName("DocHTML");
                    var unused27 = entity.Property(e => e.Hash)
                            .HasMaxLength(450)
                            .IsUnicode(false);
                    var unused26 = entity.Property(e => e.Title).HasMaxLength(512);
                    var unused25 = entity.Property(e => e.Url).HasMaxLength(350);
                })
                .Entity<Metadata>(entity =>
                {
                    KeyBuilder unused24 = entity.HasKey(e => e.MetaId).HasName("PK__Metadata__60EE5418A4699143");

                    var unused23 = entity.HasIndex(e => e.DocId, "IX_Metadata_DocId");

                    var unused22 = entity.Property(e => e.MetaId).HasDefaultValueSql("(newid())");
                })
                .Entity<RemoteRag>(entity =>
                {
                    KeyBuilder unused21 = entity.HasKey(e => e.Id).HasName("PK__RemoteRA__3214EC075F4501BD");

                    var unused20 = entity.ToTable("RemoteRAG", tb =>
                    {
                        TableTriggerBuilder unused19 = tb.HasTrigger("tr_generate_embeddings");
                        TableTriggerBuilder unused18 = tb.HasTrigger("trg_RemoteRAG_CalculateScore");
                    });

                    var unused17 = entity.HasIndex(e => e.MsDate, "IX_RemoteRAG_ms_date").IsDescending();

                    var unused16 = entity.HasIndex(e => e.Score, "IX_RemoteRAG_score").IsDescending();

                    var unused15 = entity.HasIndex(e => e.UpdatedAt, "IX_RemoteRAG_updated_at_filtered")
                            .IsDescending()
                            .HasFilter("([embedding] IS NOT NULL)");

                    var unused14 = entity.HasIndex(e => e.DocumentId, "UX_RemoteRAG_document_id").IsUnique();

                    var unused13 = entity.HasIndex(e => e.OgUrl, "UX_RemoteRAG_og_url").IsUnique();

                    var unused12 = entity.HasIndex(e => e.Embedding, "VIX_RemoteRAG_embedding");

                    var unused11 = entity.Property(e => e.Description).HasColumnName("description");
                    var unused10 = entity.Property(e => e.DocumentId)
                            .HasDefaultValueSql("(newid())", "DF__RemoteRAG__DocId__4F47C5E3")
                            .HasColumnName("document_id");
                    var unused9 = entity.Property(e => e.Embedding)
                            .HasMaxLength(1024)
                            .HasColumnName("embedding");
                    var unused8 = entity.Property(e => e.Keywords)
                            .HasMaxLength(500)
                            .HasColumnName("keywords");
                    var unused7 = entity.Property(e => e.MsDate).HasColumnName("ms_date");
                    var unused6 = entity.Property(e => e.OgUrl)
                            .HasMaxLength(500)
                            .HasColumnName("og_url");
                    var unused5 = entity.Property(e => e.Score).HasColumnName("score");
                    var unused4 = entity.Property(e => e.Summary).HasColumnName("summary");
                    var unused3 = entity.Property(e => e.Title)
                            .HasMaxLength(450)
                            .HasColumnName("title");
                    var unused2 = entity.Property(e => e.TokenCount).HasColumnName("token_count");
                    var unused1 = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                    var unused = entity.Property(e => e.Version).HasColumnName("version");
                });

        OnModelCreatingPartial(modelBuilder);
    }








    public virtual DbSet<Document> Documents { get; init; }

    public virtual DbSet<Metadata> Metadata { get; init; }

    public virtual DbSet<RemoteRag> RemoteRags { get; init; }








    private static void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        ModelBuilder unused4 = modelBuilder.Entity<Document>(entity =>
                {
                    var unused3 = entity.HasIndex(e => e.Hash, "IX_Document_Hash").IsUnique();
                    var unused2 = entity.HasIndex(e => e.Url, "IX_Document_Url").IsUnique();
                })
                .Entity<Metadata>(entity => entity.HasOne<Document>().WithMany().HasForeignKey(e => e.DocId).OnDelete(DeleteBehavior.Cascade))
                .Entity<RemoteRag>(entity =>
                {
                    var unused1 = entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);

                    var unused = entity.HasOne<Document>().WithMany().HasForeignKey(e => e.DocumentId).OnDelete(DeleteBehavior.Restrict);
                });
    }
}