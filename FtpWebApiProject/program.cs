using FtpWebApiProject.Services;
using FtpWebApiProject.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS (İzin) Ayarı - Hepsini açıyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  // Her yerden gelen isteği kabul et
                  .AllowAnyMethod()  // GET, POST hepsini kabul et
                  .AllowAnyHeader(); // Tüm başlıkları kabul et
        });
});

// Servisleri Ekle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FTP Servisini Ekle
builder.Services.Configure<FtpSettings>(builder.Configuration.GetSection("FtpSettings"));
builder.Services.AddScoped<IFtpService, FtpService>();

var app = builder.Build();

// Swagger (Her ortamda açık olsun)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 2. CORS Politikasını Devreye Al (Sırası Önemli!)
app.UseCors("AllowAll"); 

app.UseAuthorization();

app.MapControllers();

app.Run();