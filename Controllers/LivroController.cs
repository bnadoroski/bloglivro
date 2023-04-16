using BlogLivro.Configuracoes;
using BlogLivro.Models.Livros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BlogLivro.Models.Emails;

namespace BlogLivro.Controllers
{
    public class LivroController : System.Web.Http.ApiController
    {
        [HttpPost, Route("api/livro/gravar")]
        public async Task<RetornoSucesso> GravarLivro([FromBody] Livro livro)
        {
            if (livro == null)
                return new RetornoSucesso("Ops, Nenhum livro enviado, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await livro.SalvarAtualizar(conn);
        }

        [HttpDelete, Route("api/livro/excluir/{id}")]
        public async Task<RetornoSucesso> ExcluirLivro(int id)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Livro.Excluir(id, conn);
        }

        [HttpPost, Route("api/livro/buscar")]
        public async Task<List<Livro>> BuscarLivro([FromBody] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            if (filtro.Paginacao == null)
                filtro.Paginacao = new Models.Paginacao(1, 9);
            else
                filtro.Paginacao = new Models.Paginacao(filtro.Paginacao.Pagina, filtro.Paginacao.Tamanho);

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Livro.BuscarLivros(filtro, conn);
        }

        [HttpPost, Route("api/livro/buscar/portag")]
        public async Task<List<Livro>> BuscarLivroPorTag(Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            if (filtro.Paginacao == null)
                filtro.Paginacao = new Models.Paginacao(1, 3);
            else
                filtro.Paginacao = new Models.Paginacao(filtro.Paginacao.Pagina, filtro.Paginacao.Tamanho);

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Livro.BuscarPorTag(filtro, conn);
        }

        [HttpGet, Route("api/livro/buscar/completo")]
        public async Task<Livro> BuscarLivroCompleto([FromQuery] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            filtro.IDUsuario = 33;

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Livro.BuscarLivroCompleto(filtro, conn);
        }
    }
}
