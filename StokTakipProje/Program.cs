using StokTakip.Data.Context;
using StokTakip.Data.Repository;
using StokTakip.Service.Interface;
using StokTakip.Service.Service;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    string[] permissionCodes = {
    "DASHBOARD", "BIRIMLER", "PARA_BIRIMLERI", "URUN_TIPLERI", "STOK_TIPLERI",
    "FIRMALAR", "URUNLER", "PROJELER", "MAKINALAR", "PROJE_MAKINA_ATAMA",
    "STOK_GIRISI", "STOK_CIKISI", "STOK_BAKIYE", "PROJE_MALIYET",
    "MAKINA_RAPORLARI", "KULLANICI_YONETIMI", "ROL_YONETIMI"
};
    foreach (var code in permissionCodes)
    {
        options.AddPolicy($"Permission:{code}", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "permission" && c.Value == code)));
    }
});

builder.Services.AddScoped<IDapperContext, DapperContext>();

// Repository
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
builder.Services.AddScoped<IStockTypeRepository, StockTypeRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IMachineRepository, MachineRepository>();
builder.Services.AddScoped<IProjectMachineRepository, ProjectMachineRepository>();
builder.Services.AddScoped<IStockEntryRepository, StockEntryRepository>();
builder.Services.AddScoped<IStockIssueRepository, StockIssueRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMachineReportRepository, MachineReportRepository>();
builder.Services.AddScoped<IMachineReportService, MachineReportService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Service
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddScoped<IStockTypeService, StockTypeService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IProjectMachineService, ProjectMachineService>();
builder.Services.AddScoped<IStockEntryService, StockEntryService>();
builder.Services.AddScoped<IStockIssueService, StockIssueService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/ShowStatusCode/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
