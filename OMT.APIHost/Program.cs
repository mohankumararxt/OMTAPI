using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OMT.Authorization;
using OMT.DataAccess.Context;
using OMT.DataService;
using OMT.DataService.Interface;
using OMT.DataService.Service;
using OMT.DataService.Settings;
using OMT.DataService.Utility;
using OMT.DTO;


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

                             var customClaimTypes = new List<string>() { "FirstName", "Email", "OrganizationId", "UserId", "RoleId" };
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

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });
string blobConnectionString = builder.Configuration.GetSection("AzureConnectionSettings").GetValue<string>("ConnectionString");

// Register BlobServiceClient
builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
builder.Services.Configure<AzureConnectionSettings>(builder.Configuration.GetSection("AzureConnectionSettings"));



builder.Services.AddControllers();

builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IUserTestService, UserTestService>();
//add the required settings from appsettings.json
builder.Services.Configure<JwtAuthSettings>(builder.Configuration.GetSection("AuthSettings"));
builder.Services.Configure<TrdStatusSettings>(builder.Configuration.GetSection("TRDconfig")); //for trd statusid
builder.Services.Configure<EmailDetailsSettings>(builder.Configuration.GetSection("EmailConfig:Common")); //for sending email
builder.Services.Configure<BasicAuthCredential>(builder.Configuration.GetSection("BasicAuthCredential"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AzureBlob>();
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
builder.Services.AddScoped<IInvoiceJointSciService, InvoiceJointSciService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailDetailsService, EmailDetailsService>();
builder.Services.AddScoped<ISciExceptionService, SciExceptionService>();
builder.Services.AddScoped<IOrderDecisionService, OrderDecisionService>();
builder.Services.AddScoped<IUpdateGOCService, UpdateGOCService>();
builder.Services.AddScoped<IReportColumnsService, ReportColumnsService>();
builder.Services.AddScoped<IShiftDetailsService, ShiftDetailsService>();
builder.Services.AddScoped<IProductivityDashboardService, ProductivityDashboardService>();
builder.Services.AddScoped<IBroadCastAnnouncementService, BroadCastAnnouncementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IMessageService, MessageService>();
 

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

//app.UseMiddleware<BasicAuthenticationMiddleware>();

app.UseAuthorization();

app.UseCors(builder => builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .WithExposedHeaders("Authorization")
                               .AllowAnyHeader());

app.MapControllers();

app.Run();

