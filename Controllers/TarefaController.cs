using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrilhaApiDesafio.Context;
using TrilhaApiDesafio.Models;
using System;
using System.Linq;

namespace TrilhaApiDesafio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly OrganizadorContext _context;

        public TarefaController(OrganizadorContext context)
        {
            _context = context;
        }

        // GET /tarefa/5
        [HttpGet("{id:int}")]
        public IActionResult ObterPorId([FromRoute] int id)
        {
            var tarefa = _context.Tarefas.Find(id);

            if (tarefa == null)
                return NotFound(); // 404

            return Ok(tarefa); // 200
        }

        // GET /tarefa/obtertodos
        [HttpGet("ObterTodos")]
        public IActionResult ObterTodos()
        {
            // Se a tabela for grande, considere paginação
            var tarefas = _context.Tarefas.AsNoTracking().ToList();
            return Ok(tarefas);
        }

        // GET /tarefa/obterportitulo?titulo=relatorio
        [HttpGet("ObterPorTitulo")]
        public IActionResult ObterPorTitulo([FromQuery] string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return BadRequest(new { Erro = "Informe o parâmetro 'titulo'." });

            // Busca case-insensitive
            var tarefas = _context
                .Tarefas
                .AsNoTracking()
                .Where(x => EF.Functions.Like(x.Titulo, $"%{titulo}%"))
                .ToList();

            return Ok(tarefas);
        }

        // GET /tarefa/obterpordata?data=2026-02-26
        [HttpGet("ObterPorData")]
        public IActionResult ObterPorData([FromQuery] DateTime data)
        {
            // Comparando somente a parte da Data (despreza horário)
            var tarefas = _context
                .Tarefas
                .AsNoTracking()
                .Where(x => x.Data.Date == data.Date)
                .ToList();

            return Ok(tarefas);
        }

        // GET /tarefa/obterporstatus?status=Pendente
        [HttpGet("ObterPorStatus")]
        public IActionResult ObterPorStatus([FromQuery] EnumStatusTarefa status)
        {
            var tarefas = _context
                .Tarefas
                .AsNoTracking()
                .Where(x => x.Status == status)
                .ToList();

            return Ok(tarefas);
        }

        // POST /tarefa
        // Body JSON: { "titulo": "...", "descricao": "...", "data": "2026-02-26", "status": "Pendente" }
        [HttpPost]
        public IActionResult Criar([FromBody] Tarefa tarefa)
        {
            // Validações simples
            if (tarefa == null)
                return BadRequest(new { Erro = "Objeto tarefa é obrigatório." });

            if (tarefa.Data == DateTime.MinValue)
                return BadRequest(new { Erro = "A data da tarefa não pode ser vazia." });

            if (string.IsNullOrWhiteSpace(tarefa.Titulo))
                return BadRequest(new { Erro = "O título é obrigatório." });

            // Persistência
            _context.Tarefas.Add(tarefa);
            _context.SaveChanges();

            return CreatedAtAction(nameof(ObterPorId), new { id = tarefa.Id }, tarefa);
        }

        // PUT /tarefa/5
        [HttpPut("{id:int}")]
        public IActionResult Atualizar([FromRoute] int id, [FromBody] Tarefa tarefa)
        {
            var tarefaBanco = _context.Tarefas.Find(id);

            if (tarefaBanco == null)
                return NotFound();

            if (tarefa == null)
                return BadRequest(new { Erro = "Objeto tarefa é obrigatório." });

            if (tarefa.Data == DateTime.MinValue)
                return BadRequest(new { Erro = "A data da tarefa não pode ser vazia." });

            if (string.IsNullOrWhiteSpace(tarefa.Titulo))
                return BadRequest(new { Erro = "O título é obrigatório." });

            // Atualiza campos permitidos
            tarefaBanco.Titulo = tarefa.Titulo;
            tarefaBanco.Descricao = tarefa.Descricao;
            tarefaBanco.Data = tarefa.Data;
            tarefaBanco.Status = tarefa.Status;

            _context.Tarefas.Update(tarefaBanco);
            _context.SaveChanges();

            return Ok(tarefaBanco);
        }

        // DELETE /tarefa/5
        [HttpDelete("{id:int}")]
        public IActionResult Deletar([FromRoute] int id)
        {
            var tarefaBanco = _context.Tarefas.Find(id);

            if (tarefaBanco == null)
                return NotFound();

            _context.Tarefas.Remove(tarefaBanco);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
