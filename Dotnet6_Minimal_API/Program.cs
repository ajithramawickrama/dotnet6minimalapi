
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetSection("ConnectionStrings")["SQLConnectionString"].ToString();
builder.Services.AddDbContext<EmployeeDbContext>(options => options.UseSqlServer(connectionString));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Employee API", Version = "v1" });
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudiance"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
    };
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseAuthorization();
app.UseAuthentication();



app.MapPost("/Login", [AllowAnonymous] ([FromBody] LoginVM loginVm) =>
 {
     AuthenticationHelper authenticationHelper = new AuthenticationHelper(builder.Configuration);
     var token = authenticationHelper.Login(loginVm);
     if (!string.IsNullOrEmpty(token))
         return token;
     else
         return "Unauthorized";

 });

app.MapGet("/Employees",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async ([FromServices] EmployeeDbContext dbContext) =>
{
    var employees = await dbContext.Employees.ToListAsync();
    return employees;
});

app.MapGet("/Employees/{id}",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async ([FromServices] EmployeeDbContext dbContext, int id) =>
{
    var employee = await dbContext.Employees.Where(t => t.Id == id).FirstOrDefaultAsync();
    return employee;
});

app.MapPost("/Employees",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async ([FromServices] EmployeeDbContext dbContext, Employee employee) =>
{
    dbContext.Employees.Add(employee);
    await dbContext.SaveChangesAsync();
    return employee;
});

app.MapPut("/Employees",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async ([FromServices] EmployeeDbContext dbContext, Employee employee) =>
{
    dbContext.Entry(employee).State = EntityState.Modified;
    await dbContext.SaveChangesAsync();
    return employee;
});



await app.RunAsync();
