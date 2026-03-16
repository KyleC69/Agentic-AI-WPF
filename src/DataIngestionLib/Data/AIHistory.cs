// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIHistory.cs
// Author: Kyle L. Crowder
// Build Num: 051922



using DataIngestionLib.History.HistoryModels;

using Microsoft.EntityFrameworkCore;




namespace DataIngestionLib.History.Data;





public partial class AIHistory : DbContext
{
    public AIHistory()
    {
    }








    public AIHistory(DbContextOptions<AIHistory> options)
            : base(options)
    {
    }








    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=desktop-nc01091;Initial Catalog=AIChatHistory;Integrated Security=True;Trust Server Certificate=True;Command Timeout=300");








    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatHistoryMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable(tb => tb.HasTrigger("tr_generate_embeddings"));

            entity.HasIndex(e => new { e.ConversationId, e.TimestampUtc }, "IX_ChatHistoryMessages_Conversation_Timestamp");

            entity.HasIndex(e => new { e.SessionId, e.TimestampUtc }, "IX_ChatHistoryMessages_Session_Timestamp");

            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.AgentId).HasMaxLength(128);
            entity.Property(e => e.ApplicationId).HasMaxLength(128);
            entity.Property(e => e.ConversationId).HasMaxLength(128);
            entity.Property(e => e.Embedding).HasMaxLength(1024);
            entity.Property(e => e.Enabled).HasDefaultValue(false, "DF_ChatHistoryMessages_Enabled");
            entity.Property(e => e.Role).HasMaxLength(32);
            entity.Property(e => e.SessionId).HasMaxLength(128);
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

        OnModelCreatingPartial(modelBuilder);
    }








    public virtual DbSet<ChatHistoryMessage> ChatHistoryMessages { get; set; }

    public virtual DbSet<ChatHistoryTextChunk> ChatHistoryTextChunks { get; set; }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}