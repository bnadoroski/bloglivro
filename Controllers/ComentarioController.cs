using BlogLivro.Configuracoes;
using BlogLivro.Models.Comentarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class ComentarioController
    {

        [HttpPost, Route("api/comentario/gravar")]
        public async Task<RetornoSucesso> GravarComentario([FromBody] Comentario comentario)
        {
            if (comentario == null)
                return new RetornoSucesso("Ops, Nenhum comentário enviado, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await comentario.Salvar(conn);
        }

        [HttpDelete, Route("api/comentario/excluir/{id}")]
        public async Task<RetornoSucesso> ExcluirComentario(int id)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Comentario.Excluir(id, conn);
        }

        [HttpGet, Route("api/comentario/buscar")]
        public async Task<List<Comentario>> BuscarComentario([FromBody] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Comentario.Buscar(filtro, conn);
        }
    }
}
