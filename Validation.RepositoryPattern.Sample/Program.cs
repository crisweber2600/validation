using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure.Setup;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;
using Validation.RepositoryPattern.Sample.Repositories;
using Validation.RepositoryPattern.Sample.Repositories.Implementations;
using Validation.RepositoryPattern.Sample.Services;

namespace Validation.RepositoryPattern.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Repository Pattern Integration with Unified Validation System");
        Console.WriteLine("===========================================================");

        // Create a host with the repository pattern and validation system
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure Entity Framework with in-memory database
                services.AddDbContext<SampleDbContext>(options =>
                    options.UseInMemoryDatabase("RepositoryPatternSample"));

                // Configure the unified validation system with validation rules for our entities
                services.AddSetupValidation()
                    .UseEntityFramework<SampleDbContext>()
                    
                    // Configure validation flows for Product
                    .AddValidationFlow<Product>(flow => flow
                        .EnableSaveValidation()
                        .EnableDeleteValidation()
                        .EnableSoftDelete()
                        .WithThreshold(x => x.Price, ThresholdType.GreaterThan, 0)
                        .WithValidationTimeout(TimeSpan.FromMinutes(1))
                        .EnableAuditing())
                    
                    // Configure validation flows for Customer
                    .AddValidationFlow<Customer>(flow => flow
                        .EnableSaveValidation()
                        .EnableDeleteValidation()
                        .WithThreshold(x => x.CreditLimit, ThresholdType.GreaterThan, -1)
                        .WithValidationTimeout(TimeSpan.FromMinutes(1))
                        .EnableAuditing())
                    
                    // Product validation rules
                    .AddRule<Product>("PositivePrice", product => product.Price > 0)
                    .AddRule<Product>("NonNegativeQuantity", product => product.Quantity >= 0)
                    .AddRule<Product>("RequiredName", product => !string.IsNullOrWhiteSpace(product.Name))
                    .AddRule<Product>("RequiredCategory", product => !string.IsNullOrWhiteSpace(product.Category))
                    .AddRule<Product>("ReasonablePrice", product => product.Price <= 100000)
                    .AddRule<Product>("ReasonableQuantity", product => product.Quantity <= 10000)
                    
                    // Customer validation rules
                    .AddRule<Customer>("RequiredFirstName", customer => !string.IsNullOrWhiteSpace(customer.FirstName))
                    .AddRule<Customer>("RequiredLastName", customer => !string.IsNullOrWhiteSpace(customer.LastName))
                    .AddRule<Customer>("ValidEmail", customer => !string.IsNullOrWhiteSpace(customer.Email) && customer.Email.Contains('@'))
                    .AddRule<Customer>("NonNegativeCreditLimit", customer => customer.CreditLimit >= 0)
                    .AddRule<Customer>("NonNegativeBalance", customer => customer.CurrentBalance >= 0)
                    .AddRule<Customer>("BalanceWithinCreditLimit", customer => customer.CurrentBalance <= customer.CreditLimit)
                    .AddRule<Customer>("ReasonableAge", customer => customer.Age >= 18 && customer.Age <= 120)
                    
                    .ConfigureMetrics(metrics => metrics
                        .WithProcessingInterval(TimeSpan.FromSeconds(30))
                        .EnableDetailedMetrics(false))
                    
                    .ConfigureReliability(reliability => reliability
                        .WithMaxRetries(2)
                        .WithRetryDelay(TimeSpan.FromMilliseconds(500)))
                    
                    .Build();

                // Register repository pattern components
                RegisterRepositoryPattern(services);
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        // Initialize the database and run demonstrations
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting repository pattern with validation system demonstration...");

        // Run demonstrations
        await DemonstrateBasicRepositoryPattern(host.Services, logger);
        await DemonstrateValidatedRepositoryPattern(host.Services, logger);
        await DemonstrateBusinessServices(host.Services, logger);
        await DemonstrateValidationFailures(host.Services, logger);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void RegisterRepositoryPattern(IServiceCollection services)
    {
        // Register basic repositories (without validation)
        services.AddScoped<Repository<Product>>();
        services.AddScoped<Repository<Customer>>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<CustomerRepository>();

        // Register validated repositories (with validation integration)
        services.AddScoped<ValidatedRepository<Product>>();
        services.AddScoped<ValidatedRepository<Customer>>();
        services.AddScoped<ValidatedProductRepository>();
        services.AddScoped<ValidatedCustomerRepository>();

        // Register repositories using interfaces - choose validated versions for demonstration
        services.AddScoped<IProductRepository, ValidatedProductRepository>();
        services.AddScoped<ICustomerRepository, ValidatedCustomerRepository>();

        // Register business services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
    }

    private static async Task DemonstrateBasicRepositoryPattern(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n1. Basic Repository Pattern (Without Validation)");
        Console.WriteLine("------------------------------------------------");

        using var scope = services.CreateScope();
        var basicProductRepo = scope.ServiceProvider.GetRequiredService<ProductRepository>();

        logger.LogInformation("Testing basic repository operations...");

        // Add a product using basic repository (bypasses validation)
        var basicProduct = new Product
        {
            Name = "Basic Widget",
            Price = 19.99m,
            Quantity = 100,
            Category = "Widgets",
            Description = "A basic widget for demonstration"
        };

        try
        {
            await basicProductRepo.AddAsync(basicProduct);
            await basicProductRepo.SaveChangesAsync();
            
            logger.LogInformation("✓ Basic repository: Product added successfully (ID: {ProductId})", basicProduct.Id);
            
            // Retrieve and display
            var retrievedProduct = await basicProductRepo.GetByIdAsync(basicProduct.Id);
            logger.LogInformation("✓ Basic repository: Product retrieved - {ProductName}, Price: {Price:C}", 
                retrievedProduct?.Name, retrievedProduct?.Price);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Basic repository operation failed");
        }
    }

    private static async Task DemonstrateValidatedRepositoryPattern(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n2. Validated Repository Pattern (With Validation)");
        Console.WriteLine("-------------------------------------------------");

        using var scope = services.CreateScope();
        var validatedProductRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        logger.LogInformation("Testing validated repository operations...");

        // Add a valid product
        var validProduct = new Product
        {
            Name = "Validated Widget",
            Price = 29.99m,
            Quantity = 50,
            Category = "Premium Widgets",
            Description = "A validated widget with proper validation"
        };

        try
        {
            await validatedProductRepo.AddAsync(validProduct);
            await validatedProductRepo.SaveChangesAsync();
            
            logger.LogInformation("✓ Validated repository: Valid product added successfully (ID: {ProductId})", validProduct.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Validated repository operation failed");
        }

        // Try to add an invalid product (should fail validation)
        var invalidProduct = new Product
        {
            Name = "", // Invalid: empty name
            Price = -10m, // Invalid: negative price
            Quantity = -5, // Invalid: negative quantity
            Category = "",
            Description = "This should fail validation"
        };

        try
        {
            await validatedProductRepo.AddAsync(invalidProduct);
            await validatedProductRepo.SaveChangesAsync();
            
            logger.LogWarning("⚠ Validated repository: Invalid product was unexpectedly added!");
        }
        catch (Repositories.Implementations.ValidationException ex)
        {
            logger.LogInformation("✓ Validated repository: Correctly rejected invalid product. Failed rules: {FailedRules}", 
                string.Join(", ", ex.FailedRules));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Unexpected error in validated repository");
        }
    }

    private static async Task DemonstrateBusinessServices(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n3. Business Services with Repository Pattern");
        Console.WriteLine("--------------------------------------------");

        using var scope = services.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        logger.LogInformation("Testing business services with validation...");

        // Test Product Service
        try
        {
            var product = new Product
            {
                Name = "Business Logic Widget",
                Price = 45.99m,
                Quantity = 75,
                Category = "Business",
                Description = "Demonstrates business service operations"
            };

            var createdProduct = await productService.CreateProductAsync(product);
            logger.LogInformation("✓ Product service: Created product '{ProductName}' with ID {ProductId}", 
                createdProduct.Name, createdProduct.Id);

            // Update price using business logic
            var updatedProduct = await productService.UpdateProductPriceAsync(createdProduct.Id, 39.99m);
            logger.LogInformation("✓ Product service: Updated product price to {NewPrice:C}", updatedProduct.Price);

            // Check inventory value
            var inventoryValue = await productService.GetInventoryValueAsync();
            logger.LogInformation("✓ Product service: Total inventory value: {InventoryValue:C}", inventoryValue);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Product service operation failed");
        }

        // Test Customer Service
        try
        {
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Phone = "555-1234",
                DateOfBirth = new DateTime(1990, 5, 15),
                CreditLimit = 5000m,
                CurrentBalance = 0m
            };

            var createdCustomer = await customerService.CreateCustomerAsync(customer);
            logger.LogInformation("✓ Customer service: Created customer '{CustomerName}' with ID {CustomerId}", 
                createdCustomer.FullName, createdCustomer.Id);

            // Add a charge
            var chargedCustomer = await customerService.AddChargeAsync(createdCustomer.Id, 150m);
            logger.LogInformation("✓ Customer service: Added charge. New balance: {Balance:C}, Available credit: {AvailableCredit:C}", 
                chargedCustomer.CurrentBalance, chargedCustomer.AvailableCredit);

            // Process a payment
            var paidCustomer = await customerService.ProcessPaymentAsync(createdCustomer.Id, 50m);
            logger.LogInformation("✓ Customer service: Processed payment. New balance: {Balance:C}", paidCustomer.CurrentBalance);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ Customer service operation failed");
        }
    }

    private static async Task DemonstrateValidationFailures(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n4. Validation Failure Scenarios");
        Console.WriteLine("--------------------------------");

        using var scope = services.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        logger.LogInformation("Testing various validation failure scenarios...");

        // Product validation failures
        var invalidProducts = new[]
        {
            new Product { Name = "", Price = 10m, Quantity = 5, Category = "Test" }, // Empty name
            new Product { Name = "Valid Name", Price = -5m, Quantity = 5, Category = "Test" }, // Negative price
            new Product { Name = "Valid Name", Price = 10m, Quantity = -1, Category = "Test" }, // Negative quantity
            new Product { Name = "Valid Name", Price = 200000m, Quantity = 5, Category = "Test" }, // Unreasonable price
        };

        foreach (var product in invalidProducts)
        {
            try
            {
                await productService.CreateProductAsync(product);
                logger.LogWarning("⚠ Expected validation failure for product with Price={Price}, Quantity={Quantity}, Name='{Name}'", 
                    product.Price, product.Quantity, product.Name);
            }
            catch (Repositories.Implementations.ValidationException ex)
            {
                logger.LogInformation("✓ Product validation correctly failed: {FailedRules}", 
                    string.Join(", ", ex.FailedRules));
            }
            catch (Exception ex)
            {
                logger.LogInformation("✓ Product validation failed with business rule: {Message}", ex.Message);
            }
        }

        // Customer validation failures
        var invalidCustomers = new[]
        {
            new Customer { FirstName = "", LastName = "Doe", Email = "test@example.com", DateOfBirth = DateTime.Now.AddYears(-25), CreditLimit = 1000m }, // Empty first name
            new Customer { FirstName = "John", LastName = "Doe", Email = "invalid-email", DateOfBirth = DateTime.Now.AddYears(-25), CreditLimit = 1000m }, // Invalid email
            new Customer { FirstName = "John", LastName = "Doe", Email = "test@example.com", DateOfBirth = DateTime.Now.AddYears(-10), CreditLimit = 1000m }, // Too young
            new Customer { FirstName = "John", LastName = "Doe", Email = "test@example.com", DateOfBirth = DateTime.Now.AddYears(-25), CreditLimit = -1000m }, // Negative credit limit
        };

        foreach (var customer in invalidCustomers)
        {
            try
            {
                await customerService.CreateCustomerAsync(customer);
                logger.LogWarning("⚠ Expected validation failure for customer with Email='{Email}', Age={Age}, CreditLimit={CreditLimit}", 
                    customer.Email, customer.Age, customer.CreditLimit);
            }
            catch (Repositories.Implementations.ValidationException ex)
            {
                logger.LogInformation("✓ Customer validation correctly failed: {FailedRules}", 
                    string.Join(", ", ex.FailedRules));
            }
            catch (Exception ex)
            {
                logger.LogInformation("✓ Customer validation failed with business rule: {Message}", ex.Message);
            }
        }

        logger.LogInformation("Validation failure demonstration completed");
    }
}
