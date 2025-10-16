namespace Review.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; } // Внешний ключ на продукт
    public Guid UserId { get; set; } // Внешний ключ на пользователя
    public string UserName { get; set; } = string.Empty; // Денормализованные данные
    public int Rating { get; set; } // 1-5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
}

// Агрегат для статистики отзывов
public class ProductReviewsSummary : BaseEntity
{
    public Guid ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
}

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}