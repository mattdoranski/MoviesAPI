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

        // GET api/Movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetAllMovies()
        {
            var sql = @"SELECT m.*,g.*,r.*,d.*
                FROM Movie m 
                INNER JOIN Genre g ON m.GenreId = g.Id
                INNER JOIN AudienceRating r ON m.AudienceRatingId = r.Id
                INNER JOIN Director d ON m.DirectorId = d.Id";

            var movies = await _db.QueryAsync<Movie, Genre, AudienceRating, Director, Movie>( // tbl1, tbl2, return type
                sql,
                (movie, genre, audienceRating, director) =>
                {
                    movie.Genre = genre;
                    movie.AudienceRating = audienceRating;
                    movie.Director = director;
                    return movie;
                },
                splitOn: "Id" // specify what column to split on (Id is the default)
                );

            return Ok(movies);
        }

        // GET api/Movies/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovieById(int id)
        {
            var sql = @"SELECT m.*,g.*,r.*,d.*,c.*,u.*
                FROM Movie m 
                INNER JOIN Genre g ON m.GenreId = g.Id
                INNER JOIN AudienceRating r ON m.AudienceRatingId = r.Id
                INNER JOIN Director d ON m.DirectorId = d.Id
                LEFT OUTER JOIN CriticReview c ON m.Id = c.MovieId
                LEFT OUTER JOIN UserSnapshot u ON u.UserId = c.CreatedBy
                WHERE m.Id = @Id";

            // Create null Movie object
            Movie? movie = null;

            await _db.QueryAsync<Movie, Genre, AudienceRating, Director, CriticReview, User, Movie>( // tbl1, tbl2, return type
                sql,
                (m, g, r, d, c, u) =>
                {
                    if(movie == null)
                    {
                        movie = m;
                        movie.Genre = g;
                        movie.AudienceRating = r;
                        movie.Director = d;
                        movie.CriticReviews = new List<CriticReview>();
                        
                    }
                    if (r != null)
                    {
                        c.User = u;
                        movie.CriticReviews.Add(c);
                    }

                    return movie;
                },
                new {Id = id}, //Assign @Id in SQL
                splitOn: "Id,Id,UserId" // specify what column to split on (Id is the default)
                );

            return Ok(movie);
        }
    }
}
