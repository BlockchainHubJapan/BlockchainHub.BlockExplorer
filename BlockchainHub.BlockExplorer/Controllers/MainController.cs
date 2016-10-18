using Gma.QrCodeNet.Encoding.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using BlockchainHub.BlockExplorer.Models;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NBitcoin.DataEncoders;

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
			{
				search = search.Trim();
				if(search.StartsWith("0x") || search.Contains("OP_"))
				{
					return RedirectToAction("Address", new
					{
						address = search
					});
				}
				try
				{
					BitcoinAddress.Create(search, QBit.Network);
					return RedirectToAction("Address", new
					{
						address = search
					});
				}
				catch { }

				if(search.Length == 32 * 2)
				{
					if(search.StartsWith("0000000000"))
					{
						return RedirectToAction("Block", new
						{
							blockFeature = search
						});
					}
					else
					{
						return RedirectToAction("Transaction", new
						{
							txId = search
						});
					}
				}

				try
				{

					BlockFeature.Parse(search);
					return RedirectToAction("Block", new
					{
						blockFeature = search
					});
				}
				catch { }
				return View();
			}

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
				result = (int)(ago.TotalMinutes) + " minutes ago";
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
		[Route("a/{address}")]
		public async Task<ActionResult> Address(string address, int count = 10)
		{
			string nice = address;
			var balanceSelector = new BalanceSelector(address);
			Script script = null;
			if(nice.StartsWith("0x"))
			{
				script = new Script(Encoders.Hex.DecodeData(nice.Substring(2, nice.Length - 2)));
			}
			if(address.Contains("OP_"))
			{
				script = new Script(address);
				balanceSelector = new BalanceSelector(script);
			}
			nice = script?.GetDestinationAddress(QBit.Network)?.ToString() ?? script?.ToString() ?? nice;
			var balances = QBit.GetBalance(balanceSelector);
			var summary = QBit.GetBalanceSummary(balanceSelector);
			await Task.WhenAll(balances, summary);
			var model = new AddressModel()
			{
				Address = nice,
				NextCount = count + 10,
				ConfirmedBalance = ToString(summary.Result.Confirmed.Amount),
				TotalReceived = ToString(summary.Result.Spendable.Received),
				TransactionCount = summary.Result.Spendable.TransactionCount,
				UnconfirmedBalance = ToString(summary.Result.UnConfirmed.Amount),
			};
			model.Transactions = GetBlockTransactions(balances.Result.Operations.ToArray());
			return View(model);
		}

		private List<BlockTransactionModel> GetBlockTransactions(BalanceOperation[] operations)
		{
			var txModels = new List<BlockTransactionModel>();
			int i = 0;
			foreach(var op in operations)
			{
				BlockTransactionModel txModel = new BlockTransactionModel()
				{
					Amount = ToString(op.Amount),
					Hash = op.TransactionId,
					IsCoinbase = i == 0,
					Index = i,
					Income = op.Amount >= Money.Zero
				};

				txModel.Inputs = ToParts(op.SpentCoins);
				txModel.Outputs = ToParts(op.ReceivedCoins);

				txModels.Add(txModel);
				i++;
			}

			return txModels;
		}

		[Route("blocks/{blockFeature}")]
		[Route("b/{blockFeature}", Name = "Block")]
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

			var txModels = GetBlockTransactions(transactions);
			model.Transactions = txModels;
			return View(model);
		}

		[Route("qr/{bitcoinAddress}")]
		public ActionResult GetAddressQR(string bitcoinAddress)
		{
			BitcoinAddress.Create(bitcoinAddress); //anti ddos
			QrCodeImgControl control = new QrCodeImgControl();
			control.QuietZoneModule = Gma.QrCodeNet.Encoding.Windows.Render.QuietZoneModules.Zero;
			control.ErrorCorrectLevel = Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H;
			control.BackColor = System.Drawing.Color.White;
			control.Width = 200;
			control.Height = 200;
			control.Text = bitcoinAddress.ToString();
			Bitmap bitmap = new Bitmap(control.Width, control.Height);
			control.DrawToBitmap(bitmap, new Rectangle(0, 0, control.Width, control.Height));
			var bytes = ImageToByte2(bitmap);
			return this.File(bytes, "image/jpeg");
		}
		public static byte[] ImageToByte2(Bitmap img)
		{
			byte[] byteArray = new byte[0];
			using(MemoryStream stream = new MemoryStream())
			{
				img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Close();

				byteArray = stream.ToArray();
			}
			return byteArray;
		}

		private List<BlockTransactionModel> GetBlockTransactions(GetTransactionResponse[] transactions)
		{
			var txModels = new List<BlockTransactionModel>();
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

				txModels.Add(txModel);
				i++;
			}

			return txModels;
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
		[Route("t/{txId}")]
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
				InputAmount = ToString(tx.SpentCoins.Select(m => (Money)m.Amount).Sum()),
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