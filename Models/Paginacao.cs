using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Models
{
    public class Paginacao
    {
        public int Pagina { get; set; }
        public int Tamanho { get; set; }

        public int Deslocamento { get; set; }
        public int Proxima { get; set; }

        public Paginacao()
        {
        }

        public Paginacao(int pagina, int tamanho = 9)
        {
            Pagina = pagina < 1 ? 1 : pagina;
            Tamanho = tamanho < 1 ? 10 : tamanho;

            Proxima = tamanho;
            Deslocamento = (Pagina - 1) * Proxima;
        }

    }
}
