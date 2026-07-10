using System.ComponentModel.DataAnnotations;

namespace API_Data.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)] // Limite de 150 caracteres para o título
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool Completed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Chave Estrangeira e Relacionamento com User
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relacionamento Muitos-para-Muitos com Categorias
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
