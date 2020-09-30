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
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtener todos los libros
        /// </summary>
        /// <returns>List de autores</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location} - Intentando GetBooks");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location} - Obtuvo todos los autores exitosamente");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /// <summary>
        /// Obtener un solo libro
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Una instancia de Libro</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                _logger.LogInfo($"Intentando GetBook con el id: {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"No se encontró el libro con id: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"Obtuvo el libro exitosamente con la id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError(e);
            }

        }

        /// <summary>
        /// Crea un libro nuevo
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            try
            {
                _logger.LogInfo($"BooksController Intentando crear libro");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"BooksController La petición de crear un libro se hizo sin datos");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"BooksController La petición de crear un libro se hizo con datos incompletos");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    _logger.LogError($"BooksController La creacion de libro nuevo falló");
                    return StatusCode(500, "BooksController La creacion de libro nuevo falló");
                } 
                _logger.LogInfo($"BooksController Se creó nuevo autor correctamente");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }

        /// <summary>
        /// Edita o actualiza un libro
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            try
            {
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarn($"BooksController La petición de editar un libro se hizo sin datos o una id incorrecta");
                    return BadRequest(ModelState);
                }
                var bookExists = await _bookRepository.Exists(id);
                if (!bookExists)
                {
                    _logger.LogWarn($"BooksController El libro con id {id} no existe");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"BooksController La petición de editar un libro se hizo con datos incompletos");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    _logger.LogError($"BooksController La edicion del libro con id {id} falló");
                    return StatusCode(500, "BooksController La edicion del libro falló");
                }
                _logger.LogInfo($"BooksController Se creó nuevo libro correctamente");
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
        /// <param name="id"></param>
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
                    _logger.LogWarn($"BooksController id incorrecto para eliminar");
                    return BadRequest();
                }
                var bookExists = await _bookRepository.Exists(id);
                if (!bookExists)
                {
                    _logger.LogWarn($"BooksController El libro con id {id} no existe");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    _logger.LogError($"BooksController no se pudo eliminar el libro de id {id}");
                    return StatusCode(500, "BooksController No se pudo eliminar el libro");
                }
                _logger.LogInfo($"BooksController Se eliminó el libro");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError(e);
            }
        }


        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(Exception message)
        {
            var location = GetControllerActionNames();
            _logger.LogError($"{location} - {message.Message} - {message.InnerException}");
            return StatusCode(500, "Algo salió mal");
        }
    }

}
