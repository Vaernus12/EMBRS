using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Ripple.TxSigning;
using RippleDotNet;
using RippleDotNet.Model;
using RippleDotNet.Model.Account;
using RippleDotNet.Model.Transaction;
using RippleDotNet.Model.Transaction.Interfaces;
using RippleDotNet.Model.Transaction.TransactionTypes;
using RippleDotNet.Requests.Account;
using RippleDotNet.Requests.Transaction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EMBRS
{
    public static class XRPL
    {
        public static async Task SendRewardAsync(SocketSlashCommand command, SocketUser sourceUser, SocketUser destinationUser, string amount, 
                                                 bool reward = false, bool tip = false, bool faucet = false)
        {
            try
            {
                IRippleClient client = new RippleClient(Settings.WebSocketUrl);
                client.Connect();
                uint sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                var f = await client.Fees();

                while (Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier)) > Settings.MaximumFee)
                {
                    Console.WriteLine("Waiting...fees too high. Current Open Ledger Fee: " + f.Drops.OpenLedgerFee);
                    Console.WriteLine("Fees configured based on fee multiplier: " + Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier)));
                    Console.WriteLine("Maximum Fee Configured: " + Settings.MaximumFee);
                    Thread.Sleep(Settings.AccountLinesThrottle * 1000);
                    f = await client.Fees();
                }

                int feeInDrops = Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier));

                var response = await XRPL.SendXRPPaymentAsync(client, Database.RegisteredUsers[destinationUser.Id].XrpAddress, sequence, feeInDrops, amount, Settings.TransferFee);

                //Transaction Node isn't Current. Wait for Network
                if (response.EngineResult == "noCurrent" || response.EngineResult == "noNetwork")
                {
                    int retry = 0;
                    while ((response.EngineResult == "noCurrent" || response.EngineResult == "noNetwork") && retry < 3)
                    {
                        //Throttle for node to catch up
                        Thread.Sleep(Settings.TxnThrottle * 3000);
                        response = await XRPL.SendXRPPaymentAsync(client, Database.RegisteredUsers[destinationUser.Id].XrpAddress, sequence, feeInDrops, amount, Settings.TransferFee);
                        retry++;

                        if ((response.EngineResult == "noCurrent" || response.EngineResult == "noNetwork") && retry == 3)
                        {
                            await command.FollowupAsync("XRP network isn't responding. Please try again later!", ephemeral: true);
                            if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                            else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                            else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
                        }
                    }
                }
                else if (response.EngineResult == "tefPAST_SEQ")
                {
                    //Get new account sequence + try again
                    sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                    await command.FollowupAsync("Please try again!", ephemeral: true);
                    if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                    else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                    else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
                }
                else if (response.EngineResult == "telCAN_NOT_QUEUE_FEE")
                {
                    sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                    //Throttle, check fees and try again
                    Thread.Sleep(Settings.TxnThrottle * 3000);
                    await command.FollowupAsync("Please try again!", ephemeral: true);
                    if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                    else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                    else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
                }
                else if (response.EngineResult == "tesSUCCESS" || response.EngineResult == "terQUEUED")
                {
                    //Transaction Accepted by node successfully.
                    sequence++;

                    if (tip)
                    {
                        var userInfo = command.User;
                        await command.FollowupAsync($"**{sourceUser.Username}#{sourceUser.Discriminator} sent {destinationUser.Username}#{destinationUser.Discriminator} a tip of {amount} EMBRS!**");
                    }
                    else if (faucet)
                    {
                        var userInfo = command.User;
                        await command.FollowupAsync($"Faucet payout of " + amount + " EMBRS complete!", ephemeral: true);
                    }
                    else
                    {
                        var userInfo = command.User;
                        await command.FollowupAsync($"Tournament reward of " + amount + " EMBRS complete! Congratulations!", ephemeral: true);
                    }
                }
                else if (response.EngineResult == "tecPATH_DRY" || response.EngineResult == "tecDST_TAG_NEEDED")
                {
                    //Trustline was removed or Destination Tag needed for address
                    await command.FollowupAsync("EMBRS trustline is not set!", ephemeral: true);
                    if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                    else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                    else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
                    sequence++;
                }
                else
                {
                    //Failed
                    await command.FollowupAsync("EMBRS transaction failed!", ephemeral: true);
                    if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                    else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                    else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
                    sequence++;
                }

                client.Disconnect();
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                if (reward) Database.RegisteredUsers[destinationUser.Id].ReceivedTournamentReward = false;
                else if (tip) Database.RegisteredUsers[destinationUser.Id].LastTipTime = Database.RegisteredUsers[destinationUser.Id].LastTipTime.AddMinutes(-1);
                else if (faucet) Database.RegisteredUsers[destinationUser.Id].LastFaucetTime = Database.RegisteredUsers[destinationUser.Id].LastFaucetTime.AddHours(-24);
            }
        }

        public static async Task<Submit> SendXRPPaymentAsync(IRippleClient client, string destinationAddress, uint sequence, int feeInDrops, string amount, decimal transferFee = 0)
        {
            try
            {
                IPaymentTransaction paymentTransaction = new PaymentTransaction
                {
                    Account = Settings.RewardAddress,
                    Destination = destinationAddress,
                    Amount = new Currency { CurrencyCode = Settings.CurrencyCode, Issuer = Settings.IssuerAddress, Value = amount },
                    Sequence = sequence,
                    Fee = new Currency { CurrencyCode = "XRP", ValueAsNumber = feeInDrops }
                };
                if (transferFee > 0)
                {
                    paymentTransaction.SendMax = new Currency { CurrencyCode = Settings.CurrencyCode, Issuer = Settings.IssuerAddress, Value = (amount + (Convert.ToDecimal(amount) * (transferFee / 100))).ToString() };
                }

                TxSigner signer = TxSigner.FromSecret(Settings.RewardSecret);  //secret is not sent to server, offline signing only
                SignedTx signedTx = signer.SignJson(JObject.Parse(paymentTransaction.ToJson()));

                SubmitBlobRequest request = new SubmitBlobRequest()
                {
                    TransactionBlob = signedTx.TxBlob
                };

                Submit result = await client.SubmitTransactionBlob(request);

                return result;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                throw new Exception(ex.Message);
            }
        }

        public static async Task<uint> GetLatestAccountSequence(IRippleClient client, string account)
        {
            try
            {
                AccountInfo accountInfo = await client.AccountInfo(account);
                return accountInfo.AccountData.Sequence;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<BookOfferReturnObj> GetBookOffers(IRippleClient client, string from, string to)
        {
            BookOfferReturnObj returnObj = new();

            try
            {
                Currency fromCurrency = null;
                Currency toCurrency = null;

                if ((from.ToLower() == "embrs" || from.ToLower() == "embers" ||
                     to.ToLower() == "embrs" || to.ToLower() == "embers") &&
                     (from.ToLower() == "usd" || to.ToLower() == "usd")) // MUST USE INDIRECT METHOD UNTIL ORDER BOOK EXISTS IN FUTURE
                {
                    var indirectResult = await GetBookOffers(client, "usd", "xrp");
                    var indirectMidPrice = indirectResult.Midprice;

                    fromCurrency = new Currency
                    {
                        CurrencyCode = Settings.CurrencyCode,
                        Issuer = Settings.IssuerAddress
                    };

                    toCurrency = new Currency();

                    BookOffersRequest request1 = new()
                    {
                        TakerGets = fromCurrency,
                        TakerPays = toCurrency
                    };

                    BookOffersRequest request2 = new()
                    {
                        TakerGets = toCurrency,
                        TakerPays = fromCurrency
                    };

                    var offers = await client.BookOffers(request1);
                    Thread.Sleep(Settings.TxnThrottle * 1000);
                    var offers2 = await client.BookOffers(request2);

                    decimal? lowestBid = 100000;
                    for (int i = offers.Offers.Count - 1; i > 0; i--)
                    {
                        var value = offers.Offers[i].TakerPays.ValueAsXrp / offers.Offers[i].TakerGets.ValueAsNumber;
                        if (value < lowestBid) lowestBid = value;
                    }

                    decimal? highestAsk = 0;
                    for (int i = 0; i < offers2.Offers.Count; i++)
                    {
                        var value = offers2.Offers[i].TakerGets.ValueAsXrp / offers2.Offers[i].TakerPays.ValueAsNumber;
                        if (value > highestAsk) highestAsk = value;
                    }

                    var midPrice = ((lowestBid) + (highestAsk)) / 2;
                    returnObj.Midprice = midPrice / indirectMidPrice;
                }
                else
                {
                    if (from.ToLower() == "embrs" || from.ToLower() == "embers" ||
                        to.ToLower() == "embrs" || to.ToLower() == "embers")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.CurrencyCode,
                            Issuer = Settings.IssuerAddress
                        };

                        if (from.ToLower() == "xrp" || to.ToLower() == "xrp") // EMBRS/XRP
                        {
                            toCurrency = new Currency();
                        }
                        else if (from.ToLower() == "usd" || to.ToLower() == "usd") // EMBRS/USD
                        {
                            toCurrency = new Currency
                            {
                                CurrencyCode = Settings.USDCurrencyCode,
                                Issuer = Settings.USDIssuerAddress
                            };
                        }
                    }
                    else if (from.ToLower() == "stx" || to.ToLower() == "stx")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.STXCurrencyCode,
                            Issuer = Settings.STXIssuerAddress
                        };

                        if (from.ToLower() == "xrp" || to.ToLower() == "xrp") // STX/XRP
                        {
                            toCurrency = new Currency();
                        }
                    }
                    else if (from.ToLower() == "usd" || to.ToLower() == "usd")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.USDCurrencyCode,
                            Issuer = Settings.USDIssuerAddress
                        };

                        if (from.ToLower() == "xrp" || to.ToLower() == "xrp") // USD/XRP
                        {
                            toCurrency = new Currency();
                        }
                    }

                    BookOffersRequest request1 = new()
                    {
                        TakerGets = fromCurrency,
                        TakerPays = toCurrency
                    };

                    BookOffersRequest request2 = new()
                    {
                        TakerGets = toCurrency,
                        TakerPays = fromCurrency
                    };

                    var offers = await client.BookOffers(request1);
                    Thread.Sleep(Settings.TxnThrottle * 1000);
                    var offers2 = await client.BookOffers(request2);

                    decimal? lowestBid = 100000;
                    for (int i = offers.Offers.Count - 1; i > 0; i--)
                    {
                        var value = offers.Offers[i].TakerPays.ValueAsXrp / offers.Offers[i].TakerGets.ValueAsNumber;
                        if (value < lowestBid) lowestBid = value;
                    }

                    decimal? highestAsk = 0;
                    for (int i = 0; i < offers2.Offers.Count; i++)
                    {
                        var value = offers2.Offers[i].TakerGets.ValueAsXrp / offers2.Offers[i].TakerPays.ValueAsNumber;
                        if (value > highestAsk) highestAsk = value;
                    }

                    var midPrice = ((lowestBid) + (highestAsk)) / 2;
                    returnObj.Midprice = midPrice;
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }

            return returnObj;
        }

        public struct BookOfferReturnObj
        {
            public decimal? Midprice { get; set; }
        }

        public static async Task<decimal> ReturnAccountBalance(IRippleClient client, string account)
        {
            try
            {
                AccountInfo accountInfo = await client.AccountInfo(account);
                return accountInfo.AccountData.Balance.ValueAsXrp.HasValue ? accountInfo.AccountData.Balance.ValueAsXrp.Value : 0;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return 0;
            }
        }

        public static async Task<TrustLineReturnObj> ReturnTrustLines(IRippleClient client, string userAddress, string marker)
        {
            TrustLineReturnObj returnObj = new TrustLineReturnObj();
            AccountLinesRequest req = new AccountLinesRequest(userAddress);

            req.Limit = 400;
            if (marker != "")
            {
                req.Marker = marker;
            }

            AccountLines accountLines = await client.AccountLines(req);
            if (accountLines.Marker != null)
            {
                marker = accountLines.Marker.ToString();
            }
            else
            {
                marker = "";
            }
            
            foreach (TrustLine line in accountLines.TrustLines)
            {
                if (line.Currency == Settings.CurrencyCode)
                {
                    try
                    {
                        returnObj.EMBRSBalance = Convert.ToDecimal(line.Balance);
                    }
                    catch (Exception ex)
                    {
                        returnObj.EMBRSBalance = 0;
                        await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                    }
                }
                else if (line.Currency == Settings.STXCurrencyCode)
                {
                    try
                    {
                        returnObj.STXBalance = Convert.ToDecimal(line.Balance);
                    }
                    catch (Exception ex)
                    {
                        returnObj.STXBalance = 0;
                        await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                    }
                }
                else if (line.Currency == Settings.USDCurrencyCode)
                {
                    try
                    {
                        returnObj.USDBalance = Convert.ToDecimal(line.Balance);
                    }
                    catch (Exception ex)
                    {
                        returnObj.USDBalance = 0;
                        await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                    }
                }
            }

            returnObj.Marker = marker;
            return returnObj;
        }

        public struct TrustLineReturnObj
        {
            public decimal EMBRSBalance { get; set; }
            public decimal STXBalance { get; set; }
            public decimal USDBalance { get; set; }
            public string Marker { get; set; }
        }
    }
}
