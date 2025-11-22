using FtpWebApiProject.Services;
using FtpWebApiProject.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİSLER EKLENİYOR
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. CORS POLİTİKASI (Burası çok önemli)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()  // Her yerden gelen isteği kabul et
                .AllowAnyMethod()  // GET, POST, PUT, DELETE
                .AllowAnyHeader(); // Tüm başlıklar
        });
});

// 3. FTP AYARLARI VE SERVİSİ
builder.Services.Configure<FtpSettings>(builder.Configuration.GetSection("FtpSettings"));
builder.Services.AddScoped<IFtpService, FtpService>();

var app = builder.Build();

// --- MIDDLEWARE SIRALAMASI (BURASI HAYATİ) ---

// Swagger (Her zaman açık olsun)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// DİKKAT: UseCors, UseAuthorization'dan ÖNCE gelmek ZORUNDA!
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();