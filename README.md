# EMBRS
EMBRS is a platform on the XRP Ledger for game developers to provide a "Play-To-Earn" mechanic within their games

## Features

- Simulate an EMBRS payout with a valid SteamId
- Do a test EMBRS payout with a valid SteamId and XRP Address
- Register a new developer via Steam AppId
- Register a new player via player's SteamId and XRP Address

## Requirements

- [Visual Studio 2019 or greater](https://visualstudio.microsoft.com/downloads/)

## Settings

- **Steam_ID**: The AppId of the test Steam game providing EMBRS rewards
- **Test_Player**: The SteamId or Steam vanity URL of player to test rewards system out (only needs the nickname and not the full URL)
- **Test_Address**: The XRPL address to test rewards system out
- **Web_API_Key**: The Steam publisher Web API key - see [WebAPI Overview](https://partner.steamgames.com/doc/webapi_overview/auth)
- **WebSocket_URL**: Main Net: 	wss://s1.ripple.com/  wss://xrplcluster.com/  Test Net: wss://s.altnet.rippletest.net/
- **Reward_Address**: Address that holds the tokens for rewards
- **Reward_Address_Secret**: Secret to the rewards address. KEEP THIS PRIVATE AND SAFE!
- **Currency_Code**: Ticker symbol of EMBRS token (do not change)
- **Issuer_Address**: Address that issued the tokens for rewards (do not change)
- **TransferFee**: Usually not applicable. Leave at 0 if unsure. TransferRate of your token in %, must be between 0 and 100
- **Reward_Token_Amt**: Amount to send in each rewards txn
- **AccountLinesThrottle**: Number of seconds between request calls. Recommended not to change. Lower settings could result in a block from web service hosts
- **TxnThrottle**: Number of seconds between request calls. Recommended not to change. Lower settings could result in a block from web service hosts.
- **FeeMultiplier**: How many times above average fees to pay for reliable transactions
- **MaximumFee**: Maximum number of drops willing to pay for each transaction

## Getting Started

- Ensure that the config/settings.json file is completed filled out (this normally would attach to the main XRP address providing EMBRS rewards, but can work on any address holding EMBRS provided Rewards_Address and Rewards_Address_Secret are correct)
- Run the project in Visual Studio 2019
- Option 1 will simulate validating the Test_Player using Steamworks Web API to ensure they own Steam_ID and recently played it
- Option 2 completes the same testing, but will send Reward_Token_Amt to from Reward_Address to Test_Address
- Option 3 will request an AppId (not currently validated) and add as a new developer
- Option 4 will request a player's SteamId (not currently validated) and an XRP address (also not currently validated) and add as a new player
- Option 5 will exit
