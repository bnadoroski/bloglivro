using BlogLivro.Configuracoes;
using BlogLivro.Models.Gatilhos;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class GatilhoController
    {
        [HttpPost, Route("api/gatilho/gravar"), Route("api/gatilho/gravar/{idLivro}")]
        public async Task<RetornoSucesso> GravarGatilho(string nome, Int64? idLivro = null)
        {
            if (nome == null)
                return new RetornoSucesso("Ops, Nenhum gatilho enviado, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Gatilho gatilho = new Gatilho(nome);
                return await gatilho.InserirGatilhoLivro(idLivro, conn);
            }
        }

        [HttpDelete, Route("api/gatilho/{id}/remover/{idLivro}")]
        public async Task<RetornoSucesso> RemoverGatilho(Int64 id, Int64 idLivro)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Gatilho.RemoverGatilhoLivro(id, idLivro, conn);
        }

        [HttpPost, Route("api/gatilho/buscar")]
        public async Task<List<Gatilho>> BuscarGatilho([FromBody] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Gatilho.Buscar(filtro, conn);
        }
    }
}
