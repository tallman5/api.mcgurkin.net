using McGurkin.Api.Features.Iam;
using McGurkin.Api.Features.Tmdb;
using McGurkin.Api.Features.Tmdb.Data;
using McGurkin.Api.Features.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McGurkin.Api.Test
{
    [TestClass]
    public sealed class TmdbIntegrationTest
    {
        private readonly TmdbService _service;
        private readonly ILogger<TmdbService> _logger;

        public TmdbIntegrationTest()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });
                builder.SetMinimumLevel(LogLevel.Information);
            });
            _logger = loggerFactory.CreateLogger<TmdbService>();

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<IamService>()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            _service = new TmdbService(configuration, httpClientFactory, _logger);
        }

        [TestMethod]
        public async Task Tmdb_Discovers_Movies()
        {
            // Arrange
            var correlationId = Guid.NewGuid();

            var language = "en-US";
            var discoverRequest = new DiscoverRequest
            {
                GenreId = 28,
                Start = 0,
            };

            // Act
            var result = await _service.DiscoverMoviesAsync(discoverRequest, correlationId, language);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public async Task Tmdb_Get_Movie()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";
            var movieId = 1045938;

            // Act
            var result = await _service.GetMovieAsync(movieId, correlationId, language, true);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Tmdb_Get_Movie_Providers()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var movieId = 1045938;
            var language = "en-US";
            var region = HttpClientUtils.ExtractLocaleFromLanguageTag(language);

            // Act
            var result = await _service.GetMovieProvidersAsync(movieId, correlationId, language, region);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task Tmdb_Returns_Genres()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";

            // Act
            var result = await _service.GetGenresAsync(correlationId, language);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);

            // Debug to ensure cache was used
            result = await _service.GetGenresAsync(correlationId, language);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public async Task Tmdb_Returns_Providers()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";

            // Act
            var result = await _service.GetProvidersAsync(correlationId, language);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);

            // Debug to ensure cache was used
            result = await _service.GetProvidersAsync(correlationId, language);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public async Task Tmdb_Returns_Regions()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";

            // Act
            var result = await _service.GetRegionsAsync(correlationId, language);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);

            // Debug to ensure cache was used
            result = await _service.GetRegionsAsync(correlationId, language);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public async Task Tmdb_Get_Person()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";
            var personId = 234352;

            // Act
            var result = await _service.GetPersonAsync(personId, correlationId, language, false);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Tmdb_Get_Person_Details()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";
            var personId = 234352;

            // Act
            var result = await _service.GetPersonAsync(personId, correlationId, language, true);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Tmdb_Get_Random_Person()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";

            // Act
            var result = await _service.GetRandomPersonAsync(correlationId, language);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Tmdb_Search_Multi()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var language = "en-US";
            var query = "term";

            // Act
            var result = await _service.SearchMultiAsync(query, correlationId, language);

            // Assert
            Assert.IsTrue(result.Movies.Length > 0);
            Assert.IsTrue(result.People.Length > 0);
        }
    }
}
