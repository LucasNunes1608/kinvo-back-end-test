﻿using Aliquota.API.Helper;
using Aliquota.Domain.AggregatesModel.AplicacaoAggregate;
using Aliquota.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aliquota.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AplicacaosController : ControllerBase
    {
        private readonly AliquotaContext _context;

        public AplicacaosController(AliquotaContext context)
        {
            _context = context;
        }

        // Put: api/Aplicacao/RealizarResgate/5
        [HttpPut("RealizarResgate/{aplicacaoId}")]
        public async Task<ActionResult<string>> RealizarResgate(int aplicacaoId)
        {
            var aplicacao = await _context.aplicacoes.FindAsync(aplicacaoId);

            if (aplicacao == null)
                return NotFound();

            var produtoFinanceiro = await _context.produtoFinanceiros.FindAsync(aplicacao.produtoFinanceiroId);

            if (produtoFinanceiro == null)
                return NotFound();

            var dataResgate = TimeProvider.Current.UtcNow;
            var valorResgatado = new AplicacaoDomain(aplicacao).RealizarResgate(produtoFinanceiro.taxaDeRendimento, dataResgate);

            await _context.SaveChangesAsync();

            return String.Format("Operação Finalizada com sucesso! Foi resgatado R${0}!", valorResgatado);
        }

        // GET: api/Aplicacaos/CalculaValorDeResgate/5
        [HttpGet("CalculaValorDeResgate/{aplicacaoId}")]
        public async Task<ActionResult<double>> CalculaValorDeResgate(int aplicacaoId)
        {
            var aplicacao = await _context.aplicacoes.FindAsync(aplicacaoId);
            if (aplicacao == null)
            {
                return NotFound();
            }

            var produtoFinanceiro = await _context.produtoFinanceiros.FindAsync(aplicacao.produtoFinanceiroId);
            if (produtoFinanceiro == null)
            {
                return NotFound();
            }


            return new AplicacaoDomain(aplicacao).CalcularValorAResgatar(produtoFinanceiro.taxaDeRendimento, DateTime.UtcNow);
        }

        // GET: api/Aplicacaos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aplicacao>>> Getaplicacoes()
        {
            return await _context.aplicacoes.ToListAsync();
        }

        // GET: api/Aplicacaos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Aplicacao>> GetAplicacao(int id)
        {
            var aplicacao = await _context.aplicacoes.FindAsync(id);

            if (aplicacao == null)
            {
                return NotFound();
            }

            return aplicacao;
        }

        // PUT: api/Aplicacaos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAplicacao(int id, Aplicacao aplicacao)
        {
            if (id != aplicacao.Id)
            {
                return BadRequest();
            }

            _context.Entry(aplicacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AplicacaoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Aplicacaos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{valorInicial}&{produtoFinanceiroId}&{usuarioId}")]
        public async Task<ActionResult<Aplicacao>> PostAplicacao(double valorInicial, int produtoFinanceiroId, int usuarioId)
        {
            Aplicacao aplicacao = new Aplicacao(valorInicial, produtoFinanceiroId, usuarioId);
            _context.aplicacoes.Add(aplicacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAplicacao", new { id = aplicacao.Id }, aplicacao);
        }

        // DELETE: api/Aplicacaos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAplicacao(int id)
        {
            var aplicacao = await _context.aplicacoes.FindAsync(id);
            if (aplicacao == null)
            {
                return NotFound();
            }

            _context.aplicacoes.Remove(aplicacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AplicacaoExists(int id)
        {
            return _context.aplicacoes.Any(e => e.Id == id);
        }
    }
}
