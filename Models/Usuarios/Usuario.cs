using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BlogLivro.Enums;
using BlogLivro.Configuracoes;

namespace BlogLivro.Models.Usuarios
{
    public class Usuario
    {
        public int ID { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Senha { get; set; }
        [Compare("Senha")]
        public string ConfirmarSenha { get; set; }
        public Guid? GuidConfirmarEmail { get; set; }
        public string GuidConfirmarEmailString { get; set; }
        public Guid? GuidRecuperarSenha { get; set; }
        public string GuidRecuperarSenhaString { get; set; }

        public bool Ativo { get; set; }
        public string Nome { get; set; }
        public string NomeImagem { get; set; }
        public byte[] ImagemByte { get; set; }
        public string Imagem { get { return ImagemByte != null ? Convert.ToBase64String(ImagemByte) : string.Empty; } set { } }

        #region CRUD
        private async Task<RetornoSucesso> Salvar(SqlConnection conn)
        {
            if (ID == 0)
            {
                ID = (await conn.QueryAsync<int>(@"
                    INSERT INTO Usuario
                    (
                        LoginUsuario,
                        Senha,
                        Nome,
                        GuidConfirmarEmail,
                        Ativo
                    )
                    VALUES
                    (
                        @Email,
                        @Senha,
                        @Nome,
                        @GuidConfirmarEmail,
                        0
                    ); SELECT SCOPE_IDENTITY();", this)).FirstOrDefault();
            }
            else
            {
                await conn.ExecuteAsync(@"
                    Update Usuario
                    SET
                        LoginUsuario = @Email,
                        Senha = @Senha,
                        Nome = @Nome,
                        Ativo = @Ativo,
                        GuidConfirmarEmail = @GuidConfirmarEmail,
                        GuidRecuperarSenha = @GuidRecuperarSenha
                    WHERE 
                        ID = @ID", this);
            }

            Senha = string.Empty;
            ConfirmarSenha = string.Empty;

            return new RetornoSucesso
            {
                ID = ID.ToString(),
                Sucesso = ID > 0,
                Objeto = this,
                Mensagem = "Usuário inserido/atualizado"
            };
        }

        private async Task<RetornoSucesso> AtivarCadastro(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                Update Usuario
                SET
                    Ativo = 1
                WHERE 
                    ID = @ID", new { ID = this.ID });


            return new RetornoSucesso
            {
                ID = ID.ToString(),
                Sucesso = true,
                Objeto = this,
                Mensagem = "Usuário ativado com sucesso."
            };
        }

        private async Task<Usuario> BuscarLogin(SqlConnection conn)
        {
            Usuario usuario = (await conn.QueryAsync<Usuario>(@"
                SELECT 
                    Usuario.ID,
                    Usuario.LoginUsuario Email,
                    Usuario.Nome,
                    Usuario.Senha,
                    Usuario.Imagem ImagemByte,
                    Usuario.NomeImagem,
                    Usuario.GuidConfirmarEmail GuidConfirmarEmailString,
                    Usuario.GuidRecuperarSenha GuidRecuperarSenhaString,
                    Usuario.Ativo
                FROM 
                    Usuario
                WHERE
                    (Usuario.LoginUsuario COLLATE Latin1_General_CI_AI = @Email COLLATE Latin1_General_CI_AI AND Usuario.Senha = @Senha)", this)).FirstOrDefault();

            if (usuario != null)
            {
                usuario.GuidConfirmarEmail = String.IsNullOrWhiteSpace(usuario.GuidConfirmarEmailString) ? new Guid() : new Guid(usuario.GuidConfirmarEmailString);
                usuario.GuidRecuperarSenha = String.IsNullOrWhiteSpace(usuario.GuidRecuperarSenhaString) ? new Guid() : new Guid(usuario.GuidRecuperarSenhaString);
            }

                return usuario;
        }

        #endregion

        #region Buscas       
        public static async Task<Usuario> BuscarEmail(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Usuario>(@"
                SELECT 
                    Usuario.LoginUsuario Email
                FROM 
                    Usuario
                WHERE
                    (Usuario.LoginUsuario COLLATE Latin1_General_CI_AI = @Email COLLATE Latin1_General_CI_AI)", filtro)).FirstOrDefault();
        }

        public static async Task<List<Usuario>> BuscarUsuarios(Filtro filtro, SqlConnection conn)
        {
            return (await conn.QueryAsync<Usuario>(@"
            SELECT 
                Usuario.ID,
                Usuario.Nome,
                Usuario.LoginUsuario Email,
                Usuario.Imagem ImagemByte,
                Usuario.NomeImagem,
                Usuario.Ativo
            FROM 
                Usuario
            WHERE
                (@Buscar IS NULL OR ((Usuario.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @Buscar + '%' COLLATE Latin1_General_CI_AI) 
                    OR (Usuario.LoginUsuario COLLATE Latin1_General_CI_AI LIKE '%' + @Buscar + '%' COLLATE Latin1_General_CI_AI)))
            ORDER BY 
                Nome
            OFFSET  " + filtro.Paginacao.Deslocamento + @" ROWS 
            FETCH NEXT  " + filtro.Paginacao.Proxima + "ROWS ONLY", filtro)).ToList();
        }

        public static async Task<Usuario> BuscarUsuarioPorID(int id, SqlConnection conn)
        {
            return (await conn.QueryAsync<Usuario>(@"
            SELECT 
                Usuario.ID,
                Usuario.Nome,
                Usuario.LoginUsuario Email,
                Usuario.Imagem ImagemByte,
                Usuario.NomeImagem, 
                Usuario.Ativo
            FROM 
                Usuario
            WHERE
                Usuario.ID = @ID", new { ID = id })).FirstOrDefault();
        }

        public static async Task<Usuario> BuscarUsuarioPorEmail(string email, SqlConnection conn)
        {
            Usuario usuario = (await conn.QueryAsync<Usuario>(@"
            SELECT 
                Usuario.ID,
                Usuario.Nome,
                Usuario.LoginUsuario Email,
                Usuario.Imagem ImagemByte,
                Usuario.NomeImagem, 
                Usuario.GuidConfirmarEmail GuidConfirmarEmailString,
                Usuario.GuidRecuperarSenha GuidRecuperarSenhaString,
                Usuario.Ativo
            FROM 
                Usuario
            WHERE
                Usuario.LoginUsuario COLLATE Latin1_General_CI_AI = @Email COLLATE Latin1_General_CI_AI", new { Email = email })).FirstOrDefault();

            if (usuario != null)
            {
                usuario.GuidConfirmarEmail = String.IsNullOrWhiteSpace(usuario.GuidConfirmarEmailString) ? new Guid() : new Guid(usuario.GuidConfirmarEmailString);
                usuario.GuidRecuperarSenha = String.IsNullOrWhiteSpace(usuario.GuidRecuperarSenhaString) ? new Guid() : new Guid(usuario.GuidRecuperarSenhaString);
            }

            return usuario;
        }

        public static async Task<Usuario> BuscarUsuarioPorGuid(string guid, SqlConnection conn)
        {
            Usuario usuario = (await conn.QueryAsync<Usuario>(@"
            SELECT 
                Usuario.ID,
                Usuario.Nome,
                Usuario.LoginUsuario Email,
                Usuario.Imagem ImagemByte,
                Usuario.NomeImagem, 
                Usuario.GuidConfirmarEmail GuidConfirmarEmailString,
                Usuario.GuidRecuperarSenha GuidRecuperarSenhaString,
                Usuario.Ativo
            FROM 
                Usuario
            WHERE
                Usuario.GuidConfirmarEmail = @Guid OR Usuario.GuidRecuperarSenha = @Guid ", new { Guid = guid })).FirstOrDefault();

            if (usuario != null)
            {
                usuario.GuidConfirmarEmail = String.IsNullOrWhiteSpace(usuario.GuidConfirmarEmailString) ? new Guid() : new Guid(usuario.GuidConfirmarEmailString) ;
                usuario.GuidRecuperarSenha = String.IsNullOrWhiteSpace(usuario.GuidRecuperarSenhaString) ? new Guid() : new Guid(usuario.GuidRecuperarSenhaString);
            }

                return usuario;
        }
        #endregion


        #region Serviços
        public async Task<RetornoSucesso> Ativar(SqlConnection conn)
        {
            return await AtivarCadastro(conn);
        }

        public async Task<RetornoSucesso> RecuperarSenha(SqlConnection conn)
        {
            this.Senha = null;
            this.GuidRecuperarSenha = Guid.NewGuid();
            return await Salvar(conn);
        }

        public async Task<RetornoSucesso> AtualizarSalvar(SqlConnection conn)
        {
            if (!ConfirmarSenha.Equals(Senha))
                return new RetornoSucesso("As senhas não conferem.", CodigoErro.SenhasIncompativeis);

            ConfirmarSenha = string.Empty;

            if (await BuscarEmail(new Filtro { Email = this.Email }, conn) != null)
                return new RetornoSucesso("Este email já está cadastrado no sistema.", CodigoErro.EmailJaCadatrado);

            Senha = Configuracao.EncryptString(Senha);
            GuidConfirmarEmail = Guid.NewGuid();
            return await Salvar(conn);
        }

        public async Task<RetornoSucesso> RedefinirSenha(Usuario usuario, SqlConnection conn)
        {
            return await RedefinirSenhaSalvar(usuario, conn);
        }

        private async Task<RetornoSucesso> RedefinirSenhaSalvar(Usuario usuario, SqlConnection conn)
        {
            if (!usuario.ConfirmarSenha.Equals(usuario.Senha))
                return new RetornoSucesso("As senhas não conferem.", CodigoErro.SenhasIncompativeis);

            ConfirmarSenha = string.Empty;

            this.Senha = Configuracao.EncryptString(usuario.Senha);
            this.GuidRecuperarSenha = null;

            return await Salvar(conn);
        }

        public async Task<RetornoSucesso> Logar(SqlConnection conn)
        {
            Senha = Configuracao.EncryptString(Senha);

            Usuario usuario = await BuscarLogin(conn);
            if (usuario is null)
                return new RetornoSucesso("Email ou senha incorretos.", CodigoErro.ErroAoLogar);

            if (!usuario.Ativo)
                return new RetornoSucesso("Este usuário ainda não está ativo, confirme o email para logar.", CodigoErro.ErroAoLogar);

            usuario.Senha = null;
            return new RetornoSucesso() { Sucesso = true, Objeto = usuario, ID = usuario.ID.ToString() };
        }
        #endregion
    }

    public class Filtro
    {
        public Int64? ID { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Buscar { get; set; }

        public Paginacao Paginacao { get; set; }
    }
}
