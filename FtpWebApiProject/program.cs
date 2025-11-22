using FtpWebApiProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** BİZİM EKLEDİĞİMİZ KISIM ***
// IFtpService istendiğinde FtpService ver diyoruz.
builder.Services.AddScoped<IFtpService, FtpService>(); 
// *******************************

var app = builder.Build();

// ... kodun geri kalanı (Swagger, HttpsRedirection vs.)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();