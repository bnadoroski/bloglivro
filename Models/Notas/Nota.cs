using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogLivro.Models.Livros;
using BlogLivro.Models.Usuarios;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BlogLivro.Models.Notas
{
    public class Nota
    {
        public Int64 ID { get; set; }
        public Usuario Usuario { get; set; }
        public int IDUsuario { get; set; }
        public Livro Livro { get; set; }
        public Int64 IDLivro { get; set; }
        public decimal Valor { get; set; }
        public decimal ValorMinhaNota { get; set; }
        public int QuantidadeNota { get; set; }

        #region CRUD
        public async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if (ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Nota
                    (
                        IDUsuario,
                        IDLivro,
                        Valor
                    )
                    VALUES
                    (
                        @IDUsuario,
                        @IDLivro,
                        @Valor
                    ); SELECT SCOPE_IDENTITY()", this)).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE Nota
                    SET
                        IDUsuario = @IDUsuario,
                        IDLivro = @IDLivro,
                        Valor = @Valor
                    WHERE 
                        ID = @ID", this);
            }

            return new RetornoSucesso
            {
                ID = ID.ToString(),
                Sucesso = ID > 0,
                Objeto = this,
                Mensagem = "Nota inserida/atualizada"
            };
        }

        public static async Task<Nota> Buscar(Filtro filtro, SqlConnection conn)
        {
            string query = $@"
                    SELECT 
                        Nota.IDLivro,
                        AVG(ALL Nota.Valor) Valor,
                        COUNT(Nota.Valor) QuantidadeNota
						{(filtro.IDUsuario != null ? ", (SELECT TOP 1 Valor FROM Nota WHERE IDUsuario = @IDUsuario) AS ValorMinhaNota" : "")}
                    FROM
                        Nota
                    WHERE
                        (@IDLivro IS NULL OR Nota.IDLivro = @IDLivro)
                    GROUP BY IDLivro";
            return (await conn.QueryAsync<Nota>(query, filtro)).FirstOrDefault();
        }

        public static async Task<RetornoSucesso> Excluir(Filtro filtro, SqlConnection conn)
        {
            return new RetornoSucesso
            {
                Sucesso = (await conn.ExecuteAsync(@"DELETE FROM Nota WHERE IDUsuario = @IDUsuario AND IDLivro = @IDLivro", filtro) > 0),
                Mensagem = "Nota excluída com sucesso."
            };
        }
        #endregion

        #region Buscas
        public static Nota BuscarNotaLivro(Filtro filtro, SqlConnection conn)
        {
            string query = $@"
                    SELECT 
                        Nota.IDLivro,
                        AVG(ALL Nota.Valor) Valor,
                        COUNT(Nota.Valor) QuantidadeNota
						{(filtro.IDUsuario != null ? ", (SELECT TOP 1 Valor FROM Nota WHERE IDUsuario = @IDUsuario) AS ValorMinhaNota" : "")}
                    FROM
                        Nota
                    WHERE
                        (@IDLivro IS NULL OR Nota.IDLivro = @IDLivro)
                    GROUP BY IDLivro";
            return conn.Query<Nota>(query, filtro).FirstOrDefault();
        }
        #endregion
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public int? IDUsuario { get; set; }
        public Int64? IDLivro { get; set; }
        public string Valor { get; set; }
    }
}
