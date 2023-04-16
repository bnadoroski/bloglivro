using BlogLivro.Configuracoes;
using BlogLivro.Models.Geral;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BlogLivro.Controllers
{
    public class GeralController : System.Web.Http.ApiController
    {
        [HttpPost, Route("api/geral/buscar")]
        public async Task<Geral> BuscarGeral([FromBody] Filtro filtro)
        {
            if (filtro.PaginacaoLivro == null)
                filtro.PaginacaoLivro = new Models.Paginacao(1, 9);
            else
                filtro.PaginacaoLivro = new Models.Paginacao(filtro.PaginacaoLivro.Pagina, filtro.PaginacaoLivro.Tamanho);

            if (filtro.PaginacaoUsuario == null)
                filtro.PaginacaoUsuario = new Models.Paginacao(1, 12);
            else
                filtro.PaginacaoUsuario = new Models.Paginacao(filtro.PaginacaoUsuario.Pagina, filtro.PaginacaoUsuario.Tamanho);

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Geral.BuscarGeral(filtro, conn);
        }
    }
}
