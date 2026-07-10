using System.ComponentModel.DataAnnotations;

namespace API_Data.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)] // Bom espaço para o hash da senha
        public string Password { get; set; } = string.Empty;

        // Relacionamentos (Propriedades de Navegação)
        public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
