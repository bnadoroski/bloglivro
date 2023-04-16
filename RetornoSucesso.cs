using BlogLivro.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro
{
    public class RetornoSucesso
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public Object Objeto { get; set; }
        public string ID { get; set; }
        public CodigoErro CodigoErro { get; set; }

        public RetornoSucesso()
        {

        }

        public RetornoSucesso(string mensagem)
        {
            Mensagem = mensagem;
            Sucesso = false;
        }

        public RetornoSucesso(string mensagem, CodigoErro codigo)
        {
            Mensagem = mensagem;
            Sucesso = false;
            CodigoErro = codigo;
        }
    }
}
