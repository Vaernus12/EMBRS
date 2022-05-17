# EMBRS Forged
EMBRS Forged is a platform on the XRP Ledger for game developers to provide a "Play-To-Earn" mechanic within their games

## Features

- Simulate an incoming JSON message
- Simulate an EMBRS payout with a valid SteamId
- Do a test EMBRS payout with a valid SteamId and XRP Address
- Register a new developer via Steam AppId
- Register a new player via player's SteamId and XRP Address

## Roadmap

- [EMBRS Roadmap](https://emberlight.quarteroniongames.com/platform/)

## Requirements

- [Visual Studio 2019 or greater](https://visualstudio.microsoft.com/downloads/)

## Settings

- **Developer**: Developer of the rewards-based test game
- **SteamGame**: Name of the rewards-based test game
- **SteamAppID**: Steam AppId of the rewards-based test game
- **TestPlayer**: Steam vanity URL of player to test rewards system out (only needs the nickname and not the full URL)
- **TestAddress**: XRPL address to test rewards system
- **WebAPIKey**: Steam publisher Web API key - see [WebAPI Overview](https://partner.steamgames.com/doc/webapi_overview/auth)
- **AzureString**: Azure connection string to access developer blobs holding game data or patches
- **GameFilesLocation**: Location containing test game's current files or patch
- **WebSocketURL**: Main Net: 	wss://s1.ripple.com/  wss://xrplcluster.com/  Test Net: wss://s.altnet.rippletest.net/
- **RewardAddress**: Address that holds the tokens for rewards
- **RewardSecret**: Secret to the rewards address. KEEP THIS PRIVATE AND SAFE!
- **CurrencyCode**: Ticker symbol of EMBRS token (do not change)
- **IssuerAddress**: Address that issued the tokens for rewards (do not change)
- **TransferFee**: Usually not applicable. Leave at 0 if unsure. TransferRate of your token in %, must be between 0 and 100
- **RewardTokenAmt**: Amount to send in each rewards txn
- **AccountLinesThrottle**: Number of seconds between request calls. Recommended not to change. Lower settings could result in a block from web service hosts
- **TxnThrottle**: Number of seconds between request calls. Recommended not to change. Lower settings could result in a block from web service hosts.
- **FeeMultiplier**: How many times above average fees to pay for reliable transactions
- **MaximumFee**: Maximum number of drops willing to pay for each transaction

## Getting Started

- Ensure that the config/settings.json file is completed filled out (this normally would attach to the main XRP address providing EMBRS rewards, but can work on any address holding EMBRS provided RewardsAddress and RewardsSecret are correct)
- Run the project in Visual Studio 2019
- Option 1 will simulate an incoming JSON message (received from a game) and validates against public/private key
- Option 2 will simulate validating the TestPlayer using Steamworks Web API to ensure they own SteamAppID and recently played it
- Option 3 completes the same testing, but will send RewardTokenAmt to from RewardAddress to TestAddress
- Option 4 will request an AppId (not currently validated) and add as a new developer
- Option 5 will request a player's SteamId (not currently validated) and an XRP address (also not currently validated) and add as a new player
- Option 6 will exit
