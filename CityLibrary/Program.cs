using CityLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Connection string and table name for Azure Table Storage
string connectionString = "DefaultEndpointsProtocol=https;AccountName=azuretable;AccountKey=c4DhYo2OiWTgjGdr1ZP2rLmjWBl4qgiXpe4z+uWbEBgts6Ryv/MPHSptsGr038XSCTSnlkoxrpOZ+AStRLQTXg==;EndpointSuffix=core.windows.net";
string tableName = "ICETASK";
string blobContainerName = "BlobTask";
string queueName = "QueueTask";
string fileShareName = "yourfileshare";


// Register BorrowerService
builder.Services.AddSingleton(new BorrowerService(connectionString, tableName));
builder.Services.AddSingleton(new BlobStorageService(connectionString, blobContainerName));
builder.Services.AddSingleton(new QueueStorageService(connectionString, queueName));
builder.Services.AddSingleton(new FileService(connectionString, fileShareName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
