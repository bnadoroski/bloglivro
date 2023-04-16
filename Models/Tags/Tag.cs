using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Models.Tags
{
    public class Tag
    {
        public Int64 ID { get; set; }
        public string Nome { get; set; }

        public Tag()
        {
        }

        public Tag(string nome)
        {
            this.Nome = nome;
        }

        #region CRUD
        public async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if(ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Tag
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
                    UPDATE Tag
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
                Mensagem = "Tag inserida/atualizada."
            };
        }

        public static async Task<List<Tag>> SalvarEmMassa(List<Tag> tags, SqlConnection conn)
        {
            var query = $@"
                declare @keySave table (ID integer, Nome varchar(200))

                INSERT INTO Tag
                (
                    Nome
                )
                OUTPUT INSERTED.ID, INSERTED.Nome 
                VALUES 
                    ('{(string.Join("'), ('", tags.Select(x => x.Nome)))}');
                SELECT * FROM @keySave";

            return (await conn.QueryAsync<Tag>(query)).ToList();
        }

        public static async Task<RetornoSucesso> SalvarTagLivroEmMassa(Int64 idLivro, List<Tag> tags, SqlConnection conn)
        {
            var query = $@"
                INSERT INTO TagLivro
                (
                    IDLivro, IDTag
                )
                VALUES                    
                    (@IDLivro, { (string.Join("), (@IDLivro, ", tags.Select(x => x.ID)))});";

            await conn.QueryAsync<Tag>(query, new { IDLivro = idLivro});

            return new RetornoSucesso
            {
                Sucesso = true,
                Mensagem = "Tags atribuídas ao livro com sucesso."
            };
        }

        public static async Task<RetornoSucesso> SalvarTagLivro(Int64 idTag, Int64 idLivro, SqlConnection conn)
        {
            int id = (await conn.QueryAsync<int>(@"
                INSERT INTO TagLivro
                (
                    IDTag,
                    IDLivro
                )
                VALUES
                (
                    @IDTag,
                    @IDLivro
                ); SELECT SCOPE_IDENTITY()", new { IDTag = idTag, IDLivro = idLivro })).FirstOrDefault();

            return new RetornoSucesso
            {
                ID = id.ToString(),
                Sucesso = id > 0,
                Mensagem = "TagLivro inserida/atualizada."
            };
        }

        public static async Task<RetornoSucesso> RemoverTagLivro(Int64 idTag, Int64 idLivro, SqlConnection conn)
        {
            (await conn.QueryAsync<int>(@"
                DELETE FROM TagLivro WHERE IDTag = @IDTag AND IDLivro = @IDLivro", new { IDTag = idTag, IDLivro = idLivro })).FirstOrDefault();

            return new RetornoSucesso
            {
                ID = idTag.ToString(),
                Sucesso = true,
                Mensagem = "TagLivro removida."
            };
        }

        public static async Task<List<Tag>> Buscar(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Tag>(@"
                    SELECT 
                        ID,
                        Nome
                    FROM
                        Tag
                    WHERE
                        (@ID IS NULL OR Tag.ID = @ID) AND
                        (@Nome COLLATE Latin1_General_CI_AI IS NULL OR Tag.Nome LIKE '%' + @Nome + '%' COLLATE Latin1_General_CI_AI)
                    ORDER BY
                        Nome", filtro)).ToList();
        }
        #endregion

        #region Buscas

        public static async Task<List<Tag>> BuscarEmMassa(List<Tag> tags, SqlConnection conn)
        {
            return (await conn.QueryAsync<Tag>(@"
            SELECT 
                    ID,
                    Nome
            FROM 
                Tag
            WHERE 
                Nome IN @Nome", new { Nome = tags.Select(x => x.Nome) })).ToList();
        }

        //TODO : busca por pessoa / por livro / ambos
        public static List<Tag> BuscarTagLivro(Filtro filtro, SqlConnection conn)
        {
            return conn.Query<Tag>(@"
                    SELECT 
                        Tag.ID,
                        Tag.Nome
                    FROM
                        Tag
                        INNER JOIN TagLivro on TagLivro.IDTag = Tag.ID
                    WHERE
                        (@ID IS NULL OR Tag.ID = @ID) AND
                        (@IDLivro IS NULL OR TagLivro.IDLivro = @IDLivro) AND
                        (@Nome COLLATE Latin1_General_CI_AI IS NULL OR Tag.Nome LIKE '%' + @Nome + '%' COLLATE Latin1_General_CI_AI)", filtro).ToList();
        }
        #endregion

        #region Servico
        public async Task<RetornoSucesso> InserirTagLivro(Int64? idLivro, SqlConnection conn)
        {
            Tag tagExistente = (await Tag.Buscar(new Filtro { Nome = this.Nome}, conn)).FirstOrDefault();
            Tag tagLivroExistente = Tag.BuscarTagLivro(new Filtro { Nome = this.Nome, IDLivro = idLivro }, conn).FirstOrDefault();

            if (tagLivroExistente != null)
            {
                return new RetornoSucesso
                {
                    Sucesso = true,
                    Mensagem = "Esta tag já está atribuida a este livro",
                    CodigoErro = Enums.CodigoErro.TagJaExiste
                };
            }

            RetornoSucesso retorno = new RetornoSucesso();

            if (tagExistente != null)
            {
                this.ID = tagExistente.ID;
                this.Nome = tagExistente.Nome;
                retorno.Objeto = this;
                retorno.Sucesso = true;
            } else
            {
                retorno = await Salvar(conn);
            }

            if (idLivro != null && retorno.Sucesso)
            {
                await Tag.SalvarTagLivro(ID, idLivro.Value, conn);
            }
            return retorno;
        }

        public static async Task<List<Tag>> InserirTagEmMassa(List<Tag> tags, SqlConnection conn)
        {
            return await SalvarEmMassa(tags, conn);
        }

        public static async Task<RetornoSucesso> InserirTagLivroEmMassa(Int64 idLivro, List<Tag> tags, SqlConnection conn)
        {

            return await SalvarTagLivroEmMassa(idLivro, tags, conn);
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
