using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Models.Gatilhos
{
    public class Gatilho
    {
        public Int64 ID { get; set; }
        public string Nome { get; set; }

        public Gatilho()
        {
        }

        public Gatilho(string nome)
        {
            this.Nome = nome;
        }

        #region CRUD
        public static async Task<List<Gatilho>> SalvarEmMassa(List<Gatilho> gatilhos, SqlConnection conn)
        {
            var query = $@"
                declare @keySave table (ID integer, Nome varchar(200))

                INSERT INTO Gatilho
                (
                    Nome
                )
                OUTPUT INSERTED.ID, INSERTED.Nome 
                VALUES 
                    ('{(string.Join("'), ('", gatilhos.Select(x => x.Nome)))}');
                SELECT * FROM @keySave";

            return (await conn.QueryAsync<Gatilho>(query)).ToList();
        }

        public static async Task<RetornoSucesso> SalvarGatilhoLivroEmMassa(Int64 idLivro, List<Gatilho> gatilhos, SqlConnection conn)
        {
            var query = $@"
                INSERT INTO GatilhoLivro
                (
                    IDLivro, IDGatilho
                )
                VALUES                    
                    (@IDLivro, { (string.Join("), (@IDLivro, ", gatilhos.Select(x => x.ID)))});";

            await conn.QueryAsync<Gatilho>(query, new { IDLivro = idLivro });

            return new RetornoSucesso
            {
                Sucesso = true,
                Mensagem = "Gatilhos atribuídos ao livro com sucesso."
            };
        }

        public async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if(ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Gatilho
                    (
                        Nome
                    )
                    VALUES
                    (
                        @Nome
                    ); SELECT SCOPE_IDENTITY()", this)).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE Gatilho
                    SET
                        Nome = @Nome
                    WHERE
                        ID = @ID", this);
            }

            return new RetornoSucesso
            {
                ID = ID.ToString(),
                Sucesso = ID > 0,
                Objeto = this,
                Mensagem = "Gatilho inserido/atualizado."
            };
        }

        public static async Task<RetornoSucesso> SalvarGatilhoLivro(Int64 idGatilho, Int64 idLivro, SqlConnection conn)
        {
            int id = (await conn.QueryAsync<int>(@"
                INSERT INTO GatilhoLivro
                (
                    IDGatilho,
                    IDLivro
                )
                VALUES
                (
                    @IDGatilho,
                    @IDLivro
                ); SELECT SCOPE_IDENTITY()", new { IDGatilho = idGatilho, IDLivro = idLivro })).FirstOrDefault();

            return new RetornoSucesso
            {
                ID = id.ToString(),
                Sucesso = id > 0,
                Mensagem = "GatilhoLivro inserido/atualizado."
            };
        }

        public static async Task<RetornoSucesso> RemoverGatilhoLivro(Int64 idGatilho, Int64 idLivro, SqlConnection conn)
        {
            (await conn.QueryAsync<int>(@"
                DELETE FROM GatilhoLivro WHERE IDGatilho = @IDGatilho AND IDLivro = @IDLivro", new { IDGatilho = idGatilho, IDLivro = idLivro })).FirstOrDefault();

            return new RetornoSucesso
            {
                ID = idGatilho.ToString(),
                Sucesso = true,
                Mensagem = "GatilhoLivro removida."
            };
        }

        public static async Task<List<Gatilho>> Buscar(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Gatilho>(@"
                    SELECT 
                        ID,
                        Nome
                    FROM
                        Gatilho
                    WHERE
                        (@ID IS NULL OR Gatilho.ID = @ID) AND
                        (@Nome COLLATE Latin1_General_CI_AI IS NULL OR Gatilho.Nome LIKE '%' + @Nome + '%' COLLATE Latin1_General_CI_AI)
                    ORDER BY
                        Nome", filtro)).ToList();
        }
        #endregion

        #region Buscas

        public static async Task<List<Gatilho>> BuscarEmMassa(List<Gatilho> gatilhos, SqlConnection conn)
        {
            return (await conn.QueryAsync<Gatilho>(@"
            SELECT 
                    ID,
                    Nome
            FROM 
                Tag
            WHERE 
                Nome IN @Nome", new { Nome = gatilhos.Select(x => x.Nome) })).ToList();
        }

        //TODO : busca por gatilho / por livro / ambos
        public static List<Gatilho> BuscarGatilhoLivro(Filtro filtro, SqlConnection conn)
        {
            return conn.Query<Gatilho>(@"
                    SELECT 
                        Gatilho.ID,
                        Gatilho.Nome
                    FROM
                        Gatilho
                        INNER JOIN GatilhoLivro on GatilhoLivro.IDGatilho = Gatilho.ID
                    WHERE
                        (@ID IS NULL OR Gatilho.ID = @ID) AND
                        (@IDLivro IS NULL OR GatilhoLivro.IDLivro = @IDLivro) AND
                        (@Nome COLLATE Latin1_General_CI_AI IS NULL OR Gatilho.Nome LIKE '%' + @Nome + '%' COLLATE Latin1_General_CI_AI)", filtro).ToList();
        }
        #endregion

        #region Servico
        public static async Task<List<Gatilho>> InserirGatilhoEmMassa(List<Gatilho> gatilhos, SqlConnection conn)
        {
            return await SalvarEmMassa(gatilhos, conn);
        }

        public static async Task<RetornoSucesso> InserirGatilhoLivroEmMassa(Int64 idLivro, List<Gatilho> gatilhos, SqlConnection conn)
        {

            return await SalvarGatilhoLivroEmMassa(idLivro, gatilhos, conn);
        }

        public async Task<RetornoSucesso> InserirGatilhoLivro(Int64? idLivro, SqlConnection conn)
        {
            Gatilho gatilhoExistente = (await Gatilho.Buscar(new Filtro { Nome = this.Nome }, conn)).FirstOrDefault();
            Gatilho gatilhoLivroExistente = Gatilho.BuscarGatilhoLivro(new Filtro { Nome = this.Nome, IDLivro = idLivro }, conn).FirstOrDefault();

            if (gatilhoLivroExistente != null)
            {
                return new RetornoSucesso
                {
                    Sucesso = true,
                    Mensagem = "Este gatilho já está atribuido a este livro",
                    CodigoErro = Enums.CodigoErro.GatilhoJaExiste
                };
            }

            RetornoSucesso retorno = new RetornoSucesso();

            if (gatilhoExistente != null)
            {
                this.ID = gatilhoExistente.ID;
                this.Nome = gatilhoExistente.Nome;
                retorno.Objeto = this;
                retorno.Sucesso = true;
            }
            else
            {
                retorno = await Salvar(conn);
            }

            if (idLivro != null && retorno.Sucesso)
            {
                await Gatilho.SalvarGatilhoLivro(ID, idLivro.Value, conn);
            }
            return retorno;
        }
        #endregion
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public string Nome { get; set; }
        public Int64? IDLivro { get; set; }
    }
}
