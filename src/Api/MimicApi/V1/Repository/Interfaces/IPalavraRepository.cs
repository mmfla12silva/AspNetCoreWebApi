﻿using MimicApi.Helpers;
using MimicApi.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.V1.Repository.Interfaces
{
    public interface IPalavraRepository
    {
        PaginacaoList<Palavra> ObterTodas(PalavraUrlQuery query);
        Palavra Obter(int id);
        void Cadastrar(Palavra palavra);
        void Atualizar(Palavra palavra);
        void Deletar(int id);
    }
}
