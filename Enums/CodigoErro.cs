using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogLivro.Enums
{
    public enum CodigoErro
    {
        SenhasIncompativeis = 1,
        EmailJaCadatrado = 2,
        ErroAoLogar = 3,
        TagJaExiste = 4,
        GatilhoJaExiste = 5,
        EmailNaoExisteNaBase = 6
    }
}
