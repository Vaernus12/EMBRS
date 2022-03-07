using Newtonsoft.Json.Linq;
using Ripple.TxSigning;
using RippleDotNet;
using RippleDotNet.Model;
using RippleDotNet.Model.Account;
using RippleDotNet.Model.Transaction;
using RippleDotNet.Model.Transaction.Interfaces;
using RippleDotNet.Model.Transaction.TransactionTypes;
using RippleDotNet.Requests.Transaction;
using System;
using System.Threading.Tasks;

namespace EMBRS
{
    public static class XRPL
    {
        public static async Task<Submit> SendXRPPaymentAsync(IRippleClient client, string destinationAddress, uint sequence, int feeInDrops, decimal transferFee = 0)
        {
            try
            {
                IPaymentTransaction paymentTransaction = new PaymentTransaction
                {
                    Account = Settings.RewardAddress,
                    Destination = destinationAddress,
                    Amount = new Currency { CurrencyCode = Settings.CurrencyCode, Issuer = Settings.IssuerAddress, Value = Settings.RewardTokenAmt },
                    Sequence = sequence,
                    Fee = new Currency { CurrencyCode = "XRP", ValueAsNumber = feeInDrops }
                };
                if (transferFee > 0)
                {
                    paymentTransaction.SendMax = new Currency { CurrencyCode = Settings.CurrencyCode, Issuer = Settings.IssuerAddress, Value = (Settings.RewardTokenAmt + (Convert.ToDecimal(Settings.RewardTokenAmt) * (transferFee / 100))).ToString() };
                }

                TxSigner signer = TxSigner.FromSecret(Settings.RewardSecret);  //secret is not sent to server, offline signing only
                SignedTx signedTx = signer.SignJson(JObject.Parse(paymentTransaction.ToJson()));

                SubmitBlobRequest request = new()
                {
                    TransactionBlob = signedTx.TxBlob
                };

                Submit result = await client.SubmitTransactionBlob(request);

                return result;
            }
            catch (Exception ex)
            {
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
    }
}
