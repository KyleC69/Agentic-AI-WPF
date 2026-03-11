// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         RAGContext.cs
// Author: Kyle L. Crowder
// Build Num: 105656



using DataIngestionLib.ExternalKnowledge.RAGModels;

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;




namespace DataIngestionLib.Data;





public partial class RAGContext : DbContext
{

    public virtual DbSet<ChunksCode> ChunksCodes { get; set; }

    public virtual DbSet<ChunksText> ChunksTexts { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Metadata> Metadata { get; set; }

    public virtual DbSet<RagPolicyChunk> RagPolicyChunks { get; set; }

    public virtual DbSet<RemoteRag> RemoteRags { get; set; }








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelBuilder unused48 = modelBuilder.Entity<ChunksCode>(entity =>
        {
            KeyBuilder unused47 = entity.HasKey(e => e.ChunkId).HasName("PK__Chunks_C__FBFF9D00CD512EC1");

            EntityTypeBuilder<ChunksCode> unused46 = entity.ToTable("Chunks_Code");

            PropertyBuilder<Guid> unused45 = entity.Property(e => e.ChunkId).HasDefaultValueSql("(newid())");
            PropertyBuilder<SqlVector<float>> unused44 = entity.Property(e => e.Embedding).HasMaxLength(768);
        });

        ModelBuilder unused43 = modelBuilder.Entity<ChunksText>(entity =>
        {
            KeyBuilder unused42 = entity.HasKey(e => e.ChunkId).HasName("PK__Chunks_T__FBFF9D009E8FE9AD");

            EntityTypeBuilder<ChunksText> unused41 = entity.ToTable("Chunks_Text");

            IndexBuilder<ChunksText> unused40 = entity.HasIndex(e => e.DocId, "IX_ChunksText_DocId");

            PropertyBuilder<Guid> unused39 = entity.Property(e => e.ChunkId).HasDefaultValueSql("(newid())");
            PropertyBuilder<SqlVector<float>> unused38 = entity.Property(e => e.Embedding).HasMaxLength(1536);
        });

        ModelBuilder unused37 = modelBuilder.Entity<Document>(entity =>
        {
            KeyBuilder unused36 = entity.HasKey(e => e.DocId);

            PropertyBuilder<Guid> unused35 = entity.Property(e => e.DocId).HasDefaultValueSql("(newid())");
            PropertyBuilder<string?> unused34 = entity.Property(e => e.Breadcrumb).HasMaxLength(350);
            PropertyBuilder<DateTime> unused33 = entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            PropertyBuilder<string?> unused32 = entity.Property(e => e.DocHtml).HasColumnName("DocHTML");
            PropertyBuilder<string?> unused31 = entity.Property(e => e.Hash)
                    .HasMaxLength(450)
                    .IsUnicode(false);
            PropertyBuilder<string> unused30 = entity.Property(e => e.Title).HasMaxLength(512);
            PropertyBuilder<string?> unused29 = entity.Property(e => e.Url).HasMaxLength(350);
        });

        ModelBuilder unused28 = modelBuilder.Entity<Metadata>(entity =>
        {
            KeyBuilder unused27 = entity.HasKey(e => e.MetaId).HasName("PK__Metadata__60EE5418A4699143");

            IndexBuilder<Metadata> unused26 = entity.HasIndex(e => e.DocId, "IX_Metadata_DocId");

            PropertyBuilder<Guid> unused25 = entity.Property(e => e.MetaId).HasDefaultValueSql("(newid())");
        });

        ModelBuilder unused24 = modelBuilder.Entity<RagPolicyChunk>(entity =>
        {
            EntityTypeBuilder<RagPolicyChunk> unused23 = entity.ToTable("rag_policy_chunk");

            IndexBuilder<RagPolicyChunk> unused22 = entity.HasIndex(e => e.Embedding, "VIX_rag_policy_chunk_embedding");

            PropertyBuilder<int> unused21 = entity.Property(e => e.Id).HasColumnName("id");
            PropertyBuilder<string?> unused20 = entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .HasColumnName("category");
            PropertyBuilder<Guid> unused19 = entity.Property(e => e.ChunkId)
                    .HasDefaultValueSql("(newid())", "DF_rag_policy_chunk_chunk_id")
                    .HasColumnName("chunk_id");
            PropertyBuilder<string> unused18 = entity.Property(e => e.Content).HasColumnName("content");
            PropertyBuilder<DateTime> unused17 = entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("(getdate())", "DF_rag_policy_chunk_created_date")
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");
            PropertyBuilder<SqlVector<float>> unused16 = entity.Property(e => e.Embedding)
                    .HasMaxLength(1024)
                    .HasColumnName("embedding");
            PropertyBuilder<string?> unused15 = entity.Property(e => e.Tags)
                    .HasMaxLength(200)
                    .HasColumnName("tags");
        });

        ModelBuilder unused14 = modelBuilder.Entity<RemoteRag>(entity =>
        {
            KeyBuilder unused13 = entity.HasKey(e => e.Id).HasName("PK__RemoteRA__3214EC075F4501BD");

            EntityTypeBuilder<RemoteRag> unused12 = entity.ToTable("RemoteRAG");

            IndexBuilder<RemoteRag> unused11 = entity.HasIndex(e => e.Embedding, "VIX_RemoteRAG_embedding");

            PropertyBuilder<string> unused10 = entity.Property(e => e.Description).HasColumnName("description");
            PropertyBuilder<Guid> unused9 = entity.Property(e => e.DocumentId)
                    .HasDefaultValueSql("(newid())", "DF__RemoteRAG__DocId__4F47C5E3")
                    .HasColumnName("document_id");
            PropertyBuilder<SqlVector<float>?> unused8 = entity.Property(e => e.Embedding)
                    .HasMaxLength(1024)
                    .HasColumnName("embedding");
            PropertyBuilder<string?> unused7 = entity.Property(e => e.Keywords)
                    .HasMaxLength(500)
                    .HasColumnName("keywords");
            PropertyBuilder<DateTime> unused6 = entity.Property(e => e.MsDate).HasColumnName("ms_date");
            PropertyBuilder<string> unused5 = entity.Property(e => e.OgUrl)
                    .HasMaxLength(500)
                    .HasColumnName("og_url");
            PropertyBuilder<string?> unused4 = entity.Property(e => e.Summary).HasColumnName("summary");
            PropertyBuilder<string> unused3 = entity.Property(e => e.Title)
                    .HasMaxLength(450)
                    .HasColumnName("title");
            PropertyBuilder<int?> unused2 = entity.Property(e => e.TokenCount).HasColumnName("token_count");
            PropertyBuilder<DateTime?> unused1 = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            PropertyBuilder<int?> unused = entity.Property(e => e.Version).HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }








    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}