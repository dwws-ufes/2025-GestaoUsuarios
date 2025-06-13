using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UsersManager.Application.DTOs
{
    public class PermissaoDTO
    {
        private int? id;
        public int? Id { get { return id == null ? 0 : id; } set { id = value; } }  

        [Required(ErrorMessage = "O nome é obrigatório.")] // Mensagem de erro mais genérica
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")]
        [MinLength(3, ErrorMessage = "O nome deve ter no mínimo {1} caracteres.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O recurso é obrigatório.")] // Mensagem de erro ajustada
        [StringLength(255, ErrorMessage = "O recurso deve ter no máximo {1} caracteres.")]
        [MinLength(3, ErrorMessage = "O recurso deve ter no mínimo {1} caracteres.")] // Ajustado para 3, para "User-Update-Role"
        public string? Recurso { get; set; }

        [JsonPropertyName("acaoActionEnum")]
        [Required(ErrorMessage = "A ação é obrigatória.")] // Adicionado Required para o enum
        public ActionEnum Acao { get; set; }

        public enum ActionEnum
        {
            // Os valores são implicitamente 0, 1, 2, 3...
            // Por padrão, o serializador de JSON para enums no System.Text.Json usa o nome da string
            Update,
            Delete,
            Read,
            Create
        }
    }
}