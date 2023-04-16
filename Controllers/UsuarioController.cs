using BlogLivro.Configuracoes;
using BlogLivro.Models.Emails;
using BlogLivro.Models.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BlogLivro.Controllers
{
    public class UsuarioController
    {

        [HttpPost, Route("api/usuario/salvar")]
        public async Task<RetornoSucesso> CadastrarUsuario([FromForm] Usuario usuario)
        {
            if (usuario == null)
                return new RetornoSucesso("Ops, um erro ocorreu ao cadastrar usuário.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                RetornoSucesso ret = await usuario.AtualizarSalvar(conn);
                if (ret.Sucesso)
                {
                    Email email = Email.ConfirmarEmail(usuario);
                    ConfiguracaoEmail configuracaoEmail = await ConfiguracaoEmail.PegarConfiguracaoEmail();
                    await Email.SendEmailAsync(email, configuracaoEmail);
                }
                return ret;
            }
        }

        [HttpPost, Route("api/usuario/logar")]
        public async Task<RetornoSucesso> Logar([FromForm] Usuario usuario)
        {
            if (usuario == null)
                return new RetornoSucesso("Ops, um erro ocorreu ao logar.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
                return await usuario.Logar(conn);
        }

        [HttpPost, Route("api/usuario/redefinirsenha")]
        public async Task<RetornoSucesso> RedefinirSenha([FromForm] Usuario usuario)
        {
            if (usuario == null)
                return new RetornoSucesso("Ops, um erro ocorreu ao redefinir a senha.");

            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Usuario usuarioDB = await Usuario.BuscarUsuarioPorGuid(usuario.GuidRecuperarSenhaString, conn);

                if (usuarioDB == null)
                    return new RetornoSucesso("Ops, um erro ocorreu ao redefinir a senha.");

                return await usuarioDB.RedefinirSenha(usuario, conn);
            }
        }

        [HttpGet, Route("api/usuario/buscar/{id}")]
        public async Task<Usuario> Buscar(int id)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao()) 
                return await Usuario.BuscarUsuarioPorID(id, conn);
        }


        [HttpPost, Route("api/usuario/confirmaremail/{guid}")]
        public async Task<RetornoSucesso> ConfirmarEmail(string guid)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Usuario usuario = await Usuario.BuscarUsuarioPorGuid(guid, conn);

                if (usuario == null)
                    return new RetornoSucesso("Ops, um erro ocorreu ao confirmar email.");

                RetornoSucesso ret = await usuario.Ativar(conn);
                ret.Mensagem = "Email confirmado com sucesso.";
                return ret;
            }
        }

        [HttpPost, Route("api/usuario/recuperarsenha/{emailString}")]
        public async Task<RetornoSucesso> RecuperarSenha(string emailString)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Usuario usuario = await Usuario.BuscarUsuarioPorEmail(emailString, conn);
                if (usuario == null)
                    return new RetornoSucesso("O Email não está cadastrado em nossa base de dados, crie um cadastro abaixo.", Enums.CodigoErro.EmailNaoExisteNaBase);

                RetornoSucesso ret = await usuario.RecuperarSenha(conn);
                if (ret.Sucesso)
                {
                    Email email = Email.RecuperarSenha(usuario);
                    ConfiguracaoEmail configuracaoEmail = await ConfiguracaoEmail.PegarConfiguracaoEmail();
                    await Email.SendEmailAsync(email, configuracaoEmail);
                }
                return ret;
            }
        }

        [HttpPost, Route("api/usuario/reenviaremail/{email}")]
        public async Task ReenviarEmail(string email, int tipo)
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                Usuario usuario = await Usuario.BuscarUsuarioPorEmail(email, conn);
                Email emailRequest = Email.ConfirmarEmail(usuario);
                ConfiguracaoEmail configuracaoEmail = await ConfiguracaoEmail.PegarConfiguracaoEmail();
                await Email.SendEmailAsync(emailRequest, configuracaoEmail);
            }
        }
    }
}
