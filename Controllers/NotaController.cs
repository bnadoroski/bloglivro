using BlogLivro.Configuracoes;
using BlogLivro.Models.Notas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class NotaController
    {
        [HttpPost, Route("api/nota/gravar")]
        public async Task<RetornoSucesso> GravarNota(Nota nota)
        {
            if (nota == null)
                return new RetornoSucesso("Ops, Nenhuma nota enviada, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await nota.Salvar(conn);
        }

        [HttpPost, Route("api/nota/excluir")]
        public async Task<RetornoSucesso> ExcluirNota(Filtro filtro)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Nota.Excluir(filtro, conn);
        }

        [HttpPost, Route("api/nota/buscar")]
        public async Task<Nota> BuscarNota(Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Nota.Buscar(filtro, conn);
        }
    }
}
