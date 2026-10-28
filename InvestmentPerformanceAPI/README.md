# Investment Performance API

Small .NET 10 web API for the investment performance exercise.

## How to run

You need the .NET 10 SDK, and Node.js 20.19+ or 22.12+ for the optional Vue page. Commands below are run from the `InvestmentPerformanceAPI` folder.

```
cd src/InvestmentPerformance.Api
dotnet run
```

Then open http://localhost:5219/scalar/v1 in your browser (Visual Studio opens it for you). You can hit the endpoints from there. If you're in Visual Studio or Rider, there's also `InvestmentPerformance.Api.http` in the project folder with requests for the cases I tested.

There's also a Vue page in `src/InvestmentPerformance.Web`. With the API running, open a second terminal:

```
cd src/InvestmentPerformance.Web
npm install
npm run dev
```

Then go to http://localhost:5173.

Tests: `dotnet test`

## Endpoints

```
GET /api/users/{userId}/investments
GET /api/users/{userId}/investments/{investmentId}
```

The first one gives you the id and name of each investment. The second gives you the details: shares, cost basis per share, current price, current value, term and total gain/loss. It also lists each purchase separately.

An unknown user or investment is a 404, and an id below 1 is a 400. Unexpected errors are logged and come back as a 500 without any internal details. All errors use the standard problem details format.

Each lookup writes one structured log line (user and investment ids as named fields), and unexpected errors are logged with the full exception. Requests rejected with a 400 never reach the lookup. Log levels are set in `appsettings.json`. Logs go to the console for now; in production they'd be shipped to a central log platform.

## Test data

It uses SQLite, and locally the database gets wiped and re-seeded every time the app starts, so the dates are always relative to today. Some users to try (investment ids in brackets):

- 1 has a few investments (1, 2, 3), and investment 1 was bought twice
- 2 bought something exactly a year ago, which should still be short term (4)
- 3 has nothing
- 4 is down on everything except the duck stock (5, 6)
- 5 has a short-term gain (7)

## Assumptions

A user can buy the same thing more than once, so I store each purchase as a lot. That changes a few things:

- Cost basis per share is a weighted average (total paid / total shares), not an average of the prices.
- Term is per lot. If an investment has both short and long term lots, it shows as `Mixed`. The spec only mentions short and long, so this was my call.

Other stuff:

- "A year or less" is short term, using today's date in UTC.
- Money is rounded to cents and per-share prices to 4 decimal places. Each lot is rounded on its own, so the lot gains can be a cent off the total.
- Cost basis is just the purchase price. Fees and commissions aren't included.
- Bonds and mutual funds use the same shares times price math as stocks. Real bond pricing (face value, accrued interest) is out of scope.
- Current prices live in the database. They're behind an interface so a real price feed could be swapped in.
- I used SQLite so it runs with just `dotnet run` and nothing to install. I know SQL Server is preferred. Switching is a package swap, `UseSqlServer` in `Program.cs` and a new connection string in `appsettings.json`. Nothing else in the app code is SQLite-specific. The integration tests use an in-memory SQLite database.
- Demo data is only seeded into an empty database, and outside Development nothing is ever deleted. A real deployment wouldn't seed demo data at all.
- There's no auth, so the user id just comes from the URL. If you ask for someone else's investment you get a 404 instead of a 403, so you can't tell which ids exist.
- I only handled buys. Sells would need something like FIFO to decide which lots get sold.
- There's no HTTPS redirect in the app. In production TLS would normally end at a load balancer or proxy in front of it.
- The Vue page is a small demo for browsing the API. It has no tests of its own.

## Things I'd do next

- Auth (take the user id from the token instead of the URL)
- A real price feed, with caching and timeouts
- SQL Server and proper migrations instead of `EnsureCreated`
- Health checks
- Split the API into separate Domain, Application and Infrastructure projects as it grows
- A CI pipeline that builds and runs the tests on every pull request