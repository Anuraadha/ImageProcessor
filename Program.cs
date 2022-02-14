using ImageProcessor.Services;
using Microsoft.OpenApi.Models;
using static ImageProcessor.Services.ImageProcessorService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IImageProcessorService, ImageProcessorService>();
builder.Services.AddTransient<ImageProcessorService, ImageProcessorService>();

    builder.Services.AddSwaggerGen(c =>  
      {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Image Processor API", Version = "v1" });
});
var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSwagger();
 
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Image Processor API V1");
});


app.UseHttpsRedirection();
app.UseStaticFiles();
    
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
