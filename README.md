# Aerodrome Sniper

A Windows desktop terminal for trading on Aerodrome Finance (Base).
It shows new pools the moment they appear, mirrors wallets you follow,
and keeps balances, positions and fills in one window.

Built this because I got tired of keeping three browser tabs open: one with
new pools, one with a whale wallet, one with my own positions. Now its one
window.

## What it does

- **Sniper** — live feed of new Aerodrome pools with liquidity, age and quote
  filters, one-click buy, and auto-snipe rules with take-profit / stop-loss.
- **Copy** — add any Base wallet and the app mirrors its Aerodrome swaps,
  either a fixed amount or a percent of the traded size.
- **Portfolio** — balances, open positions with live P&L, and the full trade
  history with side/source/token filters.
- **Dashboard** — equity curve, day and total P&L, recent activity.

Market data comes from public APIs: GeckoTerminal for pools and prices,
Blockscout for wallet activity, Base RPC for chain state. No API keys needed.

## Install

Grab the latest build from the Releases page and unpack it anywhere.
Windows 10 or 11 with the .NET 8 Desktop Runtime is all it asks for.

## Build from source

```bash
git clone https://github.com/codberg18/aerodrome-sniper.git
cd aerodrome-sniper/src
dotnet build
dotnet run --project AerodromeSniper
```

## Where things live

- Poll intervals, API endpoints, slippage and fee defaults: Settings page.
- Account, positions, history, watchlist and rules: `%AppData%\AerodromeSniper`.
- Logs: `%LocalAppData%\AerodromeSniper\logs`.

## Screenshots

![dashboard](img/dashboard.png)

![sniper](img/sniper.png)

## Some tips

- start with the liqudity filter at $10k or higher, below that most new
  Aerodrome pools are noise that dies within the hour
- for copy trading pick wallets that do 3-10 trades a day; whoever trades
  a hundred times a day is farming something else and you will eat fees
- keep default slippage at 1% for pools above $100k liquidity, thin pools
  need more but price impact eats you anyway
- the +50 / -25 take-profit and stop-loss pair is my default for snipes,
  adjust it per token, not per mood

## Known issues

- The new-pool feed polls GeckoTerminal on an interval; when their free tier
  answers with 429 the cycle is skipped and the next one picks up.
- Copy tracking reads the public Blockscout API. Wallets that push hundreds
  of transfers a minute can lag a block or two behind.
- Maximized window keeps a small padding on some DPI scales, a side effect
  of the custom chrome.

## License

MIT

## Donate

If this saved you some time, ETH on Base works:
`0x4b7Ea2d1C0f39e8B6a5D2e9F1c8B7a6E5d4C3b2A`
