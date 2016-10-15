using BlockchainHub.BlockExplorer.Models;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockchainHub.BlockExplorer.Controllers
{
	public class MainController : Controller
	{
		QBitNinjaClient _QBit;
		QBitNinjaClient QBit
		{
			get
			{
				if(_QBit == null && Request?.Url != null)
				{
					_QBit = new QBitNinjaClient(Network.Main);
					if(Request.Url.Host.StartsWith("tbtc"))
						_QBit = new QBitNinjaClient(Network.TestNet);
				}
				return _QBit;
			}
		}
		public MainController()
		{
		}


		[Route("")]
		public async Task<ActionResult> Index(string search = null, int count = 5)
		{
			count = Math.Max(0, count);
			count = Math.Min(50, count);
			if(!string.IsNullOrWhiteSpace(search))
				return View();

			var responses =
				await Task.WhenAll(Enumerable
				.Range(0, count)
				.Select(i => QBit.GetBlock(new BlockFeature(SpecialFeature.Last) { Offset = -i }, true, true))
				.ToArray());

			var model = new MainModel();
			model.NextCount = count + 5;
			foreach(var response in responses.Where(r => r.ExtendedInformation != null && r.AdditionalInformation != null))
			{
				var blockModel = new MainBlockModel();
				blockModel.Hash = response.AdditionalInformation.BlockId;
				blockModel.Height = response.AdditionalInformation.Height;
				blockModel.Size = ToKB(response.ExtendedInformation.Size);
				blockModel.Time = ToRelative(response.AdditionalInformation.BlockTime);
				blockModel.TransactionsCount = response.ExtendedInformation.TransactionCount;
				blockModel.Fees = ToString(response.ExtendedInformation.BlockReward - response.ExtendedInformation.BlockSubsidy);
				model.Blocks.Add(blockModel);
			}
			return View(model);
		}

		private string ToRelative(DateTimeOffset time)
		{
			var ago = DateTimeOffset.UtcNow - time;
			bool negative = ago < TimeSpan.Zero;
			ago = negative ? -ago : ago;
			string result;
			if(ago.TotalMinutes < 1.0)
			{
				result = (int)(ago.TotalSeconds) + " seconds ago";
			}
			else if(ago.TotalHours < 1.0)
			{
				result =(int)(ago.TotalMinutes) + " minutes ago";
			}
			else
			{
				result = (int)(ago.TotalHours) + " h, " + (ago.Minutes) + " min ago";
			}
			result = negative ? "-" + result : result;
			return result;
		}

		private string ToKB(int size)
		{
			decimal kb = (decimal)size / 1024.0m;
			kb = decimal.Round(kb, 3);
			return kb + " kB";
		}

		[Route("addresses")]
		public ActionResult Address()
		{
			return View();
		}

		[Route("blocks/{blockFeature}", Name = "Block")]
		public async Task<ActionResult> Block(BlockFeature blockFeature, int count = 5)
		{
			count = Math.Max(0, count);
			var block = await QBit.GetBlock(blockFeature, false, true);
			count = Math.Min(block.Block.Transactions.Count, count);
			var transactions = await Task.WhenAll(Enumerable
				.Range(0, count)
				.Select(ii => QBit.GetTransaction(block.Block.Transactions[ii].GetHash()))
				.ToArray());

			BlockModel model = new BlockModel()
			{
				NextCount = count == block.Block.Transactions.Count ? 0 : Math.Min(block.Block.Transactions.Count, count + 5),
				BlockTime = block.AdditionalInformation.BlockTime,
				Confirmations = block.AdditionalInformation.Confirmations,
				Difficulty = block.AdditionalInformation.BlockHeader.Bits.Difficulty,
				Fee = ToString(block.ExtendedInformation.BlockReward - block.ExtendedInformation.BlockSubsidy),
				Hash = block.AdditionalInformation.BlockId,
				Height = block.AdditionalInformation.Height,
				MerkleRoot = block.AdditionalInformation.BlockHeader.HashMerkleRoot,
				Previous = block.AdditionalInformation.BlockHeader.HashPrevBlock,
				Size = ToKB(block.ExtendedInformation.Size),
				StrippedSize = ToKB(block.ExtendedInformation.StrippedSize),
				Version = block.AdditionalInformation.BlockHeader.Version,
				TransactionCount = block.Block.Transactions.Count
			};

			int i = 0;
			foreach(var tx in transactions)
			{
				BlockTransactionModel txModel = new BlockTransactionModel()
				{
					Amount = ToString(tx.ReceivedCoins.Select(c => (Money)c.Amount).Sum()),
					Fee = ToString(tx.Fees),
					Hash = tx.TransactionId,
					IsCoinbase = i == 0,
					Index = i
				};

				txModel.Inputs = ToParts(tx.SpentCoins);
				txModel.Outputs = ToParts(tx.ReceivedCoins);

				model.Transactions.Add(txModel);
				i++;
			}
			return View(model);
		}

		private List<BlockTransactionPartModel> ToParts(List<ICoin> coins)
		{
			return coins
				.Select(c => ToPart(c))
				.ToList();
		}

		private BlockTransactionPartModel ToPart(ICoin c)
		{
			return new BlockTransactionPartModel()
			{
				Amount = ToString((Money)c.Amount),
				ScriptPubKey = GetFriendlyScriptPubKey(c.TxOut.ScriptPubKey)
			};
		}

		private string GetFriendlyScriptPubKey(Script scriptPubKey)
		{
			var address = scriptPubKey.GetDestinationAddress(QBit.Network);
			if(address != null)
				return address.ToString();
			return scriptPubKey.ToString();
		}

		private string ToString(Money money)
		{
			var amount = money.ToUnit(MoneyUnit.BTC);
			CultureInfo culture = CultureInfo.CurrentCulture;
			var number = new NumberFormatInfo();
			number.CurrencyDecimalDigits = 8;
			number.CurrencySymbol = "BTC";
			number.CurrencyDecimalSeparator = culture.NumberFormat.CurrencyDecimalSeparator;
			number.CurrencyGroupSeparator = culture.NumberFormat.CurrencyGroupSeparator;
			number.CurrencyGroupSizes = culture.NumberFormat.CurrencyGroupSizes;
			number.CurrencyNegativePattern = 8; //culture.NumberFormat.CurrencyNegativePattern;
			number.CurrencyPositivePattern = 3; //culture.NumberFormat.CurrencyPositivePattern;
			number.NegativeSign = culture.NumberFormat.NegativeSign;
			return amount.ToString("C", number);
		}

		public int GetSize(IBitcoinSerializable data, TransactionOptions options)
		{
			var bms = new BitcoinStream(Stream.Null, true);
			bms.TransactionOptions = options;
			data.ReadWrite(bms);
			return (int)bms.Counter.WrittenBytes;
		}

		[Route("transactions/{txId}")]
		[Route("tx/{txId}")]
		public async Task<ActionResult> Transaction(uint256 txId)
		{
			var tx = await QBit.GetTransaction(txId);

			var size = GetSize(tx.Transaction, TransactionOptions.All);
			
			var model = new TransactionModel()
			{
				BlockHeight = tx.Block?.Height,
				Confirmations = tx.Block?.Confirmations ?? 0,
				Fee = ToString(tx.Fees),
				Size = ToKB(size),
				StrippedSize = ToKB(GetSize(tx.Transaction, TransactionOptions.None)),
				FeeRate = ToSatoshiPerBytes(new FeeRate(tx.Fees, size)),
				Hash = tx.TransactionId,
				SeenDate = tx.FirstSeen,
				Version = (int)tx.Transaction.Version,
				InputAmount = ToString(tx.SpentCoins.Select(m=>(Money)m.Amount).Sum()),
				OutputAmount = ToString(tx.ReceivedCoins.Select(m => (Money)m.Amount).Sum()),
				Inputs = ToParts(tx.SpentCoins),
				Outputs = ToParts(tx.ReceivedCoins)
		};
			return View(model);
		}

		private string ToSatoshiPerBytes(FeeRate feeRate)
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			var perByte = feeRate.GetFee(1);
			return perByte.Satoshi.ToString(culture) + " Sat/Byte";
		}
	}
}