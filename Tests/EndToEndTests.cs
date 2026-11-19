using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using AutoMapper;
using FluentValidation;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace LibraryManagementSystem.Tests;

public sealed class EndToEndTests : IAsyncLifetime
{
    private readonly ApiFactory factory = new();
    private HttpClient client = null!;
    private readonly string email = Guid.NewGuid().ToString("N") + "@example.invalid";
    private readonly string password = "Aa!9" + Guid.NewGuid().ToString("N");
    public async Task InitializeAsync()
    {
        client = factory.CreateClient();
        await factory.InitializeDatabase();
    }
    public async Task DisposeAsync()
    {
        try { await factory.DropTestDatabase(); }
        finally { client?.Dispose(); await factory.DisposeAsync(); }
    }

    private async Task<JsonNode> Send(HttpMethod method, string path, object? body, HttpStatusCode status)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null) request.Content = JsonContent.Create(body);
        using var response = await client.SendAsync(request);
        // Only status and route enter test diagnostics, never credentials or response tokens.
        Assert.True(response.StatusCode == status, $"{method} {path}: expected {(int)status}, got {(int)response.StatusCode}");
        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(text) ? new JsonObject() : JsonNode.Parse(text)!;
    }
    private Task<JsonNode> Post(string path, object body, HttpStatusCode status = HttpStatusCode.OK) => Send(HttpMethod.Post, path, body, status);
    private async Task Login()
    {
        await Post("/api/auth/register", new { fullName = "Test Reader", email, password });
        var login = await Post("/api/auth/login", new { email, password });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login["token"]!.GetValue<string>());
    }
    private static object Book(int categoryId, string title = "Test Book", decimal price = 12.34m) =>
        new { title, author = "Test Author", price, quantity = 2, categoryId };
    private async Task<int> Category(string name = "Test Category")
    {
        using var response = await client.PostAsJsonAsync("/api/categories", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        var id = json["data"]!["id"]!.GetValue<int>();
        Assert.EndsWith($"/api/Categories/{id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        return id;
    }

    [Fact]
    public async Task Dependency_graph_and_mapping_preserve_entity_state()
    {
        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        foreach (var type in new[] { typeof(IAuthServices), typeof(IBookService), typeof(ICategoryService), typeof(IEmailService),
            typeof(IValidator<BookCreateDto>), typeof(IValidator<BookUpdateDto>), typeof(IValidator<CategoryCreateDto>),
            typeof(IValidator<CategoryUpdateDto>), typeof(IValidator<RegisterDTO>), typeof(IValidator<LoginDTO>),
            typeof(IValidator<ForgotPasswordDto>), typeof(IValidator<ResetPasswordDto>) })
            Assert.NotNull(sp.GetRequiredService(type));
        var uow = sp.GetRequiredService<IUnitOfWork>();
        Assert.IsType<BookRepository>(uow.Books);
        Assert.IsType<CategoryRepository>(uow.Categories);
        Assert.IsAssignableFrom<IGenericRepository<Book>>(uow.Books);
        var mapper = sp.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        var date = DateTime.UtcNow.AddYears(-1);
        var category = new Category { CategoryId = 31, Name = "Science", CreatedAt = date, IsDeleted = true };
        var book = new Book { BookId = 42, CreatedAt = date, IsDeleted = true, Category = category };
        Assert.Same(book, mapper.Map(new BookUpdateDto { Title = "  Title  ", Author = "  Author  " }, book));
        Assert.Equal((42, date, true, "Title", "Author"), (book.BookId, book.CreatedAt, book.IsDeleted, book.Title, book.Author));
        Assert.Same(category, book.Category);
        var response = mapper.Map<BookResponseDto>(book);
        Assert.Equal((42, "Science"), (response.Id, response.CategoryName));
        mapper.Map(new CategoryUpdateDto { Name = "  Category  " }, category);
        Assert.Equal((31, date, true, "Category", ""), (category.CategoryId, category.CreatedAt, category.IsDeleted, category.Name, category.Description));
        Assert.Equal(31, mapper.Map<CategoryResponseDto>(category).Id);
        var created = mapper.Map<Book>(new BookCreateDto { Title = "  Title  ", Author = " Author " });
        Assert.Equal((0, false, "Title", "Author"), (created.BookId, created.IsDeleted, created.Title, created.Author));
        Assert.InRange(created.CreatedAt, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        Assert.Equal("", mapper.Map<Category>(new CategoryCreateDto { Name = "Name" }).Description);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Validation_matrix_rejects_create_and_update_before_persistence()
    {
        await Login();
        var categoryId = await Category();
        var book = await Post("/api/book", Book(categoryId), HttpStatusCode.Created);
        var bookId = book["data"]!["id"]!.GetValue<int>();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var beforeBooks = await db.Books.CountAsync();
        var beforeCategories = await db.Categories.CountAsync();
        foreach (var name in new[] { "", " ", "A", new string('A', 101), "Bad123", "Bad!" })
        foreach (var method in new[] { HttpMethod.Post, HttpMethod.Put })
        {
            var json = await Send(method, "/api/categories" + (method == HttpMethod.Put ? $"/{categoryId}" : ""), new { name }, HttpStatusCode.BadRequest);
            Assert.NotNull(json["errors"]!["Name"]);
        }
        var cases = new List<(string field, object value)>();
        foreach (var field in new[] { "title", "author" })
            foreach (var value in new[] { "", "  ", "A", new string('A', field == "title" ? 201 : 101), "Bad123", "Bad!" }) cases.Add((field, value));
        cases.AddRange(new[] { ("price", (object)0m), ("price", -1m), ("price", 0.009m), ("price", 10000000000000000m), ("quantity", -1), ("categoryId", 0), ("categoryId", -1) });
        foreach (var (field, value) in cases)
        foreach (var method in new[] { HttpMethod.Post, HttpMethod.Put })
        {
            var dto = new Dictionary<string, object> { ["title"] = "Valid Title", ["author"] = "Valid Author", ["price"] = 1m, ["quantity"] = 0, ["categoryId"] = categoryId };
            dto[field] = value;
            var json = await Send(method, "/api/book" + (method == HttpMethod.Put ? $"/{bookId}" : ""), dto, HttpStatusCode.BadRequest);
            Assert.NotNull(json["errors"]![char.ToUpperInvariant(field[0]) + field[1..]]);
        }
        Assert.Equal(beforeBooks, await db.Books.CountAsync());
        Assert.Equal(beforeCategories, await db.Categories.CountAsync());
        Assert.Equal("Test Book", (await db.Books.SingleAsync()).Title);
        Assert.Equal("Test Category", (await db.Categories.SingleAsync()).Name);
        // Inclusive valid length/price/quantity boundaries are persisted on SQL Server.
        foreach (var length in new[] { 2, 100 }) await Category(new string('A', length));
        foreach (var price in new[] { 0.01m, 9999999999999999.99m })
            await Post("/api/book", new { title = new string('A', 200), author = new string('A', 100), price, quantity = 0, categoryId }, HttpStatusCode.Created);
        await Post("/api/book", new { title = "Ab", author = "Ab", price = 1m, quantity = 0, categoryId }, HttpStatusCode.Created);
    }

    [Fact]
    public async Task Crud_soft_delete_queries_and_deleted_category_rules()
    {
        await Login();
        var category = await Category("  Science  ");
        using var response = await client.PostAsJsonAsync("/api/book", Book(category, "  Alpha  "));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["data"]!;
        var id = created["id"]!.GetValue<int>();
        Assert.Equal("Alpha", created["title"]!.GetValue<string>());
        Assert.Equal("Science", created["categoryName"]!.GetValue<string>());
        Assert.EndsWith($"/api/Book/{id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(response.Headers.Location)).StatusCode);
        var updated = await Send(HttpMethod.Put, $"/api/book/{id}", Book(category, "Beta"), HttpStatusCode.OK);
        Assert.Equal(created["createdAt"]!.ToJsonString(), updated["data"]!["createdAt"]!.ToJsonString());
        Assert.Equal(id, updated["data"]!["id"]!.GetValue<int>());
        var updatedCategory = await Send(HttpMethod.Put, $"/api/categories/{category}", new { name = " Science Revised " }, HttpStatusCode.OK);
        Assert.Equal("Science Revised", updatedCategory["data"]!["name"]!.GetValue<string>());
        await Send(HttpMethod.Delete, $"/api/categories/{category}", null, HttpStatusCode.BadRequest);
        for (var i = 0; i < 3; i++) await Post("/api/book", Book(category, "Same Title", i + 1), HttpStatusCode.Created);
        var page = (await Send(HttpMethod.Get, $"/api/book?search=Same&categoryId={category}&sortBy=title&pageSize=2&pageNumber=1", null, HttpStatusCode.OK))["data"]!;
        Assert.Equal(3, page["totalCount"]!.GetValue<int>());
        var ids = page["items"]!.AsArray().Select(x => x!["id"]!.GetValue<int>()).ToArray();
        Assert.True(ids[0] < ids[1]);
        var second = (await Send(HttpMethod.Get, "/api/book?search=Same&pageSize=2&pageNumber=2", null, HttpStatusCode.OK))["data"]!;
        Assert.Single(second["items"]!.AsArray());
        Assert.True(second["items"]![0]!["id"]!.GetValue<int>() > ids[1]);
        foreach (var sort in new[] { "title", "price", "createdat" })
        foreach (var descending in new[] { false, true })
        {
            var rows = (await Send(HttpMethod.Get, $"/api/book?sortBy={sort}&descending={descending}", null, HttpStatusCode.OK))["data"]!["items"]!.AsArray();
            var key = sort == "createdat" ? "createdAt" : sort;
            var values = rows.Select(x => sort == "price" ? (IComparable)x![key]!.GetValue<decimal>() : x![key]!.GetValue<string>()).ToArray();
            Assert.True(values.Zip(values.Skip(1)).All(pair => descending ? pair.First.CompareTo(pair.Second) >= 0 : pair.First.CompareTo(pair.Second) <= 0));
        }
        var clamp = (await Send(HttpMethod.Get, "/api/book?pageNumber=0&pageSize=999", null, HttpStatusCode.OK))["data"]!;
        Assert.Equal(1, clamp["pageNumber"]!.GetValue<int>());
        Assert.Equal(50, clamp["pageSize"]!.GetValue<int>());
        var categories = (await Send(HttpMethod.Get, "/api/categories?search=Revised&sortBy=createdat&descending=true", null, HttpStatusCode.OK))["data"]!;
        Assert.Equal(1, categories["totalCount"]!.GetValue<int>());
        var all = (await Send(HttpMethod.Get, "/api/book", null, HttpStatusCode.OK))["data"]!["items"]!.AsArray();
        foreach (var row in all) await Send(HttpMethod.Delete, $"/api/book/{row!["id"]}", null, HttpStatusCode.OK);
        await Send(HttpMethod.Get, $"/api/book/{id}", null, HttpStatusCode.NotFound);
        await Send(HttpMethod.Put, $"/api/book/{id}", Book(category), HttpStatusCode.NotFound);
        await Send(HttpMethod.Delete, $"/api/book/{id}", null, HttpStatusCode.NotFound);
        await Send(HttpMethod.Delete, $"/api/categories/{category}", null, HttpStatusCode.OK);
        await Send(HttpMethod.Get, $"/api/categories/{category}", null, HttpStatusCode.NotFound);
        await Send(HttpMethod.Put, $"/api/categories/{category}", new { name = "Changed" }, HttpStatusCode.NotFound);
        await Send(HttpMethod.Delete, $"/api/categories/{category}", null, HttpStatusCode.NotFound);
        await Post("/api/book", Book(category), HttpStatusCode.BadRequest);
        var active = await Category("Active Category");
        var newBook = await Post("/api/book", Book(active), HttpStatusCode.Created);
        await Send(HttpMethod.Put, $"/api/book/{newBook["data"]!["id"]}", Book(category), HttpStatusCode.BadRequest);
        var remaining = (await Send(HttpMethod.Get, "/api/book", null, HttpStatusCode.OK))["data"]!;
        Assert.Equal(1, remaining["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task Jwt_and_account_authentication()
    {
        foreach (var path in new[] { "/api/book", "/api/categories" })
            await Send(HttpMethod.Get, path, null, HttpStatusCode.OK);
        var expired = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken("LibraryApiTests", "LibraryApiTests",
            expires: DateTime.UtcNow.AddMinutes(-1), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(factory.SigningKey)), SecurityAlgorithms.HmacSha256)));
        await Login();
        var valid = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var parts = valid.Split('.');
        parts[2] = (parts[2][0] == 'a' ? "b" : "a") + parts[2][1..];
        foreach (var token in new string?[] { null, "malformed", string.Join('.', parts), expired })
        {
            client.DefaultRequestHeaders.Authorization = token == null ? null : new AuthenticationHeaderValue("Bearer", token);
            foreach (var path in new[] { "/api/book", "/api/categories" })
            {
                await Send(HttpMethod.Post, path, new { }, HttpStatusCode.Unauthorized);
                await Send(HttpMethod.Put, path + "/1", new { }, HttpStatusCode.Unauthorized);
                await Send(HttpMethod.Delete, path + "/1", null, HttpStatusCode.Unauthorized);
            }
        }
        await Post("/api/auth/login", new { email, password = password + "bad" }, HttpStatusCode.Unauthorized);
        await Post("/api/auth/login", new { email = "missing@example.invalid", password }, HttpStatusCode.Unauthorized);
        await Post("/api/auth/register", new { fullName = "Test Reader", email, password }, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Password_reset_expiration_attempts_replacement_confirmation_and_reuse()
    {
        await Login();
        async Task<string> Issue()
        {
            await Post("/api/auth/forgot-password", new { email });
            var code = factory.Inbox.Codes[email];
            Assert.True(Regex.IsMatch(code, "^[0-9]{6}$"), "Reset code must be six digits.");
            return code;
        }
        var existing = await Post("/api/auth/forgot-password", new { email });
        var missing = await Post("/api/auth/forgot-password", new { email = "missing@example.invalid" });
        Assert.Equal(existing.ToJsonString(), missing.ToJsonString());
        var newPassword = "Bb!8" + Guid.NewGuid().ToString("N");
        object Reset(string code, string? confirmation = null) => new { email, code, newPassword, confirmPassword = confirmation ?? newPassword };
        var first = await Issue();
        string current;
        do { current = await Issue(); } while (current == first);
        await Post("/api/auth/reset-password", Reset(first), HttpStatusCode.BadRequest);
        await Post("/api/auth/reset-password", Reset(current, "different"), HttpStatusCode.BadRequest);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.SingleAsync();
            Assert.InRange(user.ResetCodeExpiresAt!.Value, DateTime.UtcNow.AddMinutes(9.8), DateTime.UtcNow.AddMinutes(10));
            user.ResetCodeExpiresAt = DateTime.UtcNow.AddSeconds(-1);
            await db.SaveChangesAsync();
        }
        await Post("/api/auth/reset-password", Reset(current), HttpStatusCode.BadRequest);
        current = await Issue();
        var wrong = current == "000000" ? "000001" : "000000";
        for (var i = 0; i < 5; i++) await Post("/api/auth/reset-password", Reset(wrong), HttpStatusCode.BadRequest);
        await Post("/api/auth/reset-password", Reset(current), HttpStatusCode.BadRequest);
        current = await Issue();
        await Post("/api/auth/reset-password", Reset(current));
        await Post("/api/auth/reset-password", Reset(current), HttpStatusCode.BadRequest);
        await Post("/api/auth/login", new { email, password }, HttpStatusCode.Unauthorized);
        await Post("/api/auth/login", new { email, password = newPassword });
    }
}
