/appsettings.jsong
    "JwtSettings": {
        "SigningKey": "",
        "Issuer": "BimshireStore.Services.AuthAPI",
        "Audiences": [""]
    }

/Models
	/JwtSettings.cs
		namespace BimshireStore.Services.AuthAPI.Options {
			public class JwtSettings
			{
				public string? SigningKey {get; set;}
				public string? Issuer {get; set;}
				public string[]? Audiences {get; set;}
			}
		}
		
/Services
	/IdentityService.cs
		namespace BimshireStore.Services.AuthAPI.Services {
			public class IdentityService
			{
				private readonly JwtSettings? _settings;
				private readonly byte[] _key;
				
				public IdentityService(IOptions<JwtSettings> jwtOptions) {
					_settings = jwtOptions.Value;
					ArgumentNullException.ThrowIfNull(_settings);
					ArgumentNullException.ThrowIfNull(_settings.SigningKey);
					ArgumentNullException.ThrowIfNull(_settings.Audiences);
					ArgumentNullException.ThrowIfNull(_settings.Audiences[0]);
					ArgumentNullException.ThrowIfNull(_settings.Issuer);																				
					_key = Encoding.ASCII.GetBytes(_settings?.SigningKey!);
				}
				
				private static JwtSecurityTokenHandler TokenHandler => new();
				
				public SecurityToken CreateSecurityToken(ClaimsIdentity identity) {
				
					var tokenDescriptor = GetTokenDescriptor(identity);
					
					return TokenHandler.CreateToken(tokenDescriptor);
				}
				
				public string WriteToken(SecurityToken token) {
					return TokenHandler.WriteToken(token);
				}
				
				private SecurityTokenDescriptor GetTokenDescriptor(ClaimsIdentity identity) {
					return new SecurityTokenDescriptor() {
						Subject = identity,
						Expires = DateTime.Now.AddHours(2),
						Audience = _settings!.Audiences?[0]!;
						Issuer = _settings.Issuer,
						SigningCredentials = new SigningCredentials(
							new SymmetricSecurityKey(_key),
							SecurityAlgorithms.HmacSha256Signature
						)
					};
				}
			}
		}
		
		
/Program.cs

    var jwtSettings = new JwtSettings();
    builder.Configuration.Bind(nameof(JwtSettings), jwtSettings);

    var jwtSection = builder.Configuration.GetSecton(nameof(JwtSettings));
    builder.Services.Configure<JwtSettings>(jwtSection);

    builder.Services
        .AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(jwt =>
        {
            jwt.SaveToken = true;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SigningKey ?? throw new InvalidOperationException())),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudiences = jwtSettings.Audiences,
                RequireExpirationTime = false,
                ValidateLifetime = true		
            };
            jwt.Audience = jwtSettings.Audiences?[0];
            jwt.ClaimsIssuer = jwtSettings.Issuer;
        });
        
    builder.Services.AddIdentityCore<IdentityUser>(x=>
    {
        x.Password.RequireDigit = false;
        x.Password.RequiredLength = 5;
        x.RequiredLowercase = false;
        x.Password.REquiredUppercase = false;
        x.Password.ReqireNonAlphanumeric=false;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddSwaggerGen(x =>
    {
        x.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthAPI", Version ="v1"});
        x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        x.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpentApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id= "Bearer"
                    }
                },
                new string[]{}
            }
        });
    }

	
REGISTRATION AND LOGIN WORKFLOW
===    
1. Create Identity User
var identity = new ApplicationUser {Email = "email_goes_here", UserName = "email_or_uid_goes_here" };
var createdIdentity = await _userManager.CreatAsync(identity, "password_goes_here");
	
2. Create Claims
   var theCaims = new List<Claim>();
   theClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
   theClaims.Add(new Claim("Firstname", "Mary"));
   
3. Create ClaimsIdentity to be used for generating JWT
   var ClaimsIdentity = new ClaimsIdentity( new Claim[]
   {
   	new (JwtTegisteredClaimNames.Sub, identity.Email ?? thwo new InvalidOperationException());
   	new (JwtTegisteredClaimNames.Email, identity.Email ?? thwo new InvalidOperationException());
   });
   // add additional claims createad above too
   claimsIdentity.AddClaims(theCaims);
   
4. Create Token
   var token = _identityService.CreateSEcurityToken(claimsIdentity);
   var tokenString = _identityService.WriteToken(token);
   // return token usually in a cookie if it is a refresh token or in json if a access token
   
5. For login 
   //we can check that user exist
   var user = await _userManager.FindByEmailAsync("email");
   
   //we can authenticate
   var result = await _signInManager.CheckPasswordSignInAsync(user, "password", false);
   
   // get roles
   var roles = await _userManager.GetRolesAsync(user);
   
   // Create claims as above
   var theCaims = new List<Claim>();
   theClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
   theClaims.Add(new Claim("Firstname", "Mary"));  
   
   // create claims identity 
    var claimsIdentity = new ClaimsIdentity( new Claim[]
   {
   	new (JwtRegisteredClaimNames.Sub, identity.Email ?? throw new InvalidOperationException()),
   	new (JwtRegisteredClaimNames.Email, identity.Email ?? throw new InvalidOperationException()),
   });
   // add additional claims createad above too
   claimsIdentity.AddClaims(theCaims);
   
   // create token
   var token = _identityService.CreateSecurityToken(claimsIdentity);
   var tokenString = _identityService.WriteToken(token);
   // return token(s) howerver you like   
	