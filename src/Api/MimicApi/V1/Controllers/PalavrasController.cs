using AutoMapper;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicApi.Database;
using MimicApi.Helpers;
using MimicApi.V1.Models;
using MimicApi.V1.Models.DTO;
using MimicApi.V1.Repository.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.V1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Operação que pega do banco de dados todas as palavras existentes.
        /// </summary>
        /// <param name="query">Filtros de pesquisa</param>
        /// <returns>Uma lista de palavras</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery] PalavraUrlQuery query)
        {
            var item = _repository.ObterTodas(query);

            if (item.Results.Count == 0)
                return NotFound();

            PaginacaoList<PalavraDTO> lista = CriaPagiancaoList(query, item);

            return Ok(lista);
        }

        private PaginacaoList<PalavraDTO> CriaPagiancaoList(PalavraUrlQuery query, PaginacaoList<Palavra> item)
        {
            var lista = _mapper.Map<PaginacaoList<Palavra>, PaginacaoList<PalavraDTO>>(item);
            foreach (var palavra in lista.Results)
            {
                palavra.Links = new List<LinkDTO>();
                palavra.Links.Add(
                    new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET")
                );
            }

            lista.Links.Add(
                new LinkDTO("self", Url.Link("ObterTodas", query), "GET")
            );

            if (item.Paginacao != null)
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                if (query.PaginaNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery()
                    {
                        PaginaNumero = query.PaginaNumero + 1,
                        RegistroPorPagina = query.RegistroPorPagina,
                        Data = query.Data
                    };

                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }

                if (query.PaginaNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery()
                    {
                        PaginaNumero = query.PaginaNumero - 1,
                        RegistroPorPagina = query.RegistroPorPagina,
                        Data = query.Data
                    };

                    lista.Links.Add(new LinkDTO("prev", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return lista;
        }

        /// <summary>
        /// Operação que pega uma única palavra da base de dados
        /// </summary>
        /// <param name="id">Identficador da palavra</param>
        /// <returns>Um objeto de palavra</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
                [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var palavra = _repository.Obter(id);
            if (palavra == null)
                return NotFound();

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);

            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
            );
            palavraDTO.Links.Add(
                new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }), "PUT")
            );
            palavraDTO.Links.Add(
                new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }), "DELETE")
            );

            return Ok(palavraDTO);
        }
        /// <summary>
        /// Operação que realiza o cadastro da palavra
        /// </summary>
        /// <param name="palavra">Um objeto palavra</param>
        /// <returns>uUm objeto palavra com seu Identificador</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;
            _repository.Cadastrar(palavra);

            var palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
            );

            return Created($"api/palavras/{palavra.Id}", palavra);
        }
        /// <summary>
        /// Operação que realiza a substituição de dados de uma palavra específica
        /// </summary>
        /// <param name="id">Identificador da palavra a ser alterada</param>
        /// <param name="palavra">Objeto palavra com os dados a ser alterado</param>
        /// <returns></returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();

            if (palavra == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);

            var palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
            );

            return Created($"api/palavras/{palavra.Id}", palavra);
        }

        /// <summary>
        /// Operação que desativa um palavra do sistema
        /// </summary>
        /// <param name="id">Identificador da palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.1")]
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);
            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);
            return NoContent();
        }
    }
}
