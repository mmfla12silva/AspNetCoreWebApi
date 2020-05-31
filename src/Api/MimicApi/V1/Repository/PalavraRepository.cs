using Microsoft.EntityFrameworkCore;
using MimicApi.Database;
using MimicApi.Helpers;
using MimicApi.V1.Models;
using MimicApi.V1.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.V1.Repository
{
    public class PalavraRepository : IPalavraRepository
    {
        private readonly MimicContext _ctx;

        public PalavraRepository(MimicContext ctx)
        {
            _ctx = ctx;
        }

        public PaginacaoList<Palavra> ObterTodas(PalavraUrlQuery query)
        {

            var lista = new PaginacaoList<Palavra>();
            var palavras = _ctx.Palavras.AsNoTracking().AsQueryable();

            if (query.Data.HasValue)
                palavras = palavras.Where(item => item.Criado > query.Data || item.Atualizado > query.Data);

            if (query.PaginaNumero.HasValue)
            {
                var qtdRegistros = palavras.Count();
                palavras = palavras.Skip((query.PaginaNumero.Value - 1) * query.RegistroPorPagina.Value).Take(query.RegistroPorPagina.Value);
                
                var paginacao = new Paginacao();
                paginacao.NumeroPagina = query.PaginaNumero.Value;
                paginacao.RegistroPorPagina = query.RegistroPorPagina.Value;
                paginacao.TotalPaginas = qtdRegistros;
                paginacao.TotalRegistros = (int)Math.Ceiling((double)qtdRegistros / query.RegistroPorPagina.Value);

                lista.Paginacao = paginacao;
            }

            lista.Results.AddRange(palavras);
            //lista.AddRange(palavras);

            return lista;
        }
        public Palavra Obter(int id)
        {
            return _ctx.Palavras.AsNoTracking().AsQueryable().FirstOrDefault(item => item.Id == id);
        }
        public void Cadastrar(Palavra palavra)
        {
            _ctx.Palavras.Add(palavra);
            _ctx.SaveChanges();
        }
        public void Atualizar(Palavra palavra)
        {
            _ctx.Palavras.Update(palavra);
            _ctx.SaveChanges();
        }
        public void Deletar(int id)
        {
            var palavra = Obter(id);
            palavra.Ativo = false;
            _ctx.Palavras.Update(palavra);
            _ctx.SaveChanges();
        }



    }
}
