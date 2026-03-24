using API.DTO.Request.Categorias;
using API.DTO.Response.Categorias;

namespace API.Services.Categorias
{
    public interface ICategoriaService
    {
        /// <summary>
        /// Obtiene todas las categorías activas del negocio actual
        /// </summary>
        Task<CategoriaListResponse> GetAllAsync();

        /// <summary>
        /// Obtiene una categoría por ID verificando pertenencia al negocio
        /// </summary>
        Task<CategoriaResponse?> GetByIdAsync(int id);

        /// <summary>
        /// Crea una nueva categoría para el negocio actual
        /// </summary>
        Task<CategoriaResponse> CreateAsync(CrearCategoriaRequest request);

        /// <summary>
        /// Actualiza una categoría existente verificando propiedad
        /// </summary>
        Task<CategoriaResponse?> UpdateAsync(int id, ActualizarCategoriaRequest request);

        /// <summary>
        /// Realiza soft delete (desactiva) una categoría
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}