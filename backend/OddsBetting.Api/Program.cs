using OddsBetting.Api.Models;
using OddsBetting.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<SessionStorageService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BettingService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Initialize with a default invite code for first user
var storage = app.Services.GetRequiredService<SessionStorageService>();
storage.InitializeDefaultInviteCode("FIRSTUSER");

// Auth endpoints
app.MapPost("/api/auth/register", (UserRegisterRequest request, AuthService auth) =>
{
    var user = auth.Register(request);
    if (user == null)
    {
        return Results.BadRequest(new { error = "Invalid invite code or email already exists" });
    }
    
    var response = new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Balance = user.Balance,
        InviteCode = user.InviteCode
    };
    
    return Results.Ok(response);
});

app.MapPost("/api/auth/login", (UserLoginRequest request, AuthService auth) =>
{
    var (user, sessionToken) = auth.Login(request);
    if (user == null || sessionToken == null)
    {
        return Results.Unauthorized();
    }
    
    var response = new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Balance = user.Balance,
        InviteCode = user.InviteCode
    };
    
    return Results.Ok(new { user = response, sessionToken });
});

app.MapPost("/api/auth/logout", (HttpRequest request, AuthService auth) =>
{
    var sessionToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    auth.Logout(sessionToken);
    return Results.Ok();
});

app.MapGet("/api/auth/me", (HttpRequest request, AuthService auth) =>
{
    var sessionToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = auth.GetUserFromSession(sessionToken);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }
    
    var response = new UserResponse
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Balance = user.Balance,
        InviteCode = user.InviteCode
    };
    
    return Results.Ok(response);
});

// Market endpoints
app.MapGet("/api/markets", (BettingService betting) =>
{
    return Results.Ok(betting.GetAllMarkets());
});

app.MapGet("/api/markets/{id}", (string id, BettingService betting) =>
{
    var market = betting.GetMarket(id);
    if (market == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(market);
});

app.MapPost("/api/markets", (CreateMarketRequest request, HttpRequest httpRequest, AuthService auth, BettingService betting) =>
{
    var sessionToken = httpRequest.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = auth.GetUserFromSession(sessionToken);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }
    
    var market = betting.CreateMarket(request, user.Id);
    return Results.Ok(market);
});

app.MapPost("/api/markets/{id}/resolve", (string id, ResolveMarketRequest request, HttpRequest httpRequest, AuthService auth, BettingService betting) =>
{
    var sessionToken = httpRequest.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = auth.GetUserFromSession(sessionToken);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }
    
    var success = betting.ResolveMarket(id, request.Resolution, user.Id);
    if (!success)
    {
        return Results.BadRequest(new { error = "Cannot resolve market" });
    }
    
    return Results.Ok();
});

// Bet endpoints
app.MapPost("/api/bets", (PlaceBetRequest request, HttpRequest httpRequest, AuthService auth, BettingService betting) =>
{
    var sessionToken = httpRequest.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = auth.GetUserFromSession(sessionToken);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }
    
    var bet = betting.PlaceBet(request, user.Id);
    if (bet == null)
    {
        return Results.BadRequest(new { error = "Cannot place bet" });
    }
    
    return Results.Ok(bet);
});

app.MapGet("/api/bets/my", (HttpRequest request, AuthService auth, BettingService betting) =>
{
    var sessionToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = auth.GetUserFromSession(sessionToken);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }
    
    var bets = betting.GetUserBets(user.Id);
    return Results.Ok(bets);
});

// Invite code validation
app.MapGet("/api/invite/validate/{code}", (string code, SessionStorageService storage) =>
{
    var isValid = storage.ValidateInviteCode(code);
    return Results.Ok(new { valid = isValid });
});

app.Run();
