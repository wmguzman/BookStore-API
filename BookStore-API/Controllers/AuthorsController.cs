using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using BookStore_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint usado para interactuar con la tabla Authors de la base de datos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorReporitory;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;
        public AuthorsController(IAuthorRepository authorReporitory,
            ILoggerService logger,
            IMapper mapper)
        {
            _authorReporitory = authorReporitory;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtener todos los autores
        /// </summary>
        /// <returns>List de autores</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Intentando GetAuthors");
                var authors = await _authorReporitory.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Obtuvo todos los autores exitosamente");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /// <summary>
        /// Obtener un solo autor
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Una instancia de Author</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Intentando GetAuthor con el id: {id}");
                var author = await _authorReporitory.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"No se encontró el autor con id: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Obtuvo el autor exitosamente con la id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        /// <summary>
        /// Crea un autor nuevo
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"AuthorsController Intentando crear autor");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"AuthorsController La petición de crear un autor se hizo sin datos");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"AuthorsController La petición de crear un autor se hizo con datos incompletos");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorReporitory.Create(author);
                if (!isSuccess)
                {
                    _logger.LogError($"AuthorsController La creacion de autor nuevo falló");
                    return StatusCode(500, "AuthorsController La creacion de autor nuevo falló");
                }
                _logger.LogInfo($"AuthorsController Se creó nuevo autor correctamente");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /// <summary>
        /// Edita o actualiza un autor
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                if(id<1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"AuthorsController La petición de editar un autor se hizo sin datos o una id incorrecta");
                    return BadRequest(ModelState);
                }
                var authorExists = await _authorReporitory.Exists(id);
                if (!authorExists)
                {
                    _logger.LogWarn($"AuthorsController El autor con id {id} no existe");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"AuthorsController La petición de editar un autor se hizo con datos incompletos");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorReporitory.Update(author);
                if (!isSuccess)
                {
                    _logger.LogError($"AuthorsController La edicion del autor cin id {id} falló");
                    return StatusCode(500, "AuthorsController La edicion del autor falló");
                }
                _logger.LogInfo($"AuthorsController Se creó nuevo autor correctamente");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }


        /// <summary>
        /// Edita o actualiza un autor
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                {
                    _logger.LogWarn($"AuthorsController id incorrecto para eliminar");
                    return BadRequest();
                }
                var authorExists = await _authorReporitory.Exists(id);
                if (!authorExists)
                {
                    _logger.LogWarn($"AuthorsController El autor con id {id} no existe");
                    return NotFound();
                }
                var author = await _authorReporitory.FindById(id);
                var isSuccess = await _authorReporitory.Delete(author);
                if (!isSuccess)
                {
                    _logger.LogError($"AuthorsController no se pudo eliminar el autor de id {id}");
                    return StatusCode(500, "AuthorsController No se pudo eliminar el autor");
                }
                _logger.LogInfo($"AuthorsController Se eliminó el autor");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        private ObjectResult InternalError(Exception message)
        {
            _logger.LogError($"{message.Message} - {message.InnerException}");
            return StatusCode(500, "Algo salió mal");
        }
    }
}
