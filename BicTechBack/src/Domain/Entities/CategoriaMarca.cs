namespace BicTechBack.src.Core.Entities
{
    public class CategoriaMarca
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public int MarcaId { get; set; }
        public Marca Marca { get; set; }
    }
}
