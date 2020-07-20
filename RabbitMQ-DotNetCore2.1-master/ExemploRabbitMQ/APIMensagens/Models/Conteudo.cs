using System.ComponentModel.DataAnnotations;

namespace APIMensagens.Models
{
    public class Conteudo
    {
        [Required]
        public string Mensagem { get; set; }
        public MyObjeto TesteObjeto { get; set; }
    }

    public class MyObjeto
    {
        public string Nome { get; set; }
        public int Porta { get; set; }
        public string Descricao { get; set; }

        public override string ToString()
        {
            return $"{nameof(Nome)}: {Nome} {nameof(Descricao)}: {Descricao} {nameof(Porta)}: {Porta}";
        }
    }
}