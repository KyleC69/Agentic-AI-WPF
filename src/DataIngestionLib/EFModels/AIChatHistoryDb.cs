// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using AgentAILib.History.HistoryModels;
using AgentAILib.HistoryModels;

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;




namespace AgentAILib.EFModels;





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

            EntityTypeBuilder<ChatHistoryMessage> unused18 = entity.ToTable(tb => tb.HasTrigger("tr_generate_embeddings"));

            IndexBuilder<ChatHistoryMessage> unused17 = entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "IX_ChatHistoryMessages_Conversation_CreatedAt");

            PropertyBuilder<Guid> unused16 = entity.Property(e => e.MessageId).ValueGeneratedNever();
            PropertyBuilder<string> unused15 = entity.Property(e => e.AgentId).HasMaxLength(128);
            PropertyBuilder<string> unused14 = entity.Property(e => e.ApplicationId).HasMaxLength(128);
            PropertyBuilder<string> unused13 = entity.Property(e => e.ConversationId).HasMaxLength(128);
            PropertyBuilder<SqlVector<float>?> unused12 = entity.Property(e => e.Embedding).HasMaxLength(1024);
            PropertyBuilder<bool?> unused11 = entity.Property(e => e.Enabled).HasDefaultValue(false, "DF_ChatHistoryMessages_Enabled");
            PropertyBuilder<string> unused10 = entity.Property(e => e.Role).HasMaxLength(32);
            PropertyBuilder<string?> unused9 = entity.Property(e => e.Summary).HasMaxLength(2000);
            PropertyBuilder<string> unused8 = entity.Property(e => e.UserId).HasMaxLength(128);
            PropertyBuilder<int> unused = entity.Property(e => e.TokenCnt).HasMaxLength(4);
        });

        ModelBuilder unused7 = modelBuilder.Entity<ChatHistoryTextChunk>(entity =>
        {
            KeyBuilder unused6 = entity.HasKey(e => e.ChunkRecordId).HasName("PK__tmp_ms_x__B2ED0F6BA39E36A4");

            IndexBuilder<ChatHistoryTextChunk> unused5 = entity.HasIndex(e => e.Embedding, "VIX_ChatHistoryTextChunks_Embedding");

            PropertyBuilder<int> unused4 = entity.Property(e => e.ChunkRecordId).HasColumnName("ChunkRecordID");
            PropertyBuilder<long> unused3 = entity.Property(e => e.ChunkSetId).HasColumnName("ChunkSetID");
            PropertyBuilder<DateTime> unused2 = entity.Property(e => e.CreatedAt).HasDefaultValueSql("(datetime())", "DF__tmp_ms_xx__Creat__2FCF1A8A");
            PropertyBuilder<SqlVector<float>?> unused1 = entity.Property(e => e.Embedding).HasMaxLength(1024);
            PropertyBuilder<Guid> unused = entity.Property(e => e.MessageId).HasColumnName("MessageID");
        });




    }












    public virtual DbSet<ChatHistoryMessage> ChatHistoryMessages { get; set; }

    public virtual DbSet<ChatHistoryTextChunk> ChatHistoryTextChunks { get; set; }
}