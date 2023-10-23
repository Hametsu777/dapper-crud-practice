using Dapper;
using DapperCrudPractice.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace DapperCrudPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperHeroController : ControllerBase
    {
        // Need the connection string for PostgreSQL database so need to pass I configuration to access it from
        // apppsettings.json.
        private readonly IConfiguration _config;
        public SuperHeroController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<SuperHero>>> GetAllSuperHeroes()
        {
            // Need to open a postgres connection here if using dapper. When using Query, need to select type<>.
            // Need to use double quotes to read from postgresql database. Without them, it doesn't recognize the database.
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<SuperHero> heroes = await SelectAllHeroes(connection);
            return Ok(heroes);
        }

        [HttpGet("{heroId}")]
        public async Task<ActionResult<SuperHero>> GetHero(int heroId)
        {
            // Need to always use connection string for every CRUD operation. To use @Id parameter, we need another object.
            // In other words, WHERE Id = @Id needs to match up with the object new { Id = heroId }. Don't forget to add
            // quotation marks around column names as well in postgresql.
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            var hero = await connection.QueryFirstAsync<SuperHero>("SELECT * FROM \"SuperHeroes\" WHERE \"Id\" = @Id",
                new { Id = heroId });
            return Ok(hero);
        }

        [HttpPost]
        public async Task<ActionResult<List<SuperHero>>> CreateHero(SuperHero hero)
        {
            // When reading data, can use Query but when needing to update, post, or delete, need to use Execute.
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("INSERT INTO \"SuperHeroes\" (\"Name\", \"FirstName\", \"LastName\", \"Place\") VALUES (@Name, @FirstName, @LastName, @Place)", hero);
            return Ok(await SelectAllHeroes(connection));
        }

        [HttpPut]
        public async Task<ActionResult<List<SuperHero>>> UpdateHero(SuperHero hero)
        {
            // When reading data, can use Query but when needing to update, post, or delete, need to use Execute.
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("UPDATE \"SuperHeroes\" SET \"Name\" = @Name, \"FirstName\" = @FirstName, \"LastName\" = @LastName, \"Place\" = @Place WHERE \"Id\" = @Id", hero);
            return Ok(await SelectAllHeroes(connection));
        }

        [HttpDelete("{heroId}")]
        public async Task<ActionResult<List<SuperHero>>> DeleteHero(int heroId)
        {
            // When reading data, can use Query but when needing to update, post, or delete, need to use Execute.
            using var connection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("DELETE FROM \"SuperHeroes\" WHERE \"Id\" = @Id", new { Id = heroId });
            return Ok(await SelectAllHeroes(connection));
        }

        private static async Task<IEnumerable<SuperHero>> SelectAllHeroes(NpgsqlConnection connection)
        {
            return await connection.QueryAsync<SuperHero>("SELECT * FROM \"SuperHeroes\"");
        }
    }
}
