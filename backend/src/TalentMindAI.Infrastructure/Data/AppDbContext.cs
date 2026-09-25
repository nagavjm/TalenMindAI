using Microsoft.EntityFrameworkCore;
using TalentMindAI.Domain.Entities;

namespace TalentMindAI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<ResumeAnalysis> ResumeAnalyses => Set<ResumeAnalysis>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<PromptLog> PromptLogs => Set<PromptLog>();
    public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();

    // TalentMind NutriAI (Food module) - separate tables, isolated from Resume Assistant data.
    public DbSet<FoodChatHistory> FoodChatHistories => Set<FoodChatHistory>();
    public DbSet<FoodRecipe> FoodRecipes => Set<FoodRecipe>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<NutritionAnalysis> NutritionAnalyses => Set<NutritionAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Resume>(b =>
        {
            b.HasOne(r => r.User).WithMany(u => u.Resumes).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(r => r.Analysis).WithOne(a => a.Resume!).HasForeignKey<ResumeAnalysis>(a => a.ResumeId).OnDelete(DeleteBehavior.Cascade);
            b.Property(r => r.CandidateName).HasMaxLength(200);
            b.Property(r => r.FileName).HasMaxLength(300);
        });

        modelBuilder.Entity<Skill>(b =>
        {
            b.HasOne(s => s.Resume).WithMany(r => r.Skills).HasForeignKey(s => s.ResumeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Certification>(b =>
        {
            b.HasOne(c => c.Resume).WithMany(r => r.Certifications).HasForeignKey(c => c.ResumeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatHistory>(b =>
        {
            b.HasOne(c => c.User).WithMany(u => u.ChatHistories).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne(c => c.Resume).WithMany().HasForeignKey(c => c.ResumeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PromptLog>(b =>
        {
            b.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- TalentMind NutriAI (Food module) ----------
        modelBuilder.Entity<FoodChatHistory>(b =>
        {
            b.HasOne(c => c.User).WithMany(u => u.FoodChatHistories).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FoodRecipe>(b =>
        {
            b.HasOne(r => r.User).WithMany(u => u.FoodRecipes).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            b.Property(r => r.Title).HasMaxLength(300);
        });

        modelBuilder.Entity<MealPlan>(b =>
        {
            b.HasOne(m => m.User).WithMany(u => u.MealPlans).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            b.Property(m => m.Title).HasMaxLength(300);
        });

        modelBuilder.Entity<NutritionAnalysis>(b =>
        {
            b.HasOne(n => n.User).WithMany(u => u.NutritionAnalyses).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
