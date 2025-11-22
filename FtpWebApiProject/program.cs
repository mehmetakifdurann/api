using FtpWebApiProject.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. CORS Servisini Ekliyoruz (GitHub Pages'ın erişebilmesi için şart)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()  // Kim gelirse gelsin kabul et (GitHub Pages vs.)
                   .AllowAnyMethod()  // GET, POST, PUT hepsine izin ver
                   .AllowAnyHeader(); // Tüm başlıklara izin ver
        });
});

// Servisleri ekle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** BİZİM EKLEDİĞİMİZ KISIM ***
// IFtpService istendiğinde FtpService ver diyoruz.
builder.Services.AddScoped<IFtpService, FtpService>(); 
// *******************************

var app = builder.Build();

// 2. Swagger Ayarı (if koşulunu kaldırdık ki Render'da da açılsın)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// 3. CORS Politikasını Devreye Al (Tam buraya yazılmalı!)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();