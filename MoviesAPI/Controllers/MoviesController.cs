using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.DTOs;
using System.Data;
using System.Reflection;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IDbConnection _db;

        // Constructor
        public MoviesController(IDbConnection db) 
        {
            _db = db;
        }

        // Get api/Movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAllMovies()
        {
            var sql = "SELECT * FROM Movie";

            var movies = await _db.QueryAsync<Movie>(sql);

            return Ok(movies);
        }
    }
}
