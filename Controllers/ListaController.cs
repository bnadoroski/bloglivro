using BlogLivro.Configuracoes;
using BlogLivro.Models.Listas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class ListaController
    {
        [HttpPost, Route("api/lista/gravar")]
        public async Task<RetornoSucesso> GravarLista([FromBody] Lista lista)
        {
            if (lista == null)
                return new RetornoSucesso("Ops, Nenhuma lista enviada, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await lista.Salvar(conn);
        }

        [HttpDelete, Route("api/lista/excluir/{id}")]
        public async Task<RetornoSucesso> ExcluirLista(int id)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Lista.Excluir(id, conn);
        }

        [HttpGet, Route("api/lista/buscar")]
        public async Task<List<Lista>> BuscarLista([FromBody] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Lista.Buscar(filtro, conn);
        }
    }
}
