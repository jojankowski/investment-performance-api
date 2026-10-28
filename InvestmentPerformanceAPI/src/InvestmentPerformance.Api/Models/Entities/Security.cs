namespace InvestmentPerformance.Api.Models.Entities;

public class Security
{
	public int Id { get; set; }
	public required string Symbol { get; set; }
	public required string Name { get; set; }
	public AssetType AssetType { get; set; }
	public decimal CurrentPrice { get; set; }
}
