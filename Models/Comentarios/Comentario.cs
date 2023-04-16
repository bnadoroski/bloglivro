using BlogLivro.Models.Usuarios;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Models.Comentarios
{
    public enum TipoComentario
    {
        [Description("Comentários")]
        Comentario = 1,
        [Description("Imagens")]
        Imagem = 2,
        [Description("Resenhas")]
        Resenha = 3
    }

    public class Comentario
    {
        public Int64 ID { get; set; }
        public Usuario Usuario{ get; set; }
        public int IDUsuario { get; set; }
        public string Texto { get; set; }
        public TipoComentario Tipo { get; set; }
        public byte[] Imagem { get; set; }

        public async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if (ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Comentario
                    (
                        IDUsuario,
                        Texto,
                        Tipo,
                        Imagem
                    )
                    VALUES
                    (
                        @IDUsuario,
                        @Texto,
                        @Tipo,
                        @Imagem
                    )
                ")).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE Comentario
                    SET
                        IDUsuario = @IDUsuario,
                        Texto = @Texto,
                        Tipo = @Tipo,
                        Imagem = @Imagem
                    WHERE
                        ID = @ID
                ");
            }

            return new RetornoSucesso
            {
                Sucesso = ID > 0,
                Mensagem = "Comentário inserido/atualizado."
            };
        }

        public static async Task<List<Comentario>> Buscar(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Comentario>(@"
                SELECT
                    ID,
                    IDUsuario,
                    Texto,
                    Tipo,
                    Imagem
                FROM
                    Comentario
                WHERE
                    (@ID IS NULL OR Comentario.ID = @ID) AND
                    (@IDUsuario IS NULL OR Comentario.IDUsuario = @IDUsuario)
                    (@Tipo IS NULL OR Comentario.Tipo = @Tipo) AND
            ", filtro)).ToList();
        }

        public static async Task<RetornoSucesso> Excluir(Int64 id, SqlConnection conn)
        {
            return new RetornoSucesso
            {
                Sucesso = (await conn.ExecuteAsync(@"DELETE FROM Comentario WHERE ID = @ID", new { ID = id }) > 0),
                Mensagem = "Comentário excluido com sucesso."
            };
        }
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public int? IDUsuario { get; set; }
        public TipoComentario? Tipo { get; set; }
    }
}
