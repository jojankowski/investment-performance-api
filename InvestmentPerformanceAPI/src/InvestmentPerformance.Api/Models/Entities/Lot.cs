namespace InvestmentPerformance.Api.Models.Entities;

public class Lot
{
	public int Id { get; set; }
	public int InvestmentId { get; set; }
	public decimal Shares { get; set; }
	public decimal CostBasisPerShare { get; set; }
	public DateOnly PurchaseDate { get; set; }
}
