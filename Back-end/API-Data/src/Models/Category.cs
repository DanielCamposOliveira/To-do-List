using System.ComponentModel.DataAnnotations;

namespace API_Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Nome da categoria

        // Chave Estrangeira e Relacionamento com User (Categoria por usuário)
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relacionamento Muitos-para-Muitos com Tarefas
        public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    }
}
