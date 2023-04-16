using BlogLivro.Models.Notas;
using BlogLivro.Models.Tags;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogLivro.Models.Livros;
using BlogLivro.Models.Usuarios;

namespace BlogLivro.Models.Geral
{
    public class Geral
    {
        public List<Livro> Livros { get; set; }
        public List<Usuario> Usuarios { get; set; }

        #region Buscas
        public static async Task<Geral> BuscarGeral(Filtro filtro, SqlConnection conn)
        {
            Geral geral = new Geral();

            geral.Livros = await Livro.Buscar(new Models.Livros.Filtro { Buscar = filtro.Buscar, Paginacao = filtro.PaginacaoLivro }, conn);
            geral.Usuarios = await Usuario.BuscarUsuarios(new Models.Usuarios.Filtro { Buscar = filtro.Buscar, Paginacao = filtro.PaginacaoUsuario }, conn);

            return geral;
        }
        #endregion
    }

    public class Filtro
    {
        public string Buscar { get; set; }

        public Paginacao PaginacaoLivro { get; set; }
        public Paginacao PaginacaoUsuario { get; set; }
    }
}
