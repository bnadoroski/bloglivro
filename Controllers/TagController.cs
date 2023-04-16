using BlogLivro.Configuracoes;
using BlogLivro.Models.Tags;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class TagController
    {
        [HttpPost, Route("api/tag/gravar"), Route("api/tag/gravar/{idLivro}")]
        public async Task<RetornoSucesso> GravarTag(string nome, Int64? idLivro = null)
        {
            if (nome == null)
                return new RetornoSucesso("Ops, Nenhuma tag enviada, tente novamente.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Tag tag = new Tag(nome);
                return await tag.InserirTagLivro(idLivro, conn);
            }
        }

        [HttpDelete, Route("api/tag/{id}/remover/{idLivro}")]
        public async Task<RetornoSucesso> RemoverTag(Int64 id, Int64 idLivro)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Tag.RemoverTagLivro(id, idLivro, conn);
        }

        [HttpPost, Route("api/tag/buscar")]
        public async Task<List<Tag>> BuscarTag([FromBody] Filtro filtro)
        {
            if (filtro == null)
                filtro = new Filtro();

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await Tag.Buscar(filtro, conn);
        }

        [HttpPost, Route("api/tag/teste")]
        public async Task Teste()
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                List<Tag> tags = new List<Tag>()
                {
                   new Tag{ Nome = "Teste" },
                   new Tag{ Nome = "teste1" },
                   new Tag{ Nome = "teste2" }
                };

               await Tag.SalvarEmMassa(tags, conn);
            }
        }
    }
}
