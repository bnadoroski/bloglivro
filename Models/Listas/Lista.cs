using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Models.Listas
{
    public class Lista
    {
        public Int64 ID { get; set; }
        public string Descricao{ get; set; }
        public string Selecionados { get; set; }
        public byte[] Imagens { get; set; }

        #region CRUD
        public async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if(ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Lista
                    (
                        Descricao,
                        Selecionados,
                        Imagens
                    )
                    VALUES
                    (
                        @Descricao,
                        @Selecionados,
                        @Imagens
                    )
                ")).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE Lista
                    SET
                        Descricao = @Descricao,
                        Selecionados = @Selecionados,
                        Imagens = @Imagens
                    WHERE
                        ID = @ID
                ");
            }

            return new RetornoSucesso
            {
                Sucesso = ID > 0,
                Mensagem = "Lista inserida/atualizada."
            };
        }

        public static async Task<List<Lista>> Buscar(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Lista>(@"
                SELECT
                    ID,
                    Descricao,
                    Selecionados,
                    Imagens
                FROM
                    Lista
                WHERE
                    (@ID IS NULL OR Lista.ID = @ID) AND
                    (@Descricao IS NULL OR Lista.Descricao LIKE '%@Descricao%') AND
                    (@IDUsuario IS NULL OR Lista.IDUsuario = @IDUsuario)
            ", filtro)).ToList();
        }

        public static async Task<RetornoSucesso> Excluir(Int64 id, SqlConnection conn)
        {
            return new RetornoSucesso
            {
                Sucesso = (await conn.ExecuteAsync(@"DELETE FROM Lista WHERE ID = @ID", new { ID = id }) > 0),
                Mensagem = "Lista excluida com sucesso."
            };
        }
        #endregion

        #region Buscas
        #endregion
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public string Descricao { get; set; }
        public int? IDUsuario { get; set; }
    }
}
