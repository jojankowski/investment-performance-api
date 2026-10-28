namespace InvestmentPerformance.Api.Models.Entities;

public class User
{
	public int Id { get; set; }
	public required string Name { get; set; }
	public List<Investment> Investments { get; set; } = [];
}
