using Microsoft.EntityFrameworkCore;
using Recipe.Domain;

namespace Recipe.Infrastructure;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Recipe> Recipes => Set<Domain.Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ingredient>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Domain.Recipe>()
            .HasMany(r => r.RecipeIngredients)
            .WithOne()
            .HasForeignKey(ri => ri.RecipeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecipeIngredient>()
            .Property(ri => ri.AmountUnit)
            .HasConversion<string>();
    }
}
