// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIChatHistoryDb.cs
// Author: Kyle L. Crowder
// Build Num: 233124



using DataIngestionLib.History.HistoryModels;
using DataIngestionLib.HistoryModels;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;




namespace DataIngestionLib.Data;





public class AIChatHistoryDb : DbContext
{

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CHAT_HISTORY"));
    }








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelBuilder unused20 = modelBuilder.Entity<ChatHistoryMessage>(entity =>
        {
            KeyBuilder unused19 = entity.HasKey(e => e.MessageId);

            var unused18 = entity.ToTable(tb => tb.HasTrigger("tr_generate_embeddings"));

            var unused17 = entity.HasIndex(e => new { e.ConversationId, e.TimestampUtc }, "IX_ChatHistoryMessages_Conversation_Timestamp");

            var unused16 = entity.Property(e => e.MessageId).ValueGeneratedNever();
            var unused15 = entity.Property(e => e.AgentId).HasMaxLength(128);
            var unused14 = entity.Property(e => e.ApplicationId).HasMaxLength(128);
            var unused13 = entity.Property(e => e.ConversationId).HasMaxLength(128);
            var unused12 = entity.Property(e => e.Embedding).HasMaxLength(1024);
            var unused11 = entity.Property(e => e.Enabled).HasDefaultValue(false, "DF_ChatHistoryMessages_Enabled");
            var unused10 = entity.Property(e => e.Role).HasMaxLength(32);
            var unused9 = entity.Property(e => e.Summary).HasMaxLength(2000);
            var unused8 = entity.Property(e => e.UserId).HasMaxLength(128);
        });

        ModelBuilder unused7 = modelBuilder.Entity<ChatHistoryTextChunk>(entity =>
        {
            KeyBuilder unused6 = entity.HasKey(e => e.ChunkRecordId).HasName("PK__tmp_ms_x__B2ED0F6BA39E36A4");

            var unused5 = entity.HasIndex(e => e.Embedding, "VIX_ChatHistoryTextChunks_Embedding");

            var unused4 = entity.Property(e => e.ChunkRecordId).HasColumnName("ChunkRecordID");
            var unused3 = entity.Property(e => e.ChunkSetId).HasColumnName("ChunkSetID");
            var unused2 = entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())", "DF__tmp_ms_xx__Creat__2FCF1A8A");
            var unused1 = entity.Property(e => e.Embedding).HasMaxLength(1024);
            var unused = entity.Property(e => e.MessageId).HasColumnName("MessageID");
        });


    }








    public virtual DbSet<ChatHistoryMessage> ChatHistoryMessages { get; set; }

    public virtual DbSet<ChatHistoryTextChunk> ChatHistoryTextChunks { get; set; }
}