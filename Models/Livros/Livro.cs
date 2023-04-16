using BlogLivro.Models.Notas;
using BlogLivro.Models.Tags;
using BlogLivro.Models.Gatilhos;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BlogLivro.Models.Livros
{
    public class Livro
    {
        public Int64 ID { get; set; }
        public string Nome { get; set; }
        public string Sinopse { get; set; }
        public string Autor { get; set; }
        public string Colecao { get; set; }
        public byte[] ImagemByte { get; set; }
        private string imagem;
        public string Imagem { get { return imagem != null ? imagem : ImagemByte != null ? Convert.ToBase64String(ImagemByte) : string.Empty; } set { imagem = value; } }
        public string NomeImagem { get; set; }
        public Nota Nota { get; set; }
        public bool Ativo { get; set; }
        public int QuantidadeTags { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Gatilho> Gatilhos { get; set; }

        #region CRUD
        private async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if (ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Livro
                    (
                        Nome,
                        Sinopse,
                        Autor,
                        Colecao,
                        Ativo
                    )
                    VALUES
                    (
                        @Nome,
                        @Sinopse,
                        @Autor,
                        @Colecao,
                        1
                    ); SELECT SCOPE_IDENTITY();", this)).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    Update Livro
                    SET
                        Nome = @Nome,
                        Sinopse = @Sinopse,
                        Autor = @Autor,
                        Colecao = @Colecao
                    WHERE 
                        ID = @ID", this);
            }

            return new RetornoSucesso
            {
                ID = ID.ToString(),
                Sucesso = ID > 0,
                Objeto = this,
                Mensagem = "Livro inserido/atualizado"
            };
        }

        private async Task SalvarLivroImagem(SqlConnection conn)
        {

            await conn.QueryAsync<int>(@"
                    INSERT INTO LivroImagem
                    (
                        IDLivro,
                        Nome,
                        Imagem
                    )
                    VALUES
                    (
                        @ID,
                        @NomeImagem,
                        @ImagemByte
                    ); SELECT SCOPE_IDENTITY();", this);

            //    await conn.ExecuteAsync(@"
            //        Update LivroImagem
            //        SET
            //            Nome = @NomeImagem,
            //            Imagem = @ImagemByte
            //        WHERE 
            //            IDLivro = @ID", this);

        }

        public static async Task<List<Livro>> Buscar(Filtro filtro, SqlConnection conn)
        {
            string query = $@"
            SELECT 
                Livro.ID,
                Livro.Nome,
                Livro.Sinopse,
                Livro.Autor,
                Livro.Colecao,
                Livro.Ativo,
                COALESCE(LivroImagem.Nome, (SELECT Nome FROM LivroImagem WHERE IDLivro IS NULL)) NomeImagem,
                COALESCE(LivroImagem.Imagem, (SELECT Imagem FROM LivroImagem WHERE IDLivro IS NULL)) ImagemByte
				{(!string.IsNullOrWhiteSpace(filtro.Buscar) ? ", COUNT(n.item) Quantidade" : "")}
            FROM 
                Livro
            LEFT JOIN 
                LivroImagem ON Livro.ID = LivroImagem.IDLivro
				{(!string.IsNullOrWhiteSpace(filtro.Buscar) ? @"JOIN dbo.Split(@Buscar,',') n ON Livro.Nome COLLATE Latin1_General_CI_AI LIKE N'%'+n.item+'%' COLLATE Latin1_General_CI_AI OR 
				Livro.Autor COLLATE Latin1_General_CI_AI LIKE N'%'+n.item+'%' COLLATE Latin1_General_CI_AI OR
				Livro.Colecao COLLATE Latin1_General_CI_AI LIKE N'%'+n.item+'%' COLLATE Latin1_General_CI_AI OR
				Livro.Sinopse COLLATE Latin1_General_CI_AI LIKE N'%'+n.item+'%' COLLATE Latin1_General_CI_AI " : " ")}
            WHERE
                (@ID IS NULL OR Livro.ID = @ID) AND
                (@Nome IS NULL OR Livro.Nome COLLATE Latin1_General_CI_AI LIKE '%'+ @Nome + '%' COLLATE Latin1_General_CI_AI) AND
                (@Autor IS NULL OR Livro.Autor COLLATE Latin1_General_CI_AI LIKE '%' + @Autor + '%' COLLATE Latin1_General_CI_AI) AND
                (@Colecao IS NULL OR Livro.Colecao COLLATE Latin1_General_CI_AI LIKE '%' + @Colecao + '%' COLLATE Latin1_General_CI_AI) AND
                (@ApenasAtivos = 0 OR (Livro.Ativo = 1 AND @ApenasAtivos = 1))
			GROUP BY Livro.ID, Livro.Nome, Livro.Sinopse, Livro.Autor, Livro.Colecao, Livro.Ativo, LivroImagem.Nome, LivroImagem.Imagem
            ORDER BY 
				{(!string.IsNullOrWhiteSpace(filtro.Buscar) ? "Quantidade desc," : "")} Nome
            OFFSET {filtro.Paginacao.Deslocamento} ROWS 
            FETCH NEXT {filtro.Paginacao.Proxima} ROWS ONLY";

            return (await conn.QueryAsync<Livro>(query, filtro)).ToList();
        }

        public static async Task<List<Livro>> BuscarPorTag(Filtro filtro, SqlConnection conn)
        {
            string query = @"
            SELECT 
		        COUNT(Livro.ID) QuantidadeTags,
		        Livro.ID,
		        Livro.Nome,
		        Livro.Sinopse,
		        Livro.Autor,
		        Livro.Colecao,
		        Livro.Ativo,
		        COALESCE(LivroImagem.Nome, (SELECT Nome FROM LivroImagem WHERE IDLivro IS NULL)) NomeImagem,
		        COALESCE(LivroImagem.Imagem, (SELECT Imagem FROM LivroImagem WHERE IDLivro IS NULL)) ImagemByte
	        FROM 
		        Livro
	        LEFT JOIN TagLivro on TagLivro.IDLivro = Livro.ID
	        LEFT JOIN Tag on Tag.ID = TagLivro.IDTag
	        LEFT JOIN LivroImagem ON Livro.ID = LivroImagem.IDLivro
	        WHERE 
				(Livro.ID != @IDRelacionado) AND "
                + (filtro.TagsPesquisa != null && filtro.TagsPesquisa.Length > 0 ? "((Tag.ID IN @TagsPesquisa) OR " : " (") + ""
                + (String.IsNullOrWhiteSpace(filtro.Colecao) ? "" : "(Livro.Colecao COLLATE Latin1_General_CI_AI LIKE '%'+ @Colecao + '%' COLLATE Latin1_General_CI_AI) OR ") + ""
                + (String.IsNullOrWhiteSpace(filtro.Autor) ? "" : "(Livro.Autor COLLATE Latin1_General_CI_AI LIKE '%'+ @Autor + '%' COLLATE Latin1_General_CI_AI) OR ") + ""
                + (String.IsNullOrWhiteSpace(filtro.Nome) ? "" : @"(Livro.Nome COLLATE Latin1_General_CI_AI LIKE '%'+ @Nome + '%' COLLATE Latin1_General_CI_AI OR 
                    Livro.Sinopse COLLATE Latin1_General_CI_AI LIKE '%' + @Nome + '%' COLLATE Latin1_General_CI_AI)) ") + @"
	        GROUP BY Livro.ID, TagLivro.IDLivro, Livro.Nome, Livro.Sinopse, Livro.Autor, Livro.Colecao, Livro.Ativo, LivroImagem.Nome, LivroImagem.Imagem
            ORDER BY" + (String.IsNullOrWhiteSpace(filtro.Colecao) ? "" : " CASE WHEN Livro.Colecao LIKE '%'+ @Colecao + '%' COLLATE Latin1_General_CI_AI THEN 1 ELSE 2 END, ") +
            @" QuantidadeTags desc, Livro.Nome asc
            OFFSET " + filtro.Paginacao.Deslocamento + @" ROWS 
            FETCH NEXT " + filtro.Paginacao.Proxima + "ROWS ONLY";

            return (await conn.QueryAsync<Livro>(query, filtro)).ToList();
        }

        public static async Task<RetornoSucesso> Excluir(Int64 id, SqlConnection conn)
        {
            return new RetornoSucesso
            {
                Sucesso = (await conn.ExecuteAsync("UPDATE Livro SET Ativo = 0 WHERE ID = @ID", new { ID = id })) > 0,
                Mensagem = "Livro Excluído com sucesso"
            };
        }
        #endregion

        #region Buscas
        public static async Task<List<Livro>> BuscarNotaLivro(Filtro filtro, SqlConnection conn)
        {
            //TODO : tirar a media das notas e salvar na entidade nota
            return (await conn.QueryAsync<Livro>(@"
                SELECT 
                    ID,
                    Nome,
                    Sinopse,
                    Autor,
                    Colecao
                FROM 
                    Livro
                WHERE
                    (@ID IS NULL OR Livro.ID = @ID) AND
                    (@Nome IS NULL OR Livro.Nome LIKE '%@Nome%') AND
                    (@Autor IS NULL OR Livro.Autor = @Autor) AND
                    (@Colecao IS NULL OR Livro.Colecao = @Colecao)
            ", filtro)).ToList();
        }

        public static async Task<Livro> BuscarLivroCompleto(Filtro filtro, SqlConnection conn)
        {
            //TODO : buscar as tags
            return (await conn.QueryAsync(@"
                SELECT 
                    Livro.ID,
                    Livro.Nome,
                    Livro.Sinopse,
                    Livro.Autor,
                    Livro.Colecao,
                    COALESCE(LivroImagem.Nome, (SELECT Nome FROM LivroImagem WHERE IDLivro IS NULL)) NomeImagem,
                    COALESCE(LivroImagem.Imagem, (SELECT Imagem FROM LivroImagem WHERE IDLivro IS NULL)) ImagemByte
                FROM 
                    Livro                
                LEFT JOIN LivroImagem ON Livro.ID = LivroImagem.IDLivro
                LEFT JOIN TagLivro on TagLivro.IDLivro = Livro.ID
				LEFT JOIN Tag on Tag.ID = TagLivro.IDTag
                LEFT JOIN GatilhoLivro on GatilhoLivro.IDLivro = Livro.ID
				LEFT JOIN Gatilho on Gatilho.ID = GatilhoLivro.IDGatilho
                WHERE
                    (@ID IS NULL OR Livro.ID = @ID) AND
                    (@Nome IS NULL OR Livro.Nome LIKE '%@Nome%') AND
                    (@Autor IS NULL OR Livro.Autor = @Autor) AND
                    (@Colecao IS NULL OR Livro.Colecao = @Colecao)
                OPTION (RECOMPILE)
            ", new[] { typeof(Livro) }, obj =>
             {
                 Livro livro = obj[0] as Livro;
                 livro.Tags = Tag.BuscarTagLivro(new Tags.Filtro() { IDLivro = livro.ID }, conn);
                 livro.Gatilhos = Gatilho.BuscarGatilhoLivro(new Gatilhos.Filtro() { IDLivro = livro.ID }, conn);
                 livro.Nota = Nota.BuscarNotaLivro(new Notas.Filtro() { IDLivro = livro.ID, IDUsuario = filtro.IDUsuario }, conn);
                 return livro;
             }, filtro)).FirstOrDefault();
        }
        #endregion

        #region serviços
        public async Task<RetornoSucesso> SalvarAtualizar(SqlConnection conn)
        {
            RetornoSucesso retorno = await Salvar(conn);
            if (!retorno.Sucesso)
                return retorno;

            if (this.Imagem != null)
            {
                this.ImagemByte = Convert.FromBase64String(this.Imagem.Substring(23));
                await SalvarLivroImagem(conn);
            }

            if (this.Tags.Count > 0)
            {
                List<Tag> tagsExistentes = await Tag.BuscarEmMassa(this.Tags, conn);
                List<Tag> tagsInexistentes = this.Tags.Where(x => !tagsExistentes.Any(y => y.Nome.ToLower() == x.Nome.ToLower())).ToList();
                if (tagsInexistentes.Count > 0)
                    tagsInexistentes = await Tag.InserirTagEmMassa(tagsInexistentes, conn);

                await Tag.InserirTagLivroEmMassa(this.ID, tagsExistentes.Concat(tagsInexistentes).ToList(), conn);
            }

            if (this.Gatilhos.Count > 0)
            {
                List<Gatilho> gatilhosExistentes = await Gatilho.BuscarEmMassa(this.Gatilhos, conn);
                List<Gatilho> gatilhosInexistentes = this.Gatilhos.Where(x => !gatilhosExistentes.Any(y => y.Nome.ToLower() == x.Nome.ToLower())).ToList();
                if (gatilhosInexistentes.Count > 0)
                    gatilhosInexistentes = await Gatilho.InserirGatilhoEmMassa(gatilhosInexistentes, conn);

                await Gatilho.InserirGatilhoLivroEmMassa(this.ID, gatilhosExistentes.Concat(gatilhosInexistentes).ToList(), conn);
            }

            return retorno;
        }

        public static async Task<List<Livro>> BuscarLivros(Filtro filtro, SqlConnection conn)
        {
            if (!string.IsNullOrWhiteSpace(filtro.Buscar)) { 
                filtro.Buscar = String.Join(" ", filtro.Buscar.Split(' ').Where(x => x.Length > 2).ToArray());
                filtro.Buscar = filtro.Buscar.Replace(" ", ",");
            }

            return await Livro.Buscar(filtro, conn);
        }
        #endregion
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public Int64? IDRelacionado { get; set; }
        public int? IDUsuario { get; set; }
        public string Nome { get; set; }
        public string Autor { get; set; }
        public string Buscar { get; set; }
        public string Colecao { get; set; }
        public bool ApenasAtivos { get; set; }
        public int[] TagsPesquisa { get; set; }

        public Paginacao Paginacao { get; set; }
    }
}
