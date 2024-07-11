using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OMT.Authorization;
using OMT.DataAccess.Context;
using OMT.DataService.Interface;
using OMT.DataService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = builder.Configuration.GetSection("AuthSettings").GetValue<string>("Issuer"),
                         ValidAudience = builder.Configuration.GetSection("AuthSettings").GetValue<string>("Audience"),
                         IssuerSigningKey = JwtSecurityKey.Create(builder.Configuration.GetSection("AuthSettings").GetValue<string>("SecretKey"))
                     };

                     options.Events = new JwtBearerEvents
                     {
                         OnAuthenticationFailed = context =>
                         {
                             Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                             return Task.CompletedTask;
                         },
                         OnTokenValidated = context =>
                         {
                             Console.WriteLine("OnTokenValidated: " + context.SecurityToken);

                             var customClaimTypes = new List<string>() { "FirstName","Email", "OrganizationId", "UserId","RoleId" };
                             var userClaims = context.Principal.Claims.Where(_ => customClaimTypes.Contains(_.Type)).ToList();

                             IOptions<JwtAuthSettings> authSettings = Options.Create<JwtAuthSettings>(new JwtAuthSettings()
                             {
                                 Audience = builder.Configuration.GetSection("AuthSettings").GetValue<string>("Audience"),
                                 Issuer = builder.Configuration.GetSection("AuthSettings").GetValue<string>("Issuer"),
                                 ExpiryInMinutes = builder.Configuration.GetSection("AuthSettings").GetValue<int>("ExpiryInMinutes"),
                                 SecretKey = builder.Configuration.GetSection("AuthSettings").GetValue<string>("SecretKey")
                             });
                             var token = new JwtTokenBuilder(authSettings).AddClaims(userClaims).Build();

                             context.Response.Headers.Add("Authorization", "Bearer " + token.Value);

                             return Task.CompletedTask;
                         }
                     };
                 });

builder.Services.AddControllers();
builder.Services.Configure<JwtAuthSettings>(builder.Configuration.GetSection("AuthSettings"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeamsService, TeamsService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ITeamAssociationService, TeamAssociationService>();
builder.Services.AddScoped<IRolesService, RolesService>();

builder.Services.AddScoped<ISkillSetService, SkillSetService>();
builder.Services.AddScoped<IUserSkillSetService, UserSkillSetService>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<IProcessStatusService, ProcessStatusService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IProductDescriptionService, ProductDescriptionService>();
builder.Services.AddScoped<IResWareProductDescriptionsService, ResWareProductDescriptionsService>();
builder.Services.AddScoped<IBusinessGroupService, BusinessGroupService>();
builder.Services.AddScoped<IProcessTypeService, ProcessTypeService>();
builder.Services.AddScoped<ITotalOrderFeesService, TotalOrderFeesService>();
builder.Services.AddScoped<ICostCenterService, CostCenterService>();
builder.Services.AddScoped<ISourceTypeService, SourceTypeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IInvoiceJointReswareService, InvoiceJointReswareService>();

builder.Services.AddDbContext<OMTDataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
//app.UseSwaggerUI();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OMT API V1");
    c.RoutePrefix = "";
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(builder => builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .WithExposedHeaders("Authorization")
                               .AllowAnyHeader());

app.MapControllers();

app.Run();
