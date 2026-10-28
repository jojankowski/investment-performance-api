using InvestmentPerformance.Api.Models.Entities;

namespace InvestmentPerformance.Api.Data;

// Purchase dates count back from today and the local database is rebuilt on each start.
// That keeps short, long and mixed terms in the demo at all times. Ids are fixed to keep example URLs working.
public static class SeedData
{
    public static List<User> Create(DateOnly today)
    {
        var soup = new Security { Symbol = "SOUP", Name = "Big SOUP Incorporated", AssetType = AssetType.Stock, CurrentPrice = 230.50m };
        var bean = new Security { Symbol = "BEAN", Name = "Coffee Co.", AssetType = AssetType.Stock, CurrentPrice = 415.20m };
        var boats = new Security { Symbol = "BRUS", Name = "Boats-R-Us", AssetType = AssetType.Stock, CurrentPrice = 9.99m };
        var duck = new Security { Symbol = "DUCK", Name = "Rubber Duck Holdings", AssetType = AssetType.Stock, CurrentPrice = 3.75m };
        var rainyDay = new Security { Symbol = "RAIN", Name = "Rainy Day 10 Year Bond", AssetType = AssetType.Bond, CurrentPrice = 98.75m };
        var piggyBank = new Security { Symbol = "OINK", Name = "Piggy Bank Total Market Fund", AssetType = AssetType.MutualFund, CurrentPrice = 142.85m };

        return
        [
            new User
            {
                Id = 1,
                Name = "Test User 1",
                Investments =
                [
                    // Mixed term: one lot is long term and the other is short term.
                    new Investment
                    {
                        Id = 1,
                        Security = soup,
                        Lots =
                        [
                            new Lot { Shares = 10, CostBasisPerShare = 150m, PurchaseDate = today.AddYears(-2) },
                            new Lot { Shares = 5, CostBasisPerShare = 210m, PurchaseDate = today.AddMonths(-3) }
                        ]
                    },
                    new Investment
                    {
                        Id = 2,
                        Security = rainyDay,
                        Lots = [new Lot { Shares = 20, CostBasisPerShare = 101.50m, PurchaseDate = today.AddMonths(-18) }]
                    },
                    new Investment
                    {
                        Id = 3,
                        Security = piggyBank,
                        Lots = [new Lot { Shares = 12.5m, CostBasisPerShare = 130m, PurchaseDate = today.AddMonths(-6) }]
                    }
                ]
            },
            new User
            {
                Id = 2,
                Name = "Test User 2",
                Investments =
                [
                    // Bought exactly one year ago. It stays short term until tomorrow.
                    new Investment
                    {
                        Id = 4,
                        Security = bean,
                        Lots = [new Lot { Shares = 8, CostBasisPerShare = 430m, PurchaseDate = today.AddYears(-1) }]
                    }
                ]
            },
            // No investments.
            new User { Id = 3, Name = "Test User 3" },
            new User
            {
                Id = 4,
                Name = "Test User 4",
                Investments =
                [
                    // Both lots at a loss.
                    new Investment
                    {
                        Id = 5,
                        Security = boats,
                        Lots =
                        [
                            new Lot { Shares = 100, CostBasisPerShare = 12m, PurchaseDate = today.AddYears(-3) },
                            new Lot { Shares = 50, CostBasisPerShare = 18.25m, PurchaseDate = today.AddMonths(-1) }
                        ]
                    },
                    // A long-held gain.
                    new Investment
                    {
                        Id = 6,
                        Security = duck,
                        Lots = [new Lot { Shares = 1000, CostBasisPerShare = 0.50m, PurchaseDate = today.AddYears(-6) }]
                    }
                ]
            },
            new User
            {
                Id = 5,
                Name = "Test User 5",
                Investments =
                [
                    // A short-term gain.
                    new Investment
                    {
                        Id = 7,
                        Security = bean,
                        Lots = [new Lot { Shares = 10, CostBasisPerShare = 380m, PurchaseDate = today.AddMonths(-2) }]
                    }
                ]
            }
        ];
    }
}