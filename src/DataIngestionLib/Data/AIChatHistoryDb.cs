// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIChatHistory.cs
// Author: Kyle L. Crowder
// Build Num: 044232



using DataIngestionLib.History.HistoryModels;

using Microsoft.EntityFrameworkCore;




namespace DataIngestionLib.Data;





public class AIChatHistoryDb : DbContext
{
    public AIChatHistoryDb()
    {
    }








    public AIChatHistoryDb(DbContextOptions<AIChatHistoryDb> options)
            : base(options)
    {
    }








    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CHAT_HISTORY"));








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatHistoryMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable(tb => tb.HasTrigger("tr_generate_embeddings"));

            entity.HasIndex(e => new { e.ConversationId, e.TimestampUtc }, "IX_ChatHistoryMessages_Conversation_Timestamp");

            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.AgentId).HasMaxLength(128);
            entity.Property(e => e.ApplicationId).HasMaxLength(128);
            entity.Property(e => e.ConversationId).HasMaxLength(128);
            entity.Property(e => e.Embedding).HasMaxLength(1024);
            entity.Property(e => e.Enabled).HasDefaultValue(false, "DF_ChatHistoryMessages_Enabled");
            entity.Property(e => e.Role).HasMaxLength(32);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.Property(e => e.UserId).HasMaxLength(128);
        });

        modelBuilder.Entity<ChatHistoryTextChunk>(entity =>
        {
            entity.HasKey(e => e.ChunkRecordId).HasName("PK__tmp_ms_x__B2ED0F6BA39E36A4");

            entity.HasIndex(e => e.Embedding, "VIX_ChatHistoryTextChunks_Embedding");

            entity.Property(e => e.ChunkRecordId).HasColumnName("ChunkRecordID");
            entity.Property(e => e.ChunkSetId).HasColumnName("ChunkSetID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())", "DF__tmp_ms_xx__Creat__2FCF1A8A");
            entity.Property(e => e.Embedding).HasMaxLength(1024);
            entity.Property(e => e.MessageId).HasColumnName("MessageID");
        });

        
    }








    public virtual DbSet<ChatHistoryMessage> ChatHistoryMessages { get; set; }

    public virtual DbSet<ChatHistoryTextChunk> ChatHistoryTextChunks { get; set; }







}