using ATKInventoryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ATKInventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ItemsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Getitems()
    {
        var connectionString = _configuration
            .GetConnectionString("ATKInventoryConnection");

        var items = new List<object>();

        using var connection = new SqlConnection(connectionString);

        const string query = """
            SELECT id, Itemcode, Itemname, Category, Price, Stock
            FROM Items
            ORDER BY id
            """;

        using var command = new SqlCommand(query, connection);

        connection.Open();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            items.Add(new
            {
                id = reader.GetInt32(0),
                Itemcode = reader.GetString(1),
                Itemname = reader.GetString(2).Trim(),
                Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                Price = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                Stock = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
            });
        }

        return Ok(items);
    }


    [HttpGet("{id}")]
    public IActionResult GetItemById(int id)
    {
        var connectionString = _configuration
            .GetConnectionString("ATKInventoryConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
            SELECT id, Itemcode, Itemname, Category, Price, Stock
            FROM Items
            WHERE id = @id
            """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return Ok(new
            {
                id = reader.GetInt32(0),
                Itemcode = reader.GetString(1),
                Itemname = reader.GetString(2).Trim(),
                Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                Price = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                Stock = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
            });
        }

        return NotFound();
    }


    [HttpPost]
    public IActionResult CreateItem([FromBody] ItemsCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var connectionString = _configuration.GetConnectionString("ATKInventoryConnection");
        using var connection = new SqlConnection(connectionString);

        const string query = """
            INSERT INTO Items (Itemcode, Itemname, Category, Price, Stock)
            VALUES (@Itemcode, @Itemname, @Category, @Price, @Stock);
            SELECT SCOPE_IDENTITY();
            """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Itemcode", request.Itemcode);
        command.Parameters.AddWithValue("@Itemname", request.Itemname);
        command.Parameters.AddWithValue("@Category", (object)request.Category ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", request.Price);
        command.Parameters.AddWithValue("@Stock", request.Stock);

        try
        {
            connection.Open();
            var newId = Convert.ToInt32(command.ExecuteScalar());
            return CreatedAtAction(nameof(GetItemById), new { id = newId }, new { id = newId, message = "Item berhasil dibuat." });
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("UNIQUE KEY") || ex.Message.Contains("duplicate key"))
            {
                return Conflict(new
                {
                    message = "Kode item sudah terdaftar. Silakan gunakan kode lain."
                });
            }

            return StatusCode(500, new { message = "Terjadi kesalahan pada database." });
        }
    }



    [HttpPut("{id:int}")]
    public IActionResult UpdateItem(int id, [FromBody] ItemsCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var connectionString = _configuration.GetConnectionString("ATKInventoryConnection");
        using var connection = new SqlConnection(connectionString);

        const string query = """
        UPDATE Items
        SET Itemcode = @itemcode,
            Itemname = @itemname,
            Category = @category,
            Price = @price,
            Stock = @stock
        WHERE id = @id
        """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@itemcode", request.Itemcode);
        command.Parameters.AddWithValue("@itemname", request.Itemname);
        command.Parameters.AddWithValue("@category", (object)request.Category ?? DBNull.Value);
        command.Parameters.AddWithValue("@price", request.Price);
        command.Parameters.AddWithValue("@stock", request.Stock);

        try
        {
            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return NotFound(new
                {
                    message = $"Item dengan ID {id} tidak ditemukan."
                });
            }

            return Ok(new
            {
                message = "Data item berhasil diperbarui.",
                id
            });
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("UNIQUE KEY") || ex.Message.Contains("duplicate key"))
            {
                return Conflict(new
                {
                    message = "Kode item sudah terdaftar. Silakan gunakan kode lain."
                });
            }

            return StatusCode(500, new { message = "Terjadi kesalahan pada database." });
        }
    }


    [HttpDelete("{id:int}")]
    public IActionResult DeleteItem(int id)
    {
        var connectionString = _configuration
            .GetConnectionString("ATKInventoryConnection");

        using var connection = new SqlConnection(connectionString);

        const string query = """
        DELETE FROM Items
        WHERE id = @id
        """;

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            return NotFound(new
            {
                message = $"Item dengan ID {id} tidak ditemukan."
            });
        }

        return Ok(new
        {
            message = "Item berhasil dihapus.",
            id
        });
    }

}