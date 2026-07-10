using API_Data.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Data.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurando os nomes das tabelas para minusculo 
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<TaskModel>().ToTable("tasks");
            modelBuilder.Entity<Category>().ToTable("categories");

            // Configurando o relacionamento Muitos para Muitos (Task <=> Category)
            // O EF Core cria a tabela intermediária automaticamente com essa configuração
            modelBuilder.Entity<TaskModel>()
                .HasMany(t => t.Categories)
                .WithMany(c => c.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "task_categories", // Nome da tabela no banco
                    j => j.HasOne<Category>().WithMany().HasForeignKey("category_id"),
                    j => j.HasOne<TaskModel>().WithMany().HasForeignKey("task_id")
                );

            // Configurando relacionamento: User -> Tasks (Um para Muitos)
            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Se deletar o usuário, deleta as tarefas

            // Configurando relacionamento: User -> Categories (Um para Muitos)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Evita conflito de caminhos de cascade
        }


    }
}
